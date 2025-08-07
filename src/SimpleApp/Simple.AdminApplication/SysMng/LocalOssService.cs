using Mapster;
using Simple.AdminApplication.SysMng.Config;
using Simple.AdminApplication.SysMng.Entities;
using Simple.Interfaces;
using Simple.Interfaces.Dtos;
using Simple.Utils.Exceptions;
using Simple.Utils.Extensions;
using Simple.Utils.Models.Dto;

namespace Simple.AdminApplication.SysMng
{
    [Scoped]
    public class LocalOssService:IOSSService
    {
        private readonly OssConfig _ossConfig;
        private FileDocumentService _fileDocumentService;
        private IWebHostEnvironment _webHostEnvironment;

        public LocalOssService(FileDocumentService fileDocumentService, IWebHostEnvironment webHostEnvironment)
        {
            _ossConfig = ConfigHelper.GetValue<OssConfig>("LocalOss");
            _fileDocumentService = fileDocumentService;
            _webHostEnvironment = webHostEnvironment;

            // 转换 SaveFolder 为绝对路径
            if (!Path.IsPathRooted(_ossConfig.SaveFolder))
            {
                var path = PathCombine(_webHostEnvironment.WebRootPath, _ossConfig.SaveFolder);
                _ossConfig.SaveFolder = Path.GetFullPath(path);
            }

            Directory.CreateDirectory(_ossConfig.SaveFolder);
        }

        /// <summary>
        /// 启动时拷贝默认文件
        /// </summary>
        /// <returns></returns>
        public async Task CopyAllFilesFromDefault()
        {
            var sourceDirectory = PathCombine(_webHostEnvironment.WebRootPath, "files", "default");
            var destinationDirectory = PathCombine(_ossConfig.SaveFolder, "default");
            sourceDirectory = Path.GetFullPath(sourceDirectory);
            destinationDirectory = Path.GetFullPath(destinationDirectory);

            // 确保目标目录存在
            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            // 获取源目录中的所有文件
            var files = Directory.GetFiles(sourceDirectory);

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var destFilePath = PathCombine(destinationDirectory, fileName);

                var id = Path.GetFileNameWithoutExtension(file).AsGuid();
                var dbData = await _fileDocumentService.FindAsync(id);
                if (dbData == null)
                {
                    // 拷贝文件
                    if (!Equals(sourceDirectory, destinationDirectory))
                    {
                        File.Copy(file, destFilePath, overwrite: true);
                    }

                    // 保存文件信息到数据库
                    var fileInfo = new SysFileDocument
                    {
                        Id = id,
                        Path = PathHelper.GetRelativePath(_ossConfig.SaveFolder, destFilePath),
                        Name = fileName,
                        Ext = Path.GetExtension(fileName),
                        BucketName = "default",
                        StoreType = "local" // 根据需要设置存储类型
                    };
                    await _fileDocumentService.Save(fileInfo, true);
                }
            }
        }

        private void BeforeSaveFile(string ext, long length)
        {
            if (!string.IsNullOrEmpty(ext) && !_ossConfig.AllowExtensions.Contains(ext.Trim('.')))
            {
                throw new CustomException("该文件类型不允许上传");
            }

            if (_ossConfig.MaxSizeLimit != 0 && length > _ossConfig.MaxSizeLimit)
            {
                throw new CustomException("文件大小超过存储上限");
            }
        }

        /// <summary>
        /// 目录是否存在
        /// </summary>
        /// <param name="bucketName"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> BucketExistsAsync(string bucketName)
        {
            var path = PathCombine(_ossConfig.SaveFolder, bucketName);
            var isExists = Directory.Exists(path);
            return await Task.FromResult(isExists);
        }

        public async Task<bool> CreateBucketAsync(string bucketName)
        {
            var path = PathCombine(_ossConfig.SaveFolder, bucketName);
            Directory.CreateDirectory(path);
            return await Task.FromResult(true);
        }

        public string GetFullPath(string retivePath)
        {
            var fullPath = PathCombine(_ossConfig.SaveFolder, retivePath);
            return fullPath;
        }

        public string PathCombine(params string[] paths)
        {
            if (paths == null || paths.Length == 0)
                return string.Empty;

            var combinedPath = paths[0].Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
            for (int i = 1; i < paths.Length; i++)
            {
                combinedPath = Path.Combine(combinedPath,
                    paths[i].Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar));
            }

            return combinedPath;
        }

        public async Task<FileDocumentDto> GetObjectAsync(Guid documentId,
            CancellationToken cancellationToken = default)
        {
            var srcfileInfo = await _fileDocumentService.FindAsync(documentId);
            if (srcfileInfo == null)
            {
                throw new CustomException("文件不存在");
            }

            var srcPath = PathCombine(_ossConfig.SaveFolder, srcfileInfo.Path);

            var dto = srcfileInfo.Adapt<FileDocumentDto>();
            dto.FullPath = srcPath;
            return dto;
        }

        public async Task<bool> ObjectsExistsAsync(string bucketName, string objectName)
        {
            var srcfileInfo = await _fileDocumentService.FindAsync(objectName.AsGuid());
            if (srcfileInfo == null)
            {
                return false;
            }

            var srcPath = PathCombine(_ossConfig.SaveFolder, bucketName, srcfileInfo.Path);
            return File.Exists(srcPath);
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="bucketName">存储桶名称</param>
        /// <param name="documentId">文件Id，可不传递，传递后则是覆盖源文件模式</param>
        /// <param name="fileName">文件名，必传，需要校验是否允许上传</param>
        /// <param name="data">文件字节流</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SysFileDocumentDto> PutObjectAsync(string bucketName, Guid? documentId, string fileName,
            Stream data, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new CustomException("文件名不能为空");
            }

            var ext = Path.GetExtension(fileName);

            BeforeSaveFile(ext, data.Length);

            // 有传递id则先删后插
            if (documentId != null)
            {
                await _fileDocumentService.DeleteAsync(documentId.Value);
            }
            else
            {
                documentId = Guid.NewGuid();
            }

            var relativePath = string.IsNullOrEmpty(_ossConfig.FileFolderPrefix)
                ? Path.Combine(bucketName, documentId.ToString())
                : Path.Combine(bucketName, DateTime.Now.ToString(_ossConfig.FileFolderPrefix),
                    documentId + ext);
            var fullPath = PathCombine(_ossConfig.SaveFolder, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // 将流数据写入文件
            await using (var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
            {
                if (data.CanSeek)
                {
                    data.Seek(0, SeekOrigin.Begin); // 重置流位置到开头
                }

                await data.CopyToAsync(fileStream, cancellationToken);
            }

            // 保存文件信息到数据库
            var docInfo = new SysFileDocument
            {
                Id = documentId.Value,
                Path = relativePath,
                Name = fileName,
                Ext = Path.GetExtension(fileName),
                StoreType = "local" // 根据需要设置存储类型
            };
            await _fileDocumentService.Save(docInfo, true);

            var dto = docInfo.Adapt<SysFileDocumentDto>();
            
            return dto;
        }

        public async Task<bool> RemoveBucketAsync(string bucketName)
        {
            var path = PathCombine(_ossConfig.SaveFolder, bucketName);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            return await Task.FromResult(true);
        }

        public async Task<bool> RemoveObjectAsync(string bucketName, string objectName)
        {
            var documentId = objectName.AsGuid();
            await _fileDocumentService.DeleteAsync(documentId);
            return true;
        }

        public async Task<bool> RemoveObjectAsync(string bucketName, List<string> objectNames)
        {
            var documentIds = objectNames.Select(t => t.AsGuid()).ToArray();
            await _fileDocumentService.DeleteByIds(documentIds);
            return true;
        }
    }
}
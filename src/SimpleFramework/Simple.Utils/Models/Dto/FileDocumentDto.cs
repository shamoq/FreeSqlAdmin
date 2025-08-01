using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Utils.Models.Dto
{
    public class FileDocumentDto 
    {
        /// <summary>
        /// 文档Id
        /// </summary>
        public Guid DocumentId { get; set; }

        /// <summary>
        /// 文档名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 文件全路径
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string FullPath { get; set; }

        /// <summary>
        /// 文件相对路径
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string Path { get; set; }
        
        /// <summary>
        /// 创建人GUID
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public Guid? CreatedId { get; set; }

        /// <summary>创建时间</summary>
        public DateTime? CreatedTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 创建人
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string Creator { get; set; }

        /// <summary>更新时间</summary>
        public DateTime? UpdatedTime { get; set; }

        /// <summary>
        /// 更新人GUID
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public Guid? UpdatedId { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string Updator { get; set; }

        public async Task<byte[]> FileBytes()
        {
            return await File.ReadAllBytesAsync(Path);
        }

        public Stream GetFileStream()
        {
            try
            {
                // 检查文件是否存在
                if (!File.Exists(FullPath))
                {
                    throw new FileNotFoundException($"文件 {FullPath} 未找到。");
                }

                // 打开文件并返回文件流
                return new FileStream(FullPath, FileMode.Open, FileAccess.Read);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
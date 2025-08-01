using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Simple.Interfaces.Dtos;
using Simple.Utils.Models.Dto;

namespace Simple.Interfaces
{
    /// <summary>
    /// Oss服务接口
    /// </summary>
    public interface IOSSService
    {
        Task<SysFileDocumentDto> PutObjectAsync(string bucketName, Guid? documentId, string fileName,
            Stream data, CancellationToken cancellationToken = default);

        string GetFullPath(string retivePath);

        string PathCombine(params string[] paths);

        Task<FileDocumentDto> GetObjectAsync(Guid documentId, CancellationToken cancellationToken = default);
    }
}
using Simple.Utils.Models.Dto;

namespace Simple.Interfaces;

public interface IFileDocumentService
{
    /// <summary>
    /// 获取文件信息
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<FileDocumentDto> GetById(Guid id);
}
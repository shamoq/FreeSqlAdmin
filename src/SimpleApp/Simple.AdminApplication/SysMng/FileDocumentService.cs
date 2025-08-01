using FreeSql;
using Mapster;
using Simple.AdminApplication.Common;
using Simple.AdminApplication.SysMng.Entities;
using Simple.FreeSql;
using Simple.Interfaces;
using Simple.Utils.Models.Dto;

namespace Simple.AdminApplication.SysMng;

[Scoped]
public class FileDocumentService : BaseCurdService<SysFileDocument>
{
    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="documentIds"></param>
    /// <returns></returns>
    public async Task DeleteByIds(Guid[] documentIds)
    {
        await table.DeleteAsync(x => documentIds.Contains(x.Id));
    }

    public async Task<FileDocumentDto> GetById(Guid id)
    {
        var document = await table.Where(x => x.Id == id).ToOneAsync();
        if (document != null)
        {
            return document.Adapt<FileDocumentDto>();
        }

        return null;
    }
}
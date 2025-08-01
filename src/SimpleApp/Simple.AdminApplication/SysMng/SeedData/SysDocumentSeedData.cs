using Simple.AdminApplication.Common;
using Simple.AdminApplication.SysMng.Entities;
using Simple.Utils.Consts;

namespace Simple.AdminApplication.SysMng.SeedData;

public class SysDocumentSeedData
{
    public void Init(IFreeSql db)
    {
        //if (db.SysFileDocument.Any(t => t.Id == AppConsts.EmptyDocumentId))
        //{
        //    return;
        //}

        //// 保存文件信息到数据库
        //var fileInfo = new SysFileDocument
        //{
        //    Id = AppConsts.EmptyDocumentId,
        //    Path = "default/empty.docx",
        //    Name = "新文档.docx",
        //    Ext = "docx",
        //    BucketName = "default",
        //    StoreType = "local" // 根据需要设置存储类型
        //};

        //db.SysFileDocument.Add(fileInfo);
    }
}
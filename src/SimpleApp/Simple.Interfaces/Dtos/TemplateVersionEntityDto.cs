using Simple.Utils.Models;

namespace Simple.Interfaces.Dtos;

public class TemplateVersionEntityDto : InputId
{
    /// <summary>
    /// 业务数据主键Id
    /// </summary>
    public Guid BusinessId { get; set; }

    ///<summary>
    /// PDF文档guid（小程序需要）
    ///</summary>
    public virtual Guid? PdfFileId { get; set; }

    ///<summary>
    /// 图片文档guid（小程序需要）
    ///</summary>
    public virtual Guid? ImgFileId { get; set; }

    ///<summary>
    /// 文件Id
    ///</summary>
    public virtual Guid? FileId { get; set; }

    ///<summary>
    /// 文件名称，不带后缀名称的，使用时需要自己增加后缀
    /// 原因是会作为pdf，word，图片的名称
    ///</summary>
    public virtual string FileName { get; set; }

    /// <summary>
    /// 图片字段
    /// </summary>
    public virtual string Img4Pc { get; set; }

    ///<summary>
    /// 文件大小
    ///</summary>
    public virtual int? Size { get; set; }

    ///<summary>
    /// 当前文件版本号
    ///</summary>
    public virtual int? Version { get; set; } = 1;

    /// <summary>
    /// 文件路径
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// 富文本Html
    /// </summary>
    public string UmoHtmlContent { get; set; }

    /// <summary>
    /// 富文本Json
    /// </summary>
    public string UmoJsonContent { get; set; }

    /// <summary>
    /// 富文本Text
    /// </summary>
    public string UmoTextContent { get; set; }

    /// <summary>
    /// 最后使用的模板类型
    /// </summary>
    public string TemplateType { get; set; }

    /// <summary>
    ///
    /// </summary>
    public int IsLock { get; set; }
    
    /// <summary>创建时间</summary>
    public DateTime? CreatedTime { get; set; }
    
    /// <summary>
    /// 创建人GUID
    /// </summary>
    public Guid? CreatedId { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string Creator { get; set; }

    /// <summary>更新时间</summary>
    public DateTime? UpdatedTime { get; set; }

    /// <summary>
    /// 更新人GUID
    /// </summary>
    public Guid? UpdatedId { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string Updator { get; set; }
}
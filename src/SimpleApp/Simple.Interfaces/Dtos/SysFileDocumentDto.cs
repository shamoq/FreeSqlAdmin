using Simple.Utils.Models;

namespace Simple.Interfaces.Dtos;

public class SysFileDocumentDto : InputId
{
    /// <summary>
    /// 文件名
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 文件路径
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// 后缀
    /// </summary>
    public string Ext { get; set; }

    /// <summary>
    /// 存储类型
    /// </summary>
    public string StoreType { get; set; }

    /// <summary>
    /// 存储桶
    /// </summary>
    public string BucketName { get; set; }
}
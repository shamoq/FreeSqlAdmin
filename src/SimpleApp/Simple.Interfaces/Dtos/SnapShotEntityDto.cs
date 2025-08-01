using Simple.Utils.Models;

namespace Simple.Interfaces.Dtos;

public class SnapShotEntityDto : InputId
{
    /// <summary>
    /// 数据类型
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// 数据值
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// 数据hash值
    /// </summary>
    public string Hash { get; set; }

    /// <summary>
    /// 业务Id
    /// </summary>
    public Guid? BusinessId { get; set; }

    /// <summary>
    /// 版本
    /// </summary>
    public int Version { get; set; }
}
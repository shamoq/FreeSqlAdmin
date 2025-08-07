namespace Simple.Utils.Models.Entity;

public interface ITreeEntity
{
    /// <summary>
    /// 主键
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 父级ID
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 全名称
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// 排序编码
    /// </summary>
    public string OrderCode { get; set; }

    /// <summary>
    /// 排序编码
    /// </summary>
    public string OrderFullCode { get; set; }
}
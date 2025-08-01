namespace Simple.Utils.Models.Entity;

/// <summary>
/// 树形实体
/// </summary>
public class TreeEntity : DefaultEntity
{
    /// <summary>
    /// 父级ID
    /// </summary>
    public virtual Guid? ParentId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public virtual string Name { get; set; }

    /// <summary>
    /// 全名称
    /// </summary>
    public virtual string FullName { get; set; }

    /// <summary>
    /// 排序编码
    /// </summary>
    public virtual string OrderCode { get; set; }

    /// <summary>
    /// 排序编码
    /// </summary>
    public virtual string OrderFullCode { get; set; }
}
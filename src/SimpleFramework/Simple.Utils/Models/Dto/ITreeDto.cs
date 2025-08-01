namespace Simple.Utils.Models.Dto;

/// <summary>
/// 树结构实体
/// </summary>
public interface ITreeDto
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
}
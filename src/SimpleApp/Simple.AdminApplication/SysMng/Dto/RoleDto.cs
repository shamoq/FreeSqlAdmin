using Simple.Utils.Models.Dto;

namespace Simple.AdminApplication.SysMng.Dto;

public class RoleDto : BaseDto, ITreeDto
{
    /// <summary>
    /// 主键
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 角色名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 角色全名称
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// 父级Id
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; }
}
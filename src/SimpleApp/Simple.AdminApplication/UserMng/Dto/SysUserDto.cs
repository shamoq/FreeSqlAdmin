using Simple.Utils.Models.Dto;

namespace Simple.AdminApplication.UserMng.Dto;

public class SysUserDto : BaseDto
{
    /// <summary>
    /// 主键
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>组织机构Id</summary>
    public Guid OrgId { get; set; }

    /// <summary>账号</summary>
    public string UserCode { get; set; }

    /// <summary>名称</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>头像</summary>
    public string Avatar { get; set; }

    /// <summary>联系电话</summary>
    public string Phone { get; set; }

    /// <summary>电子邮箱</summary>
    public string Email { get; set; }

    /// <summary>是否是超级管理员</summary>
    public int IsAdmin { get; set; } = 0;

    /// <summary>
    /// 是否启用
    /// </summary>
    public int IsEnable { get; set; }

    /// <summary>账户角色</summary>
    public Guid? RoleId { get; set; }

    /// <summary>
    /// 上次登录时间
    /// </summary>
    public DateTime? LastLoginTime { get; set; }

    /// <summary>
    /// 上次登录IP
    /// </summary>
    public string LastIpAdress { get; set; }

    /// <summary>组织名称</summary>
    public string OrgnizationName { get; set; }

    /// <summary>组织名称</summary>
    public string OrgnizationFullName { get; set; }

    /// <summary>
    /// 角色名称
    /// </summary>
    public string RoleName { get; set; }
}
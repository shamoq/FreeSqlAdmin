namespace Simple.AdminApplication.UserMng.Dto;

public class RoleGrantRightDto
{
    /// <summary>
    /// 导航编码
    /// </summary>
    public string NavCode { get; set; }

    /// <summary>
    /// 导航名称
    /// </summary>
    public string NavName { get; set; }

    /// <summary>
    /// 菜单编码
    /// </summary>
    public string MenuCode { get; set; }

    /// <summary>
    /// 菜单名称
    /// </summary>
    public string MenuName { get; set; }

    /// <summary>
    /// 动作点
    /// </summary>
    public string ActionCode { get; set; }

    /// <summary>
    /// 动作名称
    /// </summary>
    public string ActionName { get; set; }

    /// <summary>
    /// 系统编码
    /// </summary>
    public string Application { get; set; }
    
    /// <summary>
    /// 系统名称
    /// </summary>
    public string ApplicationName { get; set; }
}
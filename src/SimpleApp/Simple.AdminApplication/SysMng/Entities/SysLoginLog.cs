namespace Simple.AdminApplication.SysMng.Entities;

/// <summary>
/// 登陆日志
/// </summary>
public class SysLoginLog : DefaultTenantEntity
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; }
    
    /// <summary>
    /// 用户编码
    /// </summary>
    public string UserCode { get; set; }

    /// <summary>
    /// 登录时间
    /// </summary>
    public DateTime LoginTime { get; set; } 

    /// <summary>
    /// 登录IP地址
    /// </summary>
    public string LoginIp { get; set; }

    /// <summary>
    /// 登录设备
    /// </summary>
    public string Device { get; set; }

    /// <summary>
    /// 浏览器信息
    /// </summary>
    public string Browser { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string ErrorMsg { get; set; }

    /// <summary>
    /// 登录设备分辨率
    /// </summary>
    public string ScreenResolution { get; set; }

    /// <summary>
    /// 会话ID
    /// </summary>
    public Guid? SessionId { get; set; }
}
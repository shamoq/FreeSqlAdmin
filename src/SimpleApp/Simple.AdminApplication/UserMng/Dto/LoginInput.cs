namespace Simple.AdminApplication.UserMng.Dto;

public class LoginInput
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string UserCode { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// 租户编码
    /// </summary>
    public string TenantCode { get; set; }
}
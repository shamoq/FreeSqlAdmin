namespace Simple.AdminApplication.UserMng.Dto;

public class TenantGrantInput
{
    /// <summary>
    /// 租户套餐Id
    /// </summary>
    public Guid TenantPackageId { get; set; }

    /// <summary>
    /// 授权码
    /// </summary>
    public List<string> ActionCodes { get; set; }
}


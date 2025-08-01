using Simple.AdminApplication.TenantMng.Entities;
using Simple.Utils.Models.BO;

namespace Simple.AdminApplication.TenantMng.Dto;

public class TenantContext
{
    private static AsyncLocal<Tenant> _currentTenant = new AsyncLocal<Tenant>();
    private static AsyncLocal<LoginUserBO> _currUser = new AsyncLocal<LoginUserBO>();

    /// <summary>
    /// 获取当前租户信息
    /// </summary>
    public static Tenant CurrentTenant
    {
        get => _currentTenant.Value;
    }

    /// <summary>
    /// 获取当前用户信息
    /// </summary>
    public static LoginUserBO CurrentUser
    {
        get => _currUser.Value;
    }
    
    public static void SetCurrentTenant(Tenant tenant)
    {
        _currentTenant.Value = tenant;
    }

    public static void ClearTenant()
    {
        _currentTenant.Value = null;
    }

    public static void SetCurrentUser(LoginUserBO user)
    {
        _currUser.Value = user;
    }
}
namespace Simple.AdminApplication.Extensions;

public class TenantTypeExtension
{
    /// <summary>
    /// 获取多租户管理实体类型
    /// </summary>
    /// <returns></returns>
    public static Type[] GetTenantManageTypes()
    {
        var tenantManageTypes = RuntimeHelper.GetAllTypes(type =>
            type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(DefaultEntity)) 
            ).ToArray();
        return tenantManageTypes;
    }
    
    /// <summary>
    /// 获取所有租户类型
    /// </summary>
    /// <returns></returns>
    public static Type[] GetTenantTypes()
    {
        var tenantTypes = RuntimeHelper.GetAllTypes(type =>
            type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(DefaultTenantEntity))).ToArray();
        return tenantTypes;
    }
}
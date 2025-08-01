using Simple.AdminApplication.TenantMng.Entities;
using Simple.Utils.Consts;

namespace Simple.AdminApplication.Extensions;

public static class ConfigurationExtension
{
    public static TenantDataSource GetManagerDataSource(this IConfiguration _configuration)
    {
        // 注册多租户后台数据库
        var dataSource = new TenantDataSource()
        {
            Id = AppConsts.TenantManagerId, Name = "默认数据库", DbType = "mysql",
            ConnectionString = _configuration.GetConnectionString("tenantDb")
        };
        return dataSource;
    }
    
    public static Tenant GetManagerTenant(this IConfiguration _configuration)
    {
        return new Tenant()
        {
            Id = AppConsts.TenantManagerId, Code = "tenant", Name = "租户管理后台",
            DataSourceId = AppConsts.TenantManagerId
        };
    }
}
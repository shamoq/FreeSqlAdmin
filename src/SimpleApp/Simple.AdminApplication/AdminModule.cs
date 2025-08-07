using System.Diagnostics;
using FreeSql;
using FreeSql.Aop;
using Mapster;
using Simple.AdminApplication.Common;
using Simple.AdminApplication.Helpers;
using Simple.AdminApplication.MapProfiles;
using Simple.AdminApplication.TenantMng;
using Simple.AdminApplication.TenantMng.Dto;
using Simple.AdminApplication.TenantMng.Entities;
using Simple.AdminApplication.UserMng.SeedData;
using Simple.AspNetCore.Services;
using Simple.Utils.Consts;
using Simple.Utils.Generator;
using Simple.Utils.Generator.Dialect;
using System.Reflection;
using System.Runtime.Loader;
using Simple.AdminApplication.Extensions;
using Simple.AdminApplication.SysMng;

namespace Simple.AdminApplication
{
    public class AdminModule : SimpleModule
    {
        public override void ConfigServices(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            // 注册FreeSql配置
            FreeSqlCloud<string> fsql = new FreeSqlCloud<string>();
            FreeSqlHelper.RegisterTenantDb(fsql, configuration.GetManagerDataSource());
            serviceCollection.AddSingleton<IFreeSql>(fsql);

            // 注册Mapster配置
            MapProfile.Configure(TypeAdapterConfig.GlobalSettings);
            serviceCollection.AddSingleton(TypeAdapterConfig.GlobalSettings);
        }

        public override void AppFirstInit()
        {
            var configuration = HostServiceExtension.ServiceProvider.GetService<IConfiguration>();
            var tenantService = HostServiceExtension.ServiceProvider.GetService<TenantService>();
            var fsqlRaw = HostServiceExtension.ServiceProvider.GetService<IFreeSql>();
            var fsql = fsqlRaw as FreeSqlCloud<string>;

            // 同步多租户管理后台数据库结构
            var manageTypes = TenantTypeExtension.GetTenantManageTypes();
            fsql.CodeFirst.SyncStructure(manageTypes);

            // 同步租户数据库结构
            var tenantTypes = TenantTypeExtension.GetTenantTypes();
            var dataSources = fsql.Select<TenantDataSource>().ToList();
            foreach (var dataSource in dataSources)
            {
                tenantService.Register(dataSource, tenantTypes);
            }

            // 每个租户执行种子数据同步
            var tenants = fsql.Select<Tenant>().ToList();
            // 添加多租户管理的特殊租户
            tenants.Add(configuration.GetManagerTenant());
            foreach (var tenant in tenants)
            {
                TenantContext.SetCurrentTenant(tenant);
                try
                {
                    // 这里会自动切换数据库
                    tenantService.SeedData(tenant);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"租户{tenant.Code}种子数据同步失败：{e.Message}");
                }
                finally
                {
                    TenantContext.ClearTenant();
                }
            }
        }
    }
}
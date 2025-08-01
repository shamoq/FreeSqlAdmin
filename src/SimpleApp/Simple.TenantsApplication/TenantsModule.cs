using Mapster;
using Simple.Utils.Generator.Dialect;
using Simple.Utils.Generator;
using Simple.Utils.Models.Tenants;
using Simple.TenantsApplication.Entitys;
using Microsoft.EntityFrameworkCore;

namespace Simple.TenantsApplication
{
    public class TenantsModule : SimpleModule
    {
        public override void ConfigServices(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddDbContext<TenantDbContext>(options =>
            {
                // 多租户数据库
                var serverVersion = ServerVersion.AutoDetect(configuration.GetConnectionString("TenantDb"));
                options.UseMySql(configuration.GetConnectionString("TenantDb"), serverVersion);

                // 移除静态连接字符串配置，改为在DbContext中动态获取
                options.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
                    .EnableSensitiveDataLogging();
            });
        }

        public override void AppFirstInit()
        {
            using (var scope = HostServiceExtension.ServiceProvider.CreateScope())
            {
                // 获取DbContext实例
                var adminDb = scope.ServiceProvider.GetService<TenantDbContext>();
                var mysql = new MysqlDatabaseInfo(adminDb.Database.GetDbConnection());
                var hostEnv = scope.ServiceProvider.GetService<IHostEnvironment>();
                var autoUpdate = new DbGenerator(mysql, hostEnv.IsDevelopment());

                // 升级数据库
                autoUpdate.Upgrade(adminDb.Database.GetDbConnection(), typeof(DefaultEntity));

                ConsoleHelper.Debug($"多租户数据库初始化完成");
            }
        }
    }
}
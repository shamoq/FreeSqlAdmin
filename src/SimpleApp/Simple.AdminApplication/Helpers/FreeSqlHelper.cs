using System.Reflection;
using FreeSql;
using FreeSql.Aop;
using FreeSql.Internal.Model;
using Simple.AdminApplication.TenantMng.Dto;
using Simple.AdminApplication.TenantMng.Entities;
using Simple.Utils.Consts;
using Simple.Utils.Exceptions;
using Simple.Utils.Helper;

namespace Simple.AdminApplication.Helpers;

public class FreeSqlHelper
{
    /// <summary>
    /// 注册租户数据库
    /// </summary>
    /// <param name="fsql"></param>
    /// <param name="source"></param>
    /// <exception cref="Exception"></exception>
    public static void RegisterTenantDb(FreeSqlCloud<string> fsql, TenantDataSource source)
    {
        fsql.DistributeTrace = log => Console.WriteLine(log.Split('\n')[0].Trim());

        // 注册数据库
        fsql.Register(source.Id.ToString(), () =>
        {
            var dbType = TypeConvertHelper.ConvertType<DataType>(source.DbType);
            IFreeSql ifsql;

            var isDev = true;
            if (isDev)
            {
                var builder = new FreeSqlBuilder().UseConnectionString(dbType, source.ConnectionString);
                builder.UseNoneCommandParameter(true);
                ifsql = builder.Build();
            }
            else
            {
                var db = new FreeSqlBuilder().UseConnectionString(dbType, source.ConnectionString).Build();
                ifsql = db;
            }

            IFreeSqlConfig(ifsql);
            return ifsql;
        });

        // 拦截实体访问
        fsql.EntitySteering = (_, e) =>
        {
            // 拦截多租户管理实体，使用多租户管理数据库
            if (e.EntityType == typeof(Tenant) ||
                e.EntityType == typeof(TenantDataSource) ||
                e.EntityType == typeof(TenantPackage) ||
                e.EntityType == typeof(TenantPackageRight))
            {
                e.DBKey = AppConsts.TenantManagerId.ToString();
            }
            // 明确指定业务实体使用租户数据库
            else
            {
                // 强制使用当前租户的数据库
                if (TenantContext.CurrentTenant == null)
                {
                    throw new Exception("未获取到租户,无法访问数据");
                }
            }
        };

        // 注册多租户数据库配置
        void IFreeSqlConfig(IFreeSql ifsql)
        {
            // 拦截租户访问
            ifsql.GlobalFilter.ApplyIf<DefaultTenantEntity>(
                "TenantFilter", // 过滤器名称
                () =>
                {
                    if (TenantContext.CurrentTenant?.Id == Guid.Empty)
                    {
                        throw new Exception("未获取到租户");
                    }

                    return true;
                }, // 过滤器生效判断
                a => a.TenantId == TenantContext.CurrentTenant.Id // 过滤器条件
            );

            ifsql.Aop.CurdBefore += (s, e) =>
            {
                //if (e.Value is DefaultEntity entity)
                //{
                //    var now = DateTime.Now;
                //    switch (e.OperationType)
                //    {
                //        case FreeSql.Aop.CurdType.Insert:
                //            entity.CreatedTime = now;
                //            entity.UpdatedTime = now;
                //            break;
                //        case FreeSql.Aop.CurdType.Update:
                //            entity.UpdatedTime = now;
                //            break;
                //    }
                //}
            };

            // 新增SQL输出配置
            ifsql.Aop.CommandAfter += (_, e) =>
            {
                Console.WriteLine($"SQL: {e.Command.CommandText}");
                // if (e.Command.Parameters.Count > 0)
                // {
                //     Console.WriteLine("Parameters:");
                //     foreach (var param in e.Command.Parameters)
                //     {
                //         Console.WriteLine($"  {param.ParameterName}: {param.Value}");
                //     }
                // }
            };

            // 审批拦截
            ifsql.Aop.AuditValue += (_, e) =>
            {
                // 每个实体只审计一次
                e.ObjectAuditBreak = true;

                // 仅处理DefaultEntity及其子类
                if (e.Object is DefaultEntity entity)
                {
                    var userId = TenantContext.CurrentUser?.Id; // 获取当前用户ID
                    var userName = TenantContext.CurrentUser?.UserName; // 获取当前用户名

                    switch (e.AuditValueType)
                    {
                        case AuditValueType.Insert:
                            // 插入操作
                            entity.CreatedTime = DateTime.Now;
                            entity.UpdatedTime = DateTime.Now;
                            entity.CreatedId = userId;
                            entity.Creator = userName;
                            entity.UpdatedId = userId;
                            entity.Updator = userName;
                            break;

                        case AuditValueType.Update:
                            // 更新操作
                            entity.UpdatedTime = DateTime.Now;
                            entity.UpdatedId = userId;
                            entity.Updator = userName;
                            break;
                    }

                    // 更新枚举列的文本值
                    if (e.AuditValueType == AuditValueType.Insert || e.AuditValueType == AuditValueType.Update)
                    {
                        var properties = e.Object.GetType().GetProperties();
                        foreach (var property in properties)
                        {
                            // 获取枚举列
                            var enumColumnAttribute = property.GetCustomAttribute<EnumColumnAttribute>();
                            if (enumColumnAttribute != null)
                            {
                                // 获取枚举要存储的文本列
                                var enumType = enumColumnAttribute.EnumType;
                                var currentValue = property.GetValue(e.Object);
                                if (currentValue != null)
                                {
                                    var targetProperty = properties.FirstOrDefault(t =>
                                        t.Name.Equals(enumColumnAttribute.TextColumnName));
                                    if (targetProperty != null)
                                    {
                                        var description =
                                            StaticClassEnumHelper.GetDescription((int)currentValue, enumType);
                                        targetProperty.SetValue(e.Object, description);
                                    }
                                }
                                else
                                {
                                }
                            }
                        }
                    }
                }

                if (e.Object is DefaultTenantEntity tenantEntity)
                {
                    tenantEntity.TenantId = TenantContext.CurrentTenant.Id;
                }
            };
        }
    }
}
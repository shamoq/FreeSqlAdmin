using FreeSql;
using Simple.AdminApplication.Common;
using Simple.AdminApplication.Extensions;
using Simple.AdminApplication.Helpers;
using Simple.AdminApplication.SysMng;
using Simple.AdminApplication.TenantMng.Dto;
using Simple.AdminApplication.TenantMng.Entities;
using Simple.AdminApplication.TenantMng.Helper;
using Simple.AdminApplication.TenantMng.Interfaces;
using Simple.AdminApplication.UserMng.Dto;
using Simple.AdminApplication.UserMng.SeedData;
using Simple.FreeSql;
using Simple.Utils.Consts;
using Simple.Utils.Exceptions;

namespace Simple.AdminApplication.TenantMng
{
    [Scoped]
    public class TenantService : BaseCurdService<Tenant>, ITenantService
    {
        private new FreeSqlCloud<string> fsql;
        private IConfiguration _configuration;

        public TenantService(IFreeSql fsql, IConfiguration configuration)
        {
            this._configuration = configuration;
            this.fsql = fsql as FreeSqlCloud<string>;
        }

        /// <summary>
        /// 获取租户信息
        /// </summary>
        public async Task<Tenant> GetTenant(string tenantCode)
        {
            var manageTenant = _configuration.GetManagerTenant();
            if (tenantCode == manageTenant.Code)
            {
                return manageTenant;
            }
            else
            {
                var tenant = await fsql.Select<Tenant>()
                     .Where(p => p.Code == tenantCode)
                     .FirstAsync();
                return tenant;
            }
        }

        /// <summary>
        /// 获取租户信息
        /// </summary>
        public async Task<Tenant> GetTenant(Guid tenantId)
        {
            var manageTenant = _configuration.GetManagerTenant();
            if (tenantId == manageTenant.Id)
            {
                return manageTenant;
            }
            else
            {
                var tenant = await fsql.Select<Tenant>()
                     .Where(p => p.Id == tenantId)
                     .FirstAsync();
                return tenant;
            }
        }

        /// <summary>
        /// 注册租户数据库
        /// </summary>
        /// <param name="dataSource"></param>
        /// <param name="tenantTypes"></param>
        /// <exception cref="CustomException"></exception>
        public void Register(TenantDataSource dataSource, Type[] tenantTypes)
        {
            var isRegister = fsql.ExistsRegister(dataSource.Id.ToString());
            if (isRegister)
            {
                return;
            }

            // 同步数据结构
            try
            {
                FreeSqlHelper.RegisterTenantDb(fsql, dataSource);
                fsql.Change(dataSource.Id.ToString());
                // fsql.CodeFirst.SyncStructure(tenantTypes);
            }
            catch (Exception e)
            {
                Console.WriteLine(dataSource.Remark + " 同步数据库失败:" + e.Message);
            }
        }

        /// <summary>
        ///  切换租户,调用此方法需要慎重，如果还有后续业务操作，是否需要将租户切换回来
        /// </summary>
        /// <param name="tenant"></param>
        public void ChangeTenant(Tenant tenant)
        {
            fsql.Change(tenant.DataSourceId.ToString());
        }

        /// <summary>
        /// 程序启动，执行租户种子数据，种子数据支持反复执行
        /// </summary>
        /// <param name="tenant"></param>
        public void SeedData(Tenant tenant)
        {
            ChangeTenant(tenant);

            new SysOrgSeedData().Init(fsql);
            new SysUserSeedData().Init(fsql);
        }

        public override async Task<Tenant> Save(Tenant entity, bool isForceAdd = false)
        {
            var isAdd = entity.Id == Guid.Empty;
            if (entity.DataSourceId == Guid.Empty)
            {
                entity.DataSourceId = AppConsts.TenantManagerId;
            }

            var retEntity = await base.Save(entity, isForceAdd);
            if (isAdd)
            {
                SeedData(retEntity);
            }

            return retEntity;
        }

        public async Task<List<TenantGrantRightDto>> GetTenantGrantArray()
        {
            var manageTenant = _configuration.GetManagerTenant();
            if (TenantContext.CurrentTenant.Id == manageTenant.Id)
            {
                List<TenantGrantRightDto> list = new List<TenantGrantRightDto>();
                var all = MetadataHelper.GetTenantAppRights();
                TreeHelper.TraverseTree(all, t => t.Children, () => new TenantPackageRight(), (treeNode, result, depth) =>
                {
                    if (depth == 0) // 第一层是应用
                    {
                        result.Application = treeNode.Code;
                        result.ApplicationName = treeNode.Name;
                    }
                    else if (treeNode.Actions != null && treeNode.Actions.Any()) // 最末级是菜单
                    {
                        list.AddRange(treeNode.Actions.Select(a => new TenantGrantRightDto
                        {
                            Application = result.Application,
                            ApplicationName = result.ApplicationName,
                            NavCode = result.NavCode,
                            NavName = result.NavName,
                            MenuCode = treeNode.Code,
                            MenuName = treeNode.Name,
                            ActionCode = a.Code,
                            ActionName = a.Name,
                        }));
                    }
                    else  // 中间都是导航
                    {
                        result.NavCode = treeNode.Code;
                        result.NavName = treeNode.Name;
                    }
                });
                return list;
            }
            else
            {
                var list = await fsql.Select<TenantPackageRight>()
                    .Where(t => t.TenantPackageId == TenantContext.CurrentTenant.TenantPackageId)
                    .ToListAsync<TenantGrantRightDto>();

                // 过滤掉租户管理后台的菜单
                list = list.Where(t => !"tenant:".Equals(t.Application)).ToList();

                return list;
            }
        }

        /// <summary>
        /// 获取当前租户的授权菜单树形结构
        /// </summary>
        /// <returns></returns>
        public async Task<List<TenantAppRightDto>> GetTenantGrantTree()
        {
            var manageTenant = _configuration.GetManagerTenant();
            if (TenantContext.CurrentTenant.Id == manageTenant.Id)
            {
                var all = MetadataHelper.GetTenantAppRights();
                return all;
            }
            else
            {
                var list = await fsql.Select<TenantPackageRight>()
                    .Where(t => t.TenantPackageId == TenantContext.CurrentTenant.TenantPackageId)
                    .ToListAsync<TenantGrantRightDto>();

                // 过滤掉租户管理后台的菜单
                list = list.Where(t => !"tenant".Equals(t.Application)).ToList();

                var applications = list.Select(t =>
                    new TenantAppRightDto
                    {
                        Name = t.ApplicationName,
                        Code = t.Application
                    }).Distinct().ToList();
                foreach (var app in applications)
                {
                    app.Children = list.Where(t => t.Application == app.Code)
                        .Select(t => new TenantAppRightDto
                        {
                            Name = t.NavName,
                            Code = t.NavCode
                        }).Distinct().ToList();
                    foreach (var nav in app.Children)
                    {
                        nav.Children = list.Where(t => t.NavCode == nav.Code && t.Application == app.Code)
                            .Select(t => new TenantAppRightDto
                            {
                                Name = t.MenuName,
                                Code = t.MenuCode
                            }).Distinct().ToList();
                        foreach (var menu in nav.Children)
                        {
                            menu.Children = list.Where(t =>
                                    t.MenuCode == menu.Code && t.NavCode == nav.Code && t.Application == app.Code)
                                .Select(t => new TenantAppRightDto
                                {
                                    Name = t.ActionName,
                                    Code = t.ActionCode
                                }).Distinct().ToList();
                        }
                    }
                }

                return applications;
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Simple.AdminApplication.Extensions;
using Simple.AdminApplication.SysMng;
using Simple.AdminApplication.TenantMng.Dto;
using Simple.AdminApplication.TenantMng.Entities;
using Simple.AspNetCore.Controllers;

namespace Simple.AdminApplication.TenantMng.Controllers
{
    [Permission("TenantList", "租户管理")]
    public class TenantController : AppCurdController<Tenant, TenantDto>
    {
        private readonly PermissionService _permissionService;
        private readonly TenantPackageService _tenantPackageService;
        private readonly TenantDataSourceService _tenantDataSourceService;
        private readonly IConfiguration _configuration;

        public TenantController(PermissionService permissionService, TenantPackageService tenantPackageService,
            TenantDataSourceService tenantDataSourceService, IConfiguration configuration)
        {
            _permissionService = permissionService;
            _tenantPackageService = tenantPackageService;
            _tenantDataSourceService = tenantDataSourceService;
            _configuration = configuration;
        }

        [HttpPost]
        [Permission("add", "保存租户")]
        [Permission("edit", "保存租户")]
        public override async Task<ApiResult> Save(ParameterModel model)
        {
            var data = await base.Save(model);
            await _permissionService.ClearPermissionCacheAsync();
            return data;
        }

        [HttpPost]
        [Permission("query", "查询")]
        public override async Task<ApiResult> Page(QueryRequestInput pageRequest)
        {
            var (total, data) = await Service.Page(pageRequest);
            
            var dataSourceList = await _tenantDataSourceService.All.ToListAsync();
            var manageDataSource = _configuration.GetManagerDataSource();
            dataSourceList.Add(manageDataSource);
            
            var dataPackageList = await _tenantPackageService.All.ToListAsync();
            foreach (var item in data)
            {
                var dataSource = dataSourceList.FirstOrDefault(t => t.Id == item.DataSourceId);
                item.SetAttributeValue("dataSourceName", dataSource?.Remark ?? "");

                var dataPackage = dataPackageList.FirstOrDefault(t => t.Id == item.TenantPackageId);
                item.SetAttributeValue("tenantPackageName", dataPackage?.Name);
            }

            return ApiResult.Success(new { data, total });
        }
    }
}
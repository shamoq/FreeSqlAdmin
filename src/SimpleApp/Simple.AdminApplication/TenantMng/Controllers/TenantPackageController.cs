using Microsoft.AspNetCore.Mvc;
using Simple.AdminApplication.SysMng;
using Simple.AdminApplication.TenantMng.Dto;
using Simple.AdminApplication.TenantMng.Entities;
using Simple.AdminApplication.TenantMng.Interfaces;
using Simple.AdminApplication.UserMng.Dto;
using Simple.AdminApplication.UserMng;
using Simple.AspNetCore.Controllers;

namespace Simple.AdminApplication.TenantMng.Controllers
{
    [Permission("TenantPackage", "租户套餐")]
    public class TenantPackageController : AppCurdController<TenantPackage, TenantPackage>
    {
        private readonly TenantPackageService _tenantPackageService;
        private readonly TenantService _tenantService;
        private readonly PermissionService _permissionService;

        public TenantPackageController(TenantPackageService tenantPackageService, TenantService tenantService, PermissionService permissionService)
        {
            _tenantPackageService = tenantPackageService;
            _tenantService = tenantService;
            _permissionService = permissionService;
        }

        [HttpPost]
        [Permission( "query","查询授权")]
        public async Task<ApiResult> GetGrants(InputId input)
        {
            var tenant = await _tenantPackageService.FindAsync(input.Id);
            var tenantRights = await _tenantService.GetTenantGrantTree();
            // 给租户授权，不允许授权多租户管理后台
            tenantRights.RemoveAll(t => t.Code == "tenant");
            var dtos = await _tenantPackageService.GetGrants(input.Id);
            var actionCodes = dtos.Select(p => p.ActionCode).ToList()
                .Union(dtos.Select(t => t.Application).Distinct())
                .Union(dtos.Select(t => t.NavCode).Distinct())
                .Union(dtos.Select(t => t.MenuCode).Distinct())
                .ToList();
            return ApiResult.Success(new { name = tenant.Name, tenantRights, actionCodes });
        }

        /// <summary>角色授权权限</summary>
        /// <returns></returns>
        [HttpPost]
        [Permission("grant","授权")]
        public async Task<ApiResult> Grant(TenantGrantInput input)
        {
            await _tenantPackageService.Grant(input);
            await _permissionService.ClearPermissionCacheAsync();
            return ApiResult.Success();
        }

        /// <summary>
        /// 根据租户获取授权菜单
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Permission("query","查询授权菜单")]
        public async Task<ApiResult> GetTenantGrants(ParameterModel input)
        {
            var list = await _tenantPackageService.GetGrants(TenantContext.CurrentTenant.TenantPackageId);
            return ApiResult.Success(list);
        }

        public override async Task<ApiResult> Page(QueryRequestInput pageRequest)
        {
            var (total, list) = await Service.Page(pageRequest);
            var summarries = _tenantService.All.GroupBy(t => t.TenantPackageId).Select(t => new
            {
                TenantPackageId = t.Key,
                Total = t.Count(),
            }).ToList();

            foreach (var item in list)
            {
                var summarry = summarries.FirstOrDefault(t => t.TenantPackageId == item.Id);
                item.SetAttributeValue("tenantCount", summarry?.Total ?? 0);
            }

            return ApiResult.Success(new { total, data = list });

        }
    }
}
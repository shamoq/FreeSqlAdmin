using Mapster;
using Microsoft.AspNetCore.Mvc;
using Simple.AdminApplication.SysMng;
using Simple.AdminApplication.SysMng.Dto;
using Simple.AdminApplication.TenantMng.Interfaces;
using Simple.AdminApplication.UserMng.Dto;
using Simple.AdminApplication.UserMng.Entities;
using Simple.AspNetCore.Controllers;

namespace Simple.AdminApplication.UserMng.Controllers
{
    /// <summary>角色控制器</summary>
    [Permission("role","角色管理")]
    public class RoleController : AppCurdController<SysRole, RoleDto>
    {
        private readonly RoleService _roleService;
        private readonly ITenantService tenantService;
        private readonly PermissionService _permissionService;

        public RoleController(RoleService roleService, ITenantService tenantService, PermissionService permissionService)
        {
            _roleService = roleService;
            this.tenantService = tenantService;
            _permissionService = permissionService;
        }

        /// <summary>角色授权菜单 有则删除，无则添加</summary>
        /// <returns></returns>
        [HttpPost]
        [Permission("query","查询授权")]
        public async Task<ApiResult> GetGrants(InputId input)
        {
            var tenantRights = await tenantService.GetTenantGrantTree();
            var role = await _roleService.FindAsync(input.Id);
            var dtos = await _roleService.GetRoleGrants(input.Id);
            var actionCodes = dtos.Select(p => p.ActionCode).ToList()
                .Union(dtos.Select(t=>t.Application).Distinct())
                .Union(dtos.Select(t=>t.NavCode).Distinct())
                .Union(dtos.Select(t=>t.MenuCode).Distinct())
                .ToList();
            return ApiResult.Success(new { name = role.Name, tenantRights, actionCodes });
        }

        /// <summary>角色授权权限</summary>
        /// <returns></returns>
        [HttpPost]
        [Permission("grant","授权")]
        public async Task<ApiResult> Grant(RoleGrantInput input)
        {
            await _roleService.Grant(input);
            await _permissionService.ClearPermissionCacheAsync();
            return ApiResult.Success();
        }

        [HttpPost]
        [Permission("query","查询角色")]
        public override async Task<ApiResult> List(QueryRequestInput pageRequest)
        {
            var list = await _roleService.GetList(t => true);
            var orderList = list.OrderBy(t => t.CreatedTime).ToList();
            var dtoList = orderList.Adapt<List<RoleDto>>();
            TreeHelper.BuildTreeProps(dtoList);
            return ApiResult.Success(dtoList);
        }
        
    }
}
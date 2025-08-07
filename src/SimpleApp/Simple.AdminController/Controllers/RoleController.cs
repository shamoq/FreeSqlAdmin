using Mapster;
using Microsoft.AspNetCore.Mvc;
using Simple.AdminApplication.Application.UserMng;
using Simple.AdminApplication.DbContexts;
using Simple.AdminApplication.Dtos;
using Simple.AdminApplication.Entitys.UserMng;
using Simple.AdminController.Models;
using Simple.AspNetCore.Controllers;

namespace Simple.AdminController.Controllers
{
    /// <summary>角色控制器</summary>
    [PermissionGroup("角色Curd")]
    public class RoleController : AppCurdController<SysRole, RoleDto>
    {
        private RoleService _roleService;

        public RoleController(RoleService roleService) 
        {
            this._roleService = roleService;
        }

        /// <summary>角色授权菜单 有则删除，无则添加</summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> GetGrants(InputId input)
        {
            var role = await _roleService.FindById(input.Id);
            var dtos = await _roleService.GetRoleGrants(input.Id);
            return ApiResult.Success(new { name = role.Name, rights = dtos });
        }

        /// <summary>角色授权权限</summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> Grant(RoleGrantInput input)
        {
            await _roleService.Grant(input);
            return ApiResult.Success();
        }

        [HttpPost]
        public override async Task<ApiResult> List(QueryRequestInput pageRequest)
        {
            var list = await _roleService.GetList(t => true);
            var orderList = list.OrderBy(t => t.CreatedTime).ToList();
            var dtoList = orderList.Adapt<List<RoleDto>>();
            TreeHelper.BuildTreeProps(dtoList, t => t.Id, t => t.ParentId, t => t.Name);
            return ApiResult.Success(dtoList);
        }
    }
}
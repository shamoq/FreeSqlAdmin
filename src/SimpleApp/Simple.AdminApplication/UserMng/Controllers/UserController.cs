using Mapster;
using Microsoft.AspNetCore.Mvc;
using Simple.AdminApplication.SysMng;
using Simple.AdminApplication.UserMng.Dto;
using Simple.AdminApplication.UserMng.Entities;
using Simple.AspNetCore.Controllers;
using Simple.AspNetCore.Services;

namespace Simple.AdminApplication.UserMng.Controllers
{
    /// <summary>用户控制器</summary>
    [Permission("user", "用户管理")]
    public class UserController : AppCurdController<SysUser, SysUserDto>
    {
        private readonly OrgService _orgService;
        private readonly RoleService _roleService;
        private readonly UserService _userService;
        private readonly UserContextService _userContext;
        private readonly PermissionService _permissionService;

        public UserController(OrgService orgService, RoleService roleService, UserService userService,
            UserContextService userContext, PermissionService permissionService)
        {
            _orgService = orgService;
            _roleService = roleService;
            _userService = userService;
            _userContext = userContext;
            _permissionService = permissionService;
        }

        [HttpPost, Permission("query", "保存")]
        public override async Task<ApiResult> Save(ParameterModel model)
        {
            var roleIds = model.GetAttributeValue<List<Guid>>("roleIds");
            var userResult = await base.Save(model);
            var user = userResult.Data as SysUserDto;

            // 保存用户角色
            if (userResult.IsSuccess && user != null && roleIds != null)
            {
                await _roleService.SetUserRoles(user.Id, roleIds);
            }

            await _permissionService.ClearPermissionCacheAsync();

            return userResult;
        }

        /// <summary>获取列表</summary>
        /// <returns></returns>
        [HttpPost, Permission("query", "查询")]
        public override async Task<ApiResult> List(QueryRequestInput pageRequest)
        {
            var list = await Service.GetList(pageRequest);

            var dtoList = list.Adapt<List<SysUser>>();

            return ApiResult.Success(dtoList);
        }

        /// <summary>获取列表</summary>
        /// <returns></returns>
        [HttpPost, Permission("query", "查询")]
        public override async Task<ApiResult> Page(QueryRequestInput pageRequest)
        {
            // 获取前端参数
            var name = pageRequest.GetAttributeValue<string>("name");
            var orgId = pageRequest.GetAttributeValue<Guid?>("orgId");

            var orgnizations = await _orgService.GetList(t => true);

            List<Guid> searchOrgnizationIds = new List<Guid>();
            if (orgId != null)
            {
                var searchOrg = orgnizations.FirstOrDefault(t => t.Id == orgId.Value);
                if (searchOrg != null)
                {
                    var searchOrgCode = searchOrg.OrderFullCode;
                    var childOrgs = orgnizations.Where(t => t.OrderFullCode.StartsWith(searchOrgCode)).ToList();

                    searchOrgnizationIds.Add(searchOrg.Id);
                    searchOrgnizationIds.AddRange(childOrgs.Select(t => t.Id).ToList());
                }
            }

            pageRequest.AdditionalExpression = new ExpressionHolder<SysUser>()
                .AddIf(searchOrgnizationIds.Count > 0, t => searchOrgnizationIds.Contains(t.OrgId))
                .AddIf(!string.IsNullOrEmpty(name), t => t.UserCode.Contains(name) || t.UserName.Contains(name));
            var (total, list) = await Service.Page(pageRequest);
            var data = list.Adapt<List<SysUserDto>>();

            // 填充角色名称和组织名称
            var userIds = list.Select(t => t.Id).Distinct().ToList();
            var userRoles = await _roleService.GetUserRoles(userIds);

            foreach (var item in data)
            {
                var roleName = string.Join(';',
                    userRoles.Where(t => t.UserId == item.Id).Select(t => t.RoleName).ToArray());
                item.RoleName = roleName;
                item.OrgnizationName = orgnizations.FirstOrDefault(t => t.Id == item.OrgId)?.Name;
                item.OrgnizationFullName = orgnizations.FirstOrDefault(t => t.Id == item.OrgId)?.FullName;
            }

            return ApiResult.Success(new { total, data });
        }

        /// <summary>获取列表</summary>
        /// <returns></returns>
        [HttpPost, Permission("query", "查询")]
        public async Task<ApiResult> ResetPassword(ParameterModel input)
        {
            var (oldPassword, newPassword) = input.GetAttributeValue<string, string>("oldPassword", "newPassword");
            if (string.IsNullOrEmpty(oldPassword))
            {
                return ApiResult.Fail("旧密码不能为空");
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                return ApiResult.Fail("新密码不能为空");
            }

            await _userService.ResetPassword(_userContext.GetUser().Id, oldPassword, newPassword);
            return ApiResult.Success();
        }

        [HttpPost, Permission("query", "查询")]
        public override async Task<ApiResult> Get(ParameterModel model)
        {
            var userResult = await base.Get(model);
            // 查询用户角色
            var user = userResult.Data as SysUserDto;
            if (userResult.IsSuccess && user != null)
            {
                var userRoles = await _roleService.GetUserRoles(new List<Guid>() { user.Id });
                user.SetAttributeValue("roleNames", userRoles.Select(t => t.RoleName));
                user.SetAttributeValue("roleIds", userRoles.Select(t => t.RoleId));
            }

            return userResult;
        }
    }
}
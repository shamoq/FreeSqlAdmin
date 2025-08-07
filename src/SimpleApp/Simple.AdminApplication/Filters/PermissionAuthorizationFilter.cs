using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Simple.AdminApplication.SysMng;
using Simple.AdminApplication.TenantMng.Dto;

namespace Simple.AdminApplication.Filters
{
    /// <summary>鉴权过滤器</summary>
    public class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly PermissionService _permissionService;

        public PermissionAuthorizationFilter(PermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var userId = TenantContext.CurrentUser.Id;
            
            // 收集方法级别的权限
            var methodPermissions = context.ActionDescriptor
                .EndpointMetadata
                .OfType<PermissionAttribute>();

            // 如果没有权限特性，直接放行
            if (!methodPermissions.Any())
            {
                return;
            }

            var groupCode = string.Empty;
            var emptyGroup = methodPermissions.Any(t=>string.IsNullOrEmpty(t.Group));
            if (emptyGroup)
            {
                // 收集控制器级别的权限
                var controllerPermission = context.ActionDescriptor
                    .EndpointMetadata
                    .OfType<PermissionAttribute>()
                    .FirstOrDefault();
                groupCode = controllerPermission?.Group;
            }

            // 获取权限码
            var codes = await _permissionService.GetUserPermissionCodeAsync(userId);

            // 5. 验证用户是否拥有所有必需权限
            var hasRight = false;
            foreach (var permission in methodPermissions)
            {
                var fullCode = permission.Group ?? groupCode +":" + permission.Code;
                if (codes.Contains(fullCode, StringComparer.OrdinalIgnoreCase))
                {
                    hasRight = true;
                    break;
                }
            }

            // 没有权限则拦截
            if (!hasRight)
            {
                context.Result = new ForbidResult();
            }
        }

        /// <summary>设置401 没有权限的返回</summary>
        /// <param name="context"></param>
        public void Set401Result(AuthorizationFilterContext context, string message = "权限异常")
        {
            var result = new ApiResult
            {
                Code = 401,
                Message = message
            };

            context.HttpContext.Response.StatusCode = 200;
            context.Result = new JsonResult(result);
            return;
        }
    }
}
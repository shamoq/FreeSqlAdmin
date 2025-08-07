using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simple.AdminApplication.SysMng;
using Simple.AdminApplication.TenantMng.Dto;
using Simple.AdminApplication.UserMng.Dto;

namespace Simple.AdminApplication.UserMng.Controllers
{
    /// <summary>
    /// 没有登录的情况下，没有用户上下文
    /// 不能注入任何数据库查询相关的上下文
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService authService;
        private readonly PermissionService _permissionService;
        private readonly SysLoginLogService _sysLoginLogService;

        public AuthController(AuthService authService, PermissionService permissionService, SysLoginLogService sysLoginLogService)
        {
            this.authService = authService;
            _permissionService = permissionService;
            _sysLoginLogService = sysLoginLogService;
        }

        /// <summary>登录</summary>
        /// <returns></returns>
        [HttpPost("login"), AllowAnonymous]
        public async Task<ApiResult> Login(LoginInput input)
        {
            var res = await authService.Login(input);

            // 为了让后续的异步上下文里有租户上下文，需要重新设置
            TenantContext.SetCurrentTenant(res.tenant);
            TenantContext.SetCurrentUser(res.user);
            var tenant = res.tenant;
            if (tenant.ExpirationTime != null)
            {
                var day = Math.Ceiling((tenant.ExpirationTime.Value - DateTime.Now).TotalDays);
                if (day < 0)
                {
                    return ApiResult.Success(new { accessToken = res.token, message = "系统已到期，请及时续费" });
                }
                else if (day <= 15)
                {
                    return ApiResult.Success(new { accessToken = res.token, message = "有效期剩余" + day + "天，请及时续费" });
                }
            }

            await _permissionService.InitializePermissionsAsync(res.user.Id);
            await _sysLoginLogService.AddLoginLog(res.user);

            return ApiResult.Success(new { accessToken = res.token });
        }

        /// <summary>退出登录</summary>
        /// <returns></returns>
        [HttpPost("logout")]
        public async Task<ApiResult> Logout()
        {
            await _permissionService.ClearPermissionCacheAsync(TenantContext.CurrentUser.Id);
            return ApiResult.Success();
        }
    }
}
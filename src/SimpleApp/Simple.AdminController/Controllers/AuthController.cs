using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Simple.AdminApplication.Application;
using Simple.AdminApplication.Application.UserMng;
using Simple.AdminApplication.Dtos;
using Simple.AdminApplication.Helpers;
using Simple.AspNetCore.Services;
using Simple.Utils.Extensions;
using Simple.Utils.Models.Tenants;

namespace Simple.AdminController.Controllers
{
    /// <summary>
    /// 没有登录的情况下，没有用户上下文
    /// 不能注入任何数据库查询相关的上下文
    /// </summary>
    [Route("api/[controller]")]
    [ApiController, PermissionGroup("系统管理")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService authService;

        public AuthController(AuthService authService)
        {
            this.authService = authService;
        }

        /// <summary>登录</summary>
        /// <returns></returns>
        [HttpPost("login"), AllowAnonymous]
        public async Task<ApiResult> Login(LoginInput input)
        {
            var accessToken = await authService.Login(input);
            var tenantInfo = TenantContext.TrySetTenantByCode(input.TenantCode);
            var day = Math.Ceiling((tenantInfo.ExpirationTime - DateTime.Now).TotalDays);
            if (day < 0)
            {
                return ApiResult.Success(new { accessToken, message = "系统已到期，请及时续费" });
            }
            else if (day <= 15)
            {
                return ApiResult.Success(new { accessToken, message = "有效期剩余" + day + "天，请及时续费" });
            }

            return ApiResult.Success(new { accessToken });
        }

        /// <summary>
        /// 获取所有租户
        /// </summary>
        /// <returns></returns>
        [HttpGet("tenants"), AllowAnonymous]
        public async Task<ApiResult> GetTenants()
        {
            var tenants = TenantContext.AllTenants.Select(t => new
            {
                t.Value.TenantCode,
                t.Value.TenantName
            });
            return await Task.FromResult(ApiResult.Success(tenants));
        }

        /// <summary>退出登录</summary>
        /// <returns></returns>
        [HttpPost("logout")]
        public ApiResult Logout()
        {
            return ApiResult.Success();
        }
    }
}
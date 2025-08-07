using FreeSql;
using Microsoft.Extensions.Caching.Distributed;
using Simple.AdminApplication.Common;
using Simple.AdminApplication.SysMng;
using Simple.AdminApplication.TenantMng;
using Simple.AdminApplication.TenantMng.Dto;
using Simple.AdminApplication.TenantMng.Entities;
using Simple.AdminApplication.UserMng.Dto;
using Simple.AspNetCore.Extensions;
using Simple.AspNetCore.Helper;
using Simple.Utils.Exceptions;
using Simple.Utils.Extensions;
using Simple.Utils.Models.BO;

namespace Simple.AdminApplication.UserMng
{
    /// <summary>授权鉴权服务</summary>
    [Scoped]
    public class AuthService
    {
        private UserService _userService;
        private TenantService _tenantService;

        public AuthService(TenantService tenantService, UserService userService)
        {
            this._tenantService = tenantService;
            this._userService = userService;
        }

        /// <summary>登录</summary>
        /// <returns></returns>
        public async Task<(Tenant tenant, LoginUserBO user, string token )> Login(LoginInput input)
        {
            if (input.UserCode.IsNullOrEmpty() || input.Password.IsNullOrEmpty())
            {
                throw new CustomException("账号或密码不能为空");
            }


            // 校验租户是否存在
            var tenant = await _tenantService.GetTenant(input.TenantCode);
            if (tenant == null)
            {
                throw new CustomException("租户不存在");
            }

            _tenantService.ChangeTenant(tenant);
            TenantContext.SetCurrentTenant(tenant);
            var user = await _userService.All.Where(p => p.UserCode == input.UserCode).FirstAsync();
            if (user == null)
            {
                throw new CustomException("账号或密码错误");
            }

            var encryptPassword = UserService.GetMd5Password(user, input.Password);

            if (user.Password != encryptPassword)
            {
                throw new CustomException("账号或密码错误");
            }

            if (user.IsEnable == 0)
            {
                throw new CustomException("用户已被禁用");
            }
            
            user.LastLoginTime = DateTime.Now;
            user.LastIpAdress = HttpContextHelper.GetRealIpAddress();
            await _userService.UpdateAsync(user);

            var payload = new LoginUserBO()
            {
                IsAdmin = user.IsAdmin,
                OrgId = user.OrgId,
                UserCode = user.UserCode,
                Id = user.Id,
                UserName = user.UserName,
                TenantId = tenant.Id,
            };

            var token = JWTHelper.CreateToken(payload);
            // await cache.SetStringAsync(input.UserCode, token);

            return (tenant, payload, token);
        }

        /// <summary>刷新token</summary>
        /// <returns></returns>
        public ApiResult RefreshAuthentication(LoginUserBO user)
        {
            var oldToken =
                HostServiceExtension.CurrentHttpContext.Request.Headers[ConfigHelper.GetValue("TokenHeadKey")];
            if (string.IsNullOrEmpty(oldToken))
            {
                return ApiResult.Success("无法获取请求的Token");
            }

            var newToken = JWTHelper.RefreshToken(oldToken);
            // cache.SetString(user.UserCode, newToken);
            return ApiResult.Success(new { authrization = newToken });
        }
    }
}
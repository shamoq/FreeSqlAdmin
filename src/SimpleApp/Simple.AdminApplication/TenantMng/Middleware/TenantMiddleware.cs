using FreeSql;
using Simple.AdminApplication.TenantMng.Dto;
using Simple.AspNetCore.Helper;
using Simple.Utils.Exceptions;

namespace Simple.AdminApplication.TenantMng.Middleware
{
    /// <summary>
    /// 租户中间件，用于处理请求中的租户信息
    /// </summary>
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                if (context.Request.Path.Value != null && (context.Request.Path.Value.Contains("swagger") ||
                                                           context.Request.Path.Value.Contains("api/auth/login")))
                {
                    await _next(context);
                    return;
                }

                // 初始化用户上下文
                var user = JWTHelper.GetPayload(context.User.Claims.ToArray());
                TenantContext.SetCurrentUser(user);

                var tenantService = context.RequestServices.GetRequiredService<TenantService>();

                // 切换到租户数据库
                try
                {
                    var tenant = await tenantService.GetTenant(user.TenantId);
                    if (tenant == null)
                    {
                        throw new CustomException("租户不存在");
                    }

                    tenantService.ChangeTenant(tenant);
                    TenantContext.SetCurrentTenant(tenant);

                    await _next(context);
                }
                finally
                {
                    // 释放租户数据库连接
                    TenantContext.ClearTenant();
                    ;
                }
            }
            catch (Exception ex)
            {
                throw new CustomException($"租户处理异常: {ex.Message}");
            }
        }
    }
}
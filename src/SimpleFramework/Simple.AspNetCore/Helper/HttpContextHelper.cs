using Microsoft.AspNetCore.Http;

namespace Simple.AspNetCore.Extensions;

public class HttpContextHelper
{
    /// <summary>获取真实IP地址</summary>
    public static string GetRealIpAddress()
    {
        var httpContext = HostServiceExtension.CurrentHttpContext;

        // 优先检查X-Forwarded-For头
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // X-Forwarded-For可能包含多个IP，取第一个
            return forwardedFor.Split(',')[0].Trim();
        }

        // 检查X-Real-IP头
        var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // 最后回退到RemoteIpAddress
        return httpContext.Connection.RemoteIpAddress?.ToString();
    }
}
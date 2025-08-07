using System.Security.Claims;
using Dm.Config;
using Simple.AdminApplication.Common;
using Simple.AdminApplication.SysMng.Entities;
using Simple.AspNetCore.Extensions;
using Simple.Utils.Models.BO;

namespace Simple.AdminApplication.SysMng;

[Scoped]
public class SysLoginLogService : BaseCurdService<SysLoginLog>
{
    public async Task AddLoginLog(LoginUserBO user)
    {
        // 创建登录日志实体
        var loginLog = new SysLoginLog
        {
            UserId = user?.Id,
            UserName = user?.UserName,
            UserCode = user?.UserCode,
            LoginTime = DateTime.Now,
            // ErrorMsg = errorMessage,
            Creator = user.UserName,
        };

        // 获取客户端IP地址
        var httpContext = HostServiceExtension.CurrentHttpContext;
        loginLog.LoginIp = HttpContextHelper.GetRealIpAddress();

        // 从User-Agent中解析浏览器和设备信息
        var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
        if (!string.IsNullOrEmpty(userAgent))
        {
            var browserInfo = ParseUserAgent(userAgent);
            loginLog.Browser = browserInfo.Browser;
            loginLog.Device = browserInfo.Device;
        }

        // 从请求头中获取屏幕分辨率（需要前端配合发送这个头）
        var resolution = httpContext.Request.Headers["X-Screen-Resolution"].ToString();
        if (!string.IsNullOrEmpty(resolution))
        {
            loginLog.ScreenResolution = resolution;
        }

        // 生成并设置会话ID
        loginLog.SessionId = user.SessionId;
        await table.InsertAsync(loginLog);
    }


    // 辅助方法：解析User-Agent获取浏览器和设备信息
    private (string Browser, string Device) ParseUserAgent(string userAgent)
    {
        // 这里使用简单的字符串解析，实际应用中可以使用专门的库如UAParser
        var browser = "Unknown";
        var device = "Unknown";

        if (userAgent.Contains("Chrome"))
        {
            browser = "Chrome";
        }
        else if (userAgent.Contains("Firefox"))
        {
            browser = "Firefox";
        }
        else if (userAgent.Contains("Safari"))
        {
            browser = "Safari";
        }
        else if (userAgent.Contains("Edge"))
        {
            browser = "Edge";
        }

        if (userAgent.Contains("Mobile"))
        {
            if (userAgent.Contains("iPhone"))
            {
                device = "iPhone";
            }
            else if (userAgent.Contains("Android"))
            {
                device = "Android";
            }
        }
        else
        {
            device = "Desktop";
            // 添加操作系统检测
            if (userAgent.Contains("Windows NT"))
            {
                device += " (Windows)";
            }
            else if (userAgent.Contains("Mac OS X"))
            {
                device += " (macOS)";
            }
            else if (userAgent.Contains("Linux"))
            {
                device += " (Linux)";
            }
            else if (userAgent.Contains("FreeBSD"))
            {
                device += " (FreeBSD)";
            }
        }

        return (browser, device);
    }
}
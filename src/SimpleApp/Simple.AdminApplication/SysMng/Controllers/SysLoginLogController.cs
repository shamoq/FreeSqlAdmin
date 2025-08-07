using Mapster;
using Microsoft.AspNetCore.Mvc;
using Simple.AspNetCore.Controllers;
using Simple.Utils.Models.Dto;

namespace Simple.AdminApplication.SysMng.Controllers;

[Permission("sysloginlog", "登录日志")]
public class SysLoginLogController : AppAuthController
{
    private readonly SysLoginLogService _sysLoginLogService;

    public SysLoginLogController(SysLoginLogService sysLoginLogService)
    {
        _sysLoginLogService = sysLoginLogService;
    }
    
    /// <summary>获取分页列表</summary>
    /// <returns></returns>
    [HttpPost, Permission("query", "查询")]
    public virtual async Task<ApiResult> Page(QueryRequestInput pageRequest)
    {
        var (total, list) = await _sysLoginLogService.Page(pageRequest);
        return ApiResult.Success(new { total, data = list });
    }
}
using Microsoft.AspNetCore.Mvc;
using Simple.AspNetCore.Controllers;

namespace Simple.AdminController.Controllers;

public class ToolsController : AppAuthController
{
    /// <summary>
    /// 获取Guid
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult> NewGuid()
    {
        var guid = Guid.NewGuid();
        var result = await Task.FromResult(guid);
        return ApiResult.Success(result);
    }
}
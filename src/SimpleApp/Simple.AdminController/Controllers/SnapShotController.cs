using Microsoft.AspNetCore.Mvc;
using Simple.AdminApplication.Application;
using Simple.AdminApplication.Entitys;
using Simple.AspNetCore.Controllers;

namespace Simple.AdminController.Controllers;

public class SnapShotController : AppCurdController<SysSnapShot, SysSnapShot>
{
    private SnapShotService _snapShotService;

    public SnapShotController(SnapShotService snapShotService)
    {
        _snapShotService = snapShotService;
    }

    [HttpPost]
    public async Task<ApiResult> GetByBusinessId(ParameterModel input)
    {
        var (businessId, type) = input.GetAttributeValue<Guid, string>("businessId", "type");
        if (ValueHelper.IsNullOrEmpty(businessId) || ValueHelper.IsNullOrEmpty(type))
        {
            return ApiResult.Fail("参数错误");
        }

       var data = await  _snapShotService.GetSnapByBusinessId<string>(businessId, type);
       return ApiResult.Success(data);
    }
}
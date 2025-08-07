using Mapster;
using Microsoft.AspNetCore.Mvc;
using Simple.AdminApplication.Application;
using Simple.AdminApplication.Dtos;
using Simple.AdminApplication.Entitys.Contracts;
using Simple.AspNetCore.Controllers;

namespace Simple.AdminController.Controllers;

public class CustomFieldController : AppCurdController<CustomField, CustomFieldDto>
{
    private readonly CustomFieldService _customFieldService;

    public CustomFieldController(CustomFieldService customFieldService)
    {
        _customFieldService = customFieldService;
    }

    /// <summary>
    /// 不含系统默认字段
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult> GetUserFields(ParameterModel input)
    {
        if (input.Id == null)
        {
            return ApiResult.Fail("请选择模版");
        }

        var containsDefault = input.GetAttributeValue<bool?>("containsDefault");

        List<CustomFieldDto> dtoList;
        if (containsDefault == true)
        {
            dtoList = await _customFieldService.GetContractFields(input.Id.Value);
        }
        else
        {
            dtoList = await _customFieldService.GetUserFields(input.Id.Value);
        }

        TreeHelper.BuildTreeProps(dtoList, t => t.Id, t => t.ParentId, t => t.Name);

        return ApiResult.Success(dtoList);
    }

    [HttpPost]
    public async Task<ApiResult> ResetOrder(ParameterModel input)
    {
        var ids = input.GetAttributeValue<List<Guid>>("ids");
        if (ids == null || ids.Count == 0)
        {
            return ApiResult.Fail("ids 参数错误");
        }

        await _customFieldService.ResetOrder(ids);
        return ApiResult.Success();
    }

    [HttpPost]
    public async Task<ApiResult> SaveFields(ParameterModel input)
    {
        var businessId = input.GetAttributeValue<Guid>("businessId");
        var fields = input.GetAttributeValue<List<CustomFieldDto>>("fields");
        var customFields = fields.Adapt<List<CustomField>>();
        await _customFieldService.SaveFields(businessId, customFields);
        return ApiResult.Success();
    }
}
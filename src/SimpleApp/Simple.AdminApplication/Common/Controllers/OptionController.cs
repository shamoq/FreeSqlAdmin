using Microsoft.AspNetCore.Mvc;
using Simple.AdminApplication.UserMng;
using Simple.AdminApplication.UserMng.Entities;
using Simple.AspNetCore.Controllers;
using Simple.Utils.Models.Dto;

namespace Simple.AdminApplication.Common.Controllers;

public class OptionController : AppAuthController
{
    private UserService _userService;
    private OrgService _orgService;

    public OptionController(UserService userService, OrgService orgService)
    {
        _userService = userService;
        _orgService = orgService;
    }

    [HttpPost]
    public async Task<ApiResult> GetUser(QueryRequestInput input)
    {
        var search = input.GetAttributeValue<string>("search");

        var express = new ExpressionHolder<SysUser>()
            .AddIf(true, t => t.IsEnable == 1)
            .AddIf(!string.IsNullOrEmpty(search), t => t.UserName.Contains(search))
            .ToExpression();

        var list = await _userService.GetList(express);
        var orgnizationIds = list.Select(t => t.OrgId).ToList();
        var orgnizations = await _orgService.GetList(t => orgnizationIds.Contains(t.Id));

        var dtos = new List<OptionDto>();
        foreach (var user in list)
        {
            var orgnization = orgnizations.FirstOrDefault(t => t.Id == user.OrgId);
            var displayName = user.UserName;
            if (orgnization != null)
            {
                displayName = user.UserName + "(" + orgnization.Name + ")";
            }

            dtos.Add(new OptionDto(user.Id, displayName, new
            {
                orgnizationId = orgnization?.Id,
                orgnizationName = orgnization?.Name,
                userName = user.UserName,
            }));
        }

        return ApiResult.Success(dtos);
    }

    /// <summary>
    /// 获取枚举
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult> GetEnums(ParameterModel input)
    {
        var type = input.GetAttributeValue<string>("type");
        if (string.IsNullOrEmpty(type))
        {
            return ApiResult.Success(new int[] { });
        }

        // 获取枚举
        var typeName = $"Simple.AdminApplication.Enums.{type}, Simple.AdminApplication";

        var staticClassType = Type.GetType(typeName);

        var options = StaticClassEnumHelper.GetOptions(staticClassType);

        var reuslt = await Task.FromResult(options);
        return ApiResult.Success(reuslt);
    }
}
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Simple.AdminApplication.Application;
using Simple.AdminApplication.Application.UserMng;
using Simple.AdminApplication.Dtos;
using Simple.AdminApplication.Entitys.Contracts;
using Simple.AdminApplication.Entitys.UserMng;
using Simple.AspNetCore.Controllers;
using Simple.Utils.Models.Dto;

namespace Simple.AdminController.Controllers;

public class OptionController : AppAuthController
{
    private UserService _userService;
    private SignPartyService _signPartyService;
    private ProviderService _providerService;
    private OrgnizationService _orgnizationService;

    public OptionController(UserService userService, SignPartyService signPartyService, ProviderService providerService, OrgnizationService orgnizationService)
    {
        _userService = userService;
        _signPartyService = signPartyService;
        _providerService = providerService;
        _orgnizationService = orgnizationService;
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
        var orgnizationIds = list.Select(t => t.OrgnizationId).ToList();
        var orgnizations = await _orgnizationService.GetList(t => orgnizationIds.Contains(t.Id));

        var dtos = new List<OptionDto>();
        foreach (var user in list)
        {
            var orgnization = orgnizations.FirstOrDefault(t => t.Id == user.OrgnizationId);
            var displayName = user.UserName;
            if (orgnization != null)
            {
                displayName = user.UserName + "(" + orgnization.Name + ")";
            }
            
            dtos.Add(new OptionDto(user.Id, displayName, new
            {
                orgnizationId = orgnization.Id,
                orgnizationName = orgnization.Name,
                userName = user.UserName,
            }));
        }

        return ApiResult.Success(dtos);
    }

    [HttpPost]
    public async Task<ApiResult> GetSignParty(QueryRequestInput input)
    {
        var search = input.GetAttributeValue<string>("search");

        var express = new ExpressionHolder<SignParty>()
            .AddIf(true, t => t.IsEnable == 1)
            .AddIf(!string.IsNullOrEmpty(search),
                t => t.Name.Contains(search) || t.Phone.Contains(search) || t.CompanyName.Contains(search))
            .ToExpression();

        var list = await _signPartyService.GetList(express);
        var dtos = list.Select(t => new OptionDto(t.Id, t.Name, t)).ToList();
        return ApiResult.Success(dtos);
    }

    [HttpPost]
    public async Task<ApiResult> GetProvider(QueryRequestInput input)
    {
        var search = input.GetAttributeValue<string>("search");

        var express = new ExpressionHolder<Provider>()
            .AddIf(true, t => t.IsEnable == 1)
            .AddIf(!string.IsNullOrEmpty(search), t => t.Name.Contains(search) || t.Phone.Contains(search))
            .ToExpression();

        var list = await _providerService.GetList(express);
        var dtos = list.Select(t => new OptionDto(t.Id, t.Name, t)).ToList();
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
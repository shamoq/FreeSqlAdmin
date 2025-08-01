using Mapster;
using Microsoft.AspNetCore.Mvc;
using Simple.AdminApplication.Application;
using Simple.AdminApplication.Application.UserMng;
using Simple.AdminApplication.Dtos;
using Simple.AdminApplication.Entitys.UserMng;
using Simple.AdminController.Models;
using Simple.AspNetCore.Controllers;
using Simple.Utils.Consts;

namespace Simple.AdminController.Controllers;

public class OrgnizationController : AppCurdController<SysOrgnization, OrgnizationDto>
{
    private OrgnizationService _orgnizationService;

    public OrgnizationController(OrgnizationService orgnizationService) 
    {
        this._orgnizationService = orgnizationService;
    }

    [HttpPost]
    public override async Task<ApiResult> List(QueryRequestInput pageRequest)
    {
        var name = pageRequest.GetAttributeValue<string>("Name");
        var list = await _orgnizationService.GetList(t => true);
        if (!string.IsNullOrEmpty(name))
        {
            var filterList = list.Where(t => t.Name.Contains(name)).ToList();
            var filterCodes = filterList.Select(t => t.OrderFullCode).ToList();
            var result = list.Where(t => filterCodes.Any(c => c.Contains(t.OrderFullCode))).ToList();
            list = result.Union(filterList).ToList();
        }

        var orderList = list.OrderBy(t => t.OrderFullCode).ToList();
        var dtoList = orderList.Adapt<List<OrgnizationDto>>();
        TreeHelper.BuildTreeProps(dtoList, t => t.Id, t => t.ParentId, t => t.Name);
        foreach (var dto in dtoList)
        {
            var parent =  dtoList.FirstOrDefault(t=> t.Id == dto.ParentId);
            dto.SetAttributeValue("ParentName", parent?.Name);
        }
        return ApiResult.Success(dtoList);
    }
}
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Simple.AdminApplication.Application;
using Simple.AdminApplication.Entitys.Contracts;
using Simple.AdminApplication.Entitys.UserMng;
using Simple.AdminController.Models;
using Simple.AspNetCore.Controllers;
using Simple.Utils.Consts;

namespace Simple.AdminController.Controllers;

public class ContractTypeController : AppCurdController<ContractType, ContractTypeDto>
{
    private readonly ContractTypeService _service;

    public ContractTypeController(ContractTypeService service, CustomFieldService customFieldService)
    {
        this._service = service;
    }


    /// <summary>获取列表</summary>
    /// <returns></returns>
    [HttpPost, Permission("query", "查询")]
    public override async Task<ApiResult> List(QueryRequestInput pageRequest)
    {
        var search = pageRequest.GetAttributeValue<string>("search");
        pageRequest.AdditionalExpression = new ExpressionHolder<ContractType>()
            .AddIf(!string.IsNullOrEmpty(search), t => t.Name.Contains(search));
        var list = await _service.GetList(pageRequest.GetExpression<ContractType>());

        var orderList = list.OrderBy(t => t.OrderCode).ToList();
        var dtoList = orderList.Adapt<List<ContractTypeDto>>();
        TreeHelper.BuildTreeProps(dtoList, t => t.Id, t => t.ParentId, t => t.Name);

        return ApiResult.Success(dtoList);
    }

    /// <summary>获取列表，拼接一个查询全部</summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<ApiResult> GetListAndAll(QueryRequestInput pageRequest)
    {
        // 新增一个所有分类
        var list = await _service.GetList(pageRequest.GetExpression<ContractType>());
        var orderList = list.OrderBy(t => t.OrderCode).ToList();
        var dtoList = orderList.Adapt<List<ContractTypeDto>>();
        var allGuid = Guid.NewGuid();
        TreeHelper.BuildTreeProps(dtoList, t => t.Id, t => t.ParentId, t => t.Name);
        foreach (var item in dtoList)
        {
            if (item.ParentId == null || item.ParentId == Guid.Empty)
            {
                item.ParentId = allGuid;
            }
        }

        dtoList.Insert(0, new ContractTypeDto() { Id = allGuid, Name = "所有分类", ParentId = null });

        return ApiResult.Success(dtoList);
    }
  
}
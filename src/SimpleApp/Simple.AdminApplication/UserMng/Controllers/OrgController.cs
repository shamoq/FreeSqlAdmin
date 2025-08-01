using Mapster;
using Microsoft.AspNetCore.Mvc;
using Simple.AdminApplication.SysMng.Dto;
using Simple.AdminApplication.UserMng.Entities;
using Simple.AspNetCore.Controllers;

namespace Simple.AdminApplication.UserMng.Controllers;

[Permission("Org", "组织管理")]
public class OrgController : AppCurdController<SysOrg, OrgnizationDto>
{
    private readonly OrgService _orgService;

    public OrgController(OrgService orgService)
    {
        _orgService = orgService;
    }

    [HttpPost]
    [Permission("query","分页查询")]
    public override async Task<ApiResult> List(QueryRequestInput pageRequest)
    {
        var name = pageRequest.GetAttributeValue<string>("Name");
        var list = await _orgService.GetList(t => true);
        if (!string.IsNullOrEmpty(name))
        {
            var filterList = list.Where(t => t.Name.Contains(name)).ToList();
            var filterCodes = filterList.Select(t => t.OrderFullCode).ToList();
            var result = list.Where(t => filterCodes.Any(c => c.Contains(t.OrderFullCode))).ToList();
            list = result.Union(filterList).ToList();
        }

        var orderList = list.OrderBy(t => t.OrderFullCode).ToList();
        var dtoList = orderList.Adapt<List<OrgnizationDto>>();
        TreeHelper.BuildTreeProps(dtoList);
        foreach (var dto in dtoList)
        {
            var parent = dtoList.FirstOrDefault(t => t.Id == dto.ParentId);
            dto.SetAttributeValue("ParentName", parent?.Name);
        }

        return ApiResult.Success(dtoList);
    }
}
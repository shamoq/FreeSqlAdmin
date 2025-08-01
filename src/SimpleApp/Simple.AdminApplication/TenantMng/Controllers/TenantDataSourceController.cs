using FreeSql;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Simple.AdminApplication.Extensions;
using Simple.AdminApplication.Helpers;
using Simple.AdminApplication.SysMng;
using Simple.AdminApplication.TenantMng.Entities;
using Simple.AspNetCore.Controllers;
using Simple.Utils.Models.Dto;

namespace Simple.AdminApplication.TenantMng.Controllers;

[Permission("TenantDataSource", "租户数据源")]
public class TenantDataSourceController : AppCurdController<TenantDataSource, TenantDataSource>
{
    private readonly TenantService _tenantService;
    private readonly TenantDataSourceService _tenantDataSourceService;
    private readonly IConfiguration _configuration;

    public TenantDataSourceController(TenantService tenantService, TenantDataSourceService tenantDataSourceService,
        IConfiguration configuration)
    {
        _tenantService = tenantService;
        _tenantDataSourceService = tenantDataSourceService;
        _configuration = configuration;
    }

    [HttpPost]
    [Permission("query", "测试连接")]
    public async Task<ApiResult> Connect(ParameterModel input)
    {
        var (dbType, connectionString) = input.GetAttributeValue<string, string>("dbType", "connectionString");

        var sqlDbType = TypeConvertHelper.ConvertType<DataType>(dbType);

        var canConnect = await FreeSqlDbHelper.CanConnect(sqlDbType, connectionString);
        return ApiResult.Success(canConnect);
    }

    [HttpPost]
    [Permission("query", "分页查询")]
    public override async Task<ApiResult> Page(QueryRequestInput pageRequest)
    {
        var (total, list) = await Service.Page(pageRequest);
        var summarries = _tenantService.All.GroupBy(t => t.DataSourceId).Select(t => new
        {
            DataSourceId = t.Key,
            Total = t.Count(),
        }).ToList();
        foreach (var dataSource in list)
        {
            var summarry = summarries.FirstOrDefault(t => t.DataSourceId == dataSource.Id);
            dataSource.SetAttributeValue("tenantCount", summarry?.Total ?? 0);

            // 判断是否能连接
            var connectionString = dataSource.ConnectionString;
            var dbType = TypeConvertHelper.ConvertType<DataType>(dataSource.DbType);
            var canConnect = await FreeSqlDbHelper.CanConnect(dbType, connectionString);
            dataSource.SetAttributeValue("status", canConnect ? 1 : 0);
        }


        return ApiResult.Success(new { total, data = list });
    }


    [HttpPost]
    [Permission("query", "单个查询")]
    public override async Task<ApiResult> Get(ParameterModel model)
    {
        if (!model.Id.HasValue)
            return ApiResult.Fail("没有获取到参数 ");
        var entity = await Service.FindAsync(model.Id.Value);
        if (entity is null)
            return ApiResult.Success();

        var count = await _tenantService.All.Where(t => t.DataSourceId == model.Id).CountAsync();
        entity.SetAttributeValue("tenantCount", count);

        return ApiResult.Success(entity);
    }

    [HttpPost]
    [Permission("query", "选项查询")]
    public async Task<ApiResult> Option(QueryRequestInput input)
    {
        var list = await _tenantDataSourceService.GetList(input);
        var manageDataSource = _configuration.GetManagerDataSource();
        list.Insert(0, manageDataSource);
        var options = list.Adapt<List<OptionDto>>();
        return ApiResult.Success(options);
    }
}
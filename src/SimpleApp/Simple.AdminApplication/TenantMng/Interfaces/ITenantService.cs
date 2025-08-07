using System.Linq.Expressions;
using Simple.AdminApplication.TenantMng.Dto;
using Simple.AdminApplication.TenantMng.Entities;
using Simple.AdminApplication.UserMng.Dto;
using Simple.Utils.Interface;

namespace Simple.AdminApplication.TenantMng.Interfaces;

public interface ITenantService
{
    /// <summary>
    /// 注册租户数据库
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="tenantTypes"></param>
    void Register(TenantDataSource dataSource, Type[] tenantTypes);


    /// <summary>
    /// 查询租户
    /// </summary>
    /// <param name="where"></param>
    /// <returns></returns>
    Task<List<Tenant>> GetList(Expression<Func<Tenant, bool>> where);


    /// <summary>
    /// 根据租户获取授权菜单，数组结构
    /// </summary>
    /// <returns></returns>
    Task<List<TenantGrantRightDto>> GetTenantGrantArray();
    
    /// <summary>
    /// 根据租户获取授权菜单，树形结构
    /// </summary>
    /// <returns></returns>
    Task<List<TenantAppRightDto>> GetTenantGrantTree();
}
using System.Linq.Expressions;
using FreeSql;
using FreeSql.Internal.Model;
using Microsoft.Extensions.DependencyInjection;
using Simple.AdminApplication.Helpers;
using Simple.AspNetCore;
using Simple.Utils.Interface;
using Simple.Utils.Models;
using Simple.Utils.Models.Entity;

namespace Simple.AdminApplication.Common;

public class BaseCurdService<T> : ICurdService<T> where T : DefaultEntity
{
    protected IBaseRepository<T> table;
    protected IFreeSql fsql;

    public BaseCurdService()
    {
        fsql = HostServiceExtension.ServiceProvider.GetService<IFreeSql>();
        table = fsql.GetRepository<T>();
    }

    public ISelect<T> All => table.Select;

    // 新增操作实现
    public virtual Task<T> AddAsync(T entity)
    {
        return table.InsertAsync(entity);
    }

    public virtual Task AddAsync(List<T> entities)
    {
        return table.InsertAsync(entities);
    }

    // 更新操作实现
    public virtual Task UpdateAsync(T entity)
    {
        return table.UpdateAsync(entity);
    }

    public async Task<int> UpdateRangeAsync(List<T> entities)
    {
        return await table.UpdateAsync(entities);
    }

    public virtual async Task<T> Save(T entity, bool isForceAdd = false)
    {
        if (isForceAdd || entity.Id == Guid.Empty)
        {
            await table.InsertAsync(entity);
        }
        else
        {
            await table.UpdateAsync(entity);
        }

        return entity;
    }

    // 删除操作实现
    public virtual async Task<bool> DeleteAsync(T entity)
    {
        return await table.DeleteAsync(entity) > 0;
    }

    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        return await table.DeleteAsync(a => a.Equals(id)) > 0;
    }

    public virtual async Task<int> DeleteAsync(List<T> entities)
    {
        return await table.DeleteAsync(entities);
    }

    public virtual async Task<int> DeleteAsync(Expression<Func<T, bool>> predicate)
    {
        return await table.DeleteAsync(predicate);
    }

    public virtual Task<T> SetFlag(ParameterModel model)
    {
        // return table.SetFlag(model);
        return null;
    }

    public virtual Task<T> FindAsync(Guid id)
    {
        return table.Where(t => t.Id == id).FirstAsync();
    }

    public virtual Task<T> FindAsync(Expression<Func<T, bool>> where)
    {
        return table.Where(where).ToOneAsync<T>();
    }

    public virtual async Task<(int, List<T>)> Page(QueryRequestInput pageRequest)
    {
        // 手动传递的查询条件
        var additionalExpression = pageRequest.AdditionalExpression != null &&
                                   pageRequest.AdditionalExpression.InnerExpression is Expression<Func<T, bool>>
                                       additionalExpressionFunc
            ? additionalExpressionFunc
            : null;

        var dynamicFilter = FreeSqlFilterHelper.GetDynamicFilterInfo<T>(pageRequest.Filters);
        var query = fsql.Select<T>().WhereDynamicFilter(dynamicFilter)
            .WhereIf(additionalExpression != null, additionalExpression);

        // 默认按创建时间倒序
        if (string.IsNullOrEmpty(pageRequest.SortField))
        {
            query = query.OrderByDescending(a => a.CreatedTime);
        }
        else
        {
            query = query.OrderByPropertyName(pageRequest.SortField,
                string.Equals(pageRequest.SortType, "asc", StringComparison.OrdinalIgnoreCase));
        }

        // 分页查询
        var total = await query.CountAsync();
        if (total == 0)
        {
            return (0, new List<T>());
        }

        var list = await query.Page(pageRequest.Page, pageRequest.PageSize).ToListAsync();

        return ((int)total, list);
    }

    public virtual async Task<List<T>> GetList(QueryRequestInput pageRequest)
    {
        var dynamicFilter = FreeSqlFilterHelper.GetDynamicFilterInfo<T>(pageRequest.Filters);
        var query = fsql.Select<T>().WhereDynamicFilter(dynamicFilter)
            .OrderByPropertyNameIf(string.IsNullOrEmpty(pageRequest.SortField),
                pageRequest.SortField, string.Equals(pageRequest.SortType, "asc", StringComparison.OrdinalIgnoreCase));

        // 分页查询
        var list = await query.ToListAsync();

        return list;
    }

    public virtual Task<List<T>> GetList(Expression<Func<T, bool>> where)
    {
        return table.Where(where).ToListAsync();
    }
}
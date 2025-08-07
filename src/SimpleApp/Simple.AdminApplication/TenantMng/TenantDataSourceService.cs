using FreeSql;
using Simple.AdminApplication.Common;
using Simple.AdminApplication.Extensions;
using Simple.AdminApplication.TenantMng.Entities;
using Simple.AdminApplication.TenantMng.Interfaces;
using Simple.Utils.Exceptions;

namespace Simple.AdminApplication.TenantMng;

/// <summary>
/// 租户数据源服务
/// </summary>
[Scoped]
public class TenantDataSourceService : BaseCurdService<TenantDataSource>
{
    private ITenantService tenantService;

    public TenantDataSourceService(ITenantService tenantService)
    {
        this.tenantService = tenantService;
    }

    public override async Task<TenantDataSource> Save(TenantDataSource entity, bool isForceAdd = false)
    {
        var needSync = false;
        // 修改模式
        if (entity.Id != Guid.Empty)
        {
            var oldEntity = await FindAsync(entity.Id);
            if (entity.ConnectionString != oldEntity.ConnectionString)
            {
                // 检查数据源下是否已经有了租户，如果有了租户，不允许修改数据源
                var tenants = await tenantService.GetList(t => t.DataSourceId == entity.Id);
                if (tenants.Any())
                {
                    throw new CustomException("数据源下有租户，不允许修改连接信息");
                }
                needSync = true;
            }

            // 判断是否存在同名连接
            var exists = await table.Where(t => t.ConnectionString == entity.ConnectionString && t.Id != entity.Id)
                .AnyAsync();
            if (exists)
            {
                throw new CustomException("数据连接已存在，不允许重复修改");
            }

            await table.UpdateAsync(entity);
        }
        else
        {
            // 判断是否存在同名连接
            var exists = await table.Where(t => t.ConnectionString == entity.ConnectionString).AnyAsync();
            if (exists)
            {
                throw new CustomException("数据连接已存在，不允许重复添加");
            }

            await table.InsertAsync(entity);
            
            needSync = true;
        }

        if (needSync)
        {
            tenantService.Register(entity, TenantTypeExtension.GetTenantTypes());
        }
        

        return entity;
    }


    public override async Task<bool> DeleteAsync(Guid id)
    {
        // 检查数据源下是否已经有了租户，如果有了租户，不允许删除数据源
        var tenants = await tenantService.GetList(t => t.DataSourceId == id);
        if (tenants.Any())
        {
            throw new CustomException("数据源下有租户，不允许删除");
        }
        
        await base.DeleteAsync(id);

        return true;
    }
}
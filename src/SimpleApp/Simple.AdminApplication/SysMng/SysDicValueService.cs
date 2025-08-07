using Simple.AdminApplication.Common;
using Simple.AdminApplication.SysMng.Entities;
using Simple.FreeSql.Helpers;

namespace Simple.AdminApplication.SysMng;

[Scoped]
public class SysDicValueService : BaseCurdService<SysDicValue>
{
    public override async Task<SysDicValue> Save(SysDicValue entity, bool isForceAdd = false)
    {
        await new TreeEntityHelper<SysDicValue>(fsql).Save(entity);
        return entity;
    }
}
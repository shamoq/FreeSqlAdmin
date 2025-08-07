using Mapster;
using Simple.AdminApplication.Common;
using Simple.AdminApplication.Extensions;
using Simple.AdminApplication.UserMng.Entities;
using Simple.FreeSql.Helpers;
using Simple.Interfaces;
using Simple.Interfaces.Dtos;
using Simple.Utils.Consts;
using Simple.Utils.Exceptions;

namespace Simple.AdminApplication.UserMng;

[Scoped]
public class OrgService : BaseCurdService<SysOrg>
{
    public override async Task<SysOrg> Save(SysOrg entity, bool isForceAdd = false)
    {
        var hasData =
            await All.AnyAsync(
                t => t.Name == entity.Name && t.Id != entity.Id && t.ParentId == entity.ParentId);
        if (hasData)
            throw new CustomException("组织名称已存在");

        await new TreeEntityHelper<SysOrg>(fsql).Save(entity);
        return entity;
    }

    public override async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await table.Where(t => t.Id == id).FirstAsync();
        // 校验是否有子级数据
        var hasChild = await All.AnyAsync(t => t.ParentId == entity.Id);
        if (hasChild)
            throw new CustomException("请先删除子级");

        var orgnization = await table.Where(t => t.Id == entity.Id).FirstAsync(); ;
        if (orgnization is null)
            throw new CustomException("数据不存在或已删除");

        // 组织下有用户，无法删除
        var hasUser = await fsql.Select<SysUser>().Where(t => t.OrgId == entity.Id).AnyAsync();
        if (hasUser)
        {
            throw new CustomException("存在用户，无法删除");
        }

        var count = await table.Select.Where(t => true).CountAsync();
        if (count <= 1)
        {
            throw new CustomException("不能删除最后一个组织");
        }

        await table.DeleteAsync(entity);

        return true;
    }

    public async Task<OrgEntityDto> GetById(Guid id)
    {
        var entity = await table.Where(t => t.Id == id).FirstAsync();
        return entity.Adapt<OrgEntityDto>();
    }
}
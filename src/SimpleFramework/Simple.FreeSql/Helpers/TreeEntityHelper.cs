using Simple.Utils.Extensions;
using Simple.Utils.Models.Entity;

namespace Simple.FreeSql.Helpers;

public class TreeEntityHelper<T> where T : class, ITreeEntity
{
    private readonly IFreeSql fsql;

    public TreeEntityHelper(IFreeSql fsql)
    {
        this.fsql = fsql;
    }

    /// <summary>
    /// 保存并设置树相关属性，排序编码，全编码，全名称
    /// </summary>
    /// <param name="entity"></param>
    public async Task Save(T entity)
    {
        var repo = fsql.GetRepository<T>();
        var all = await repo.Select.ToListAsync();

        var parent = all.FirstOrDefault(t => t.Id == entity.ParentId);
        var oldData = all.FirstOrDefault(t => t.Id == entity.Id);

        if (oldData == null)
        {
            // 获取当前父级下所有子级的最大排序编码
            var maxChildOrderCode = all
                .Where(t => t.ParentId == entity.ParentId)
                .Max(t => t.OrderCode);

            if (string.IsNullOrEmpty(maxChildOrderCode))
            {
                maxChildOrderCode = "0000";
            }

            // 将最大排序编码转换为整数并加 1
            int nextOrderNumber = maxChildOrderCode.AsInt() + 1;

            // 将新的排序编码格式化为 4 位数字
            string nextOrderCode = nextOrderNumber.ToString("D4");

            // 生成完整的排序编码
            string fullOrderCode = parent == null ? nextOrderCode : $"{parent.OrderFullCode}.{nextOrderCode}";

            // 设置模型的排序编码和完整排序编码
            entity.OrderCode = nextOrderCode;
            entity.OrderFullCode = fullOrderCode;
            entity.FullName = parent != null ? parent.FullName + "-" + entity.Name : entity.Name;

            await repo.InsertAsync(entity);
        }
        else
        {
            await repo.UpdateAsync(entity);
           
            // 修改模式，要判断名称是否发生变化
            if (oldData.Name != entity.Name)
            {
                all.Remove(oldData);
                all.Add(entity);
                RefreshChildrenFullName(entity, all);
                await repo.UpdateAsync(all);
            }
        }
    }

    private void RefreshChildrenFullName(T parent, List<T> all)
    {
        var children = all.Where(t => t.ParentId == parent?.Id).ToList();
        foreach (var child in children)
        {
            if (child.ParentId == parent.Id)
            {
                child.FullName = parent.FullName + "-" + child.Name;
            }

            RefreshChildrenFullName(child, all);
        }
    }
}
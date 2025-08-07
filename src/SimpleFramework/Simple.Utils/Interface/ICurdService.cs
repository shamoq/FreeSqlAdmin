using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Simple.Utils.Models;

namespace Simple.Utils.Interface
{
    /// <summary>
    /// 实现增删改查的接口 用于快速搭建基于领域的api
    /// 核心用于前端快速Curd
    /// </summary>
    public interface ICurdService<T> where T : class
    {
        Task<T> AddAsync(T entity);

        /// <summary>
        /// 数据保存，默认根据Id判断是否是新增，如果Id为空，则添加状态，否则修改状态
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="isForceAdd">强制新增，用于指定Id新增场景</param>
        /// <returns> </returns>
        Task<T> Save(T entity, bool isForceAdd = false);

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id"> </param>
        /// <returns> </returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// 设置flag属性
        /// </summary>
        /// <param name="model"> </param>
        /// <returns> </returns>
        Task<T> SetFlag(ParameterModel model);

        Task<T> FindAsync(Guid id);

        Task<T> FindAsync(Expression<Func<T, bool>> where);

        /// <summary>
        /// 条件分页
        /// </summary>
        /// <returns> </returns>
        Task<(int, List<T>)> Page(QueryRequestInput pageRequest);

        Task<List<T>> GetList(QueryRequestInput pageRequest);
        
        Task<List<T>> GetList(Expression<Func<T, bool>> where);
    }
}
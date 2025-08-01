// using Microsoft.EntityFrameworkCore;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Linq.Expressions;
// using System.Text;
// using System.Threading.Tasks;
// using Simple.Utils.Attributes;
// using Simple.Utils.Models;
// using Simple.Utils.Models.Entity;
// using Simple.Utils.Helper;
// using Simple.Utils.Interface;
// using Simple.Utils.Exceptions;
//
// namespace Simple.EntityFrameworkCore
// {
//     public class EFCurdService<T> : ICurdService<T> where T : DefaultEntity
//     {
//         public readonly AppDbContext appDb;
//         protected DbSet<T> table;
//
//         public EFCurdService(AppDbContext appDb)
//         {
//             this.appDb = appDb;
//             table = appDb.Set<T>();
//         }
//
//         /// <summary>
//         /// 数据保存，默认根据Id判断是否是新增，如果Id为空，则添加状态，否则修改状态
//         /// </summary>
//         /// <param name="entity"></param>
//         /// <param name="isForceAdd">强制新增，用于指定Id新增场景</param>
//         /// <returns></returns>
//         public virtual async Task<T> Save(T entity, bool isForceAdd = false)
//         {
//             // 如果实体的Id为空，则添加状态
//             if (isForceAdd || entity.Id == Guid.Empty)
//             {
//                 appDb.Entry(entity).State = EntityState.Added;
//             }
//             else
//             {
//                 // 如果实体未跟踪，则添加状态
//                 if (appDb.Entry(entity).State == EntityState.Detached)
//                 {
//                     appDb.Update(entity);
//                 }
//                 // else
//                 // {
//                 //     var oldEntity = await appDb.FindAsync<T>(entity.Id);
//                 //     if (oldEntity is null)
//                 //     {
//                 //         appDb.Entry(entity).State = EntityState.Added;
//                 //     }
//                 //     else
//                 //     {
//                 //         var entry = appDb.Attach(oldEntity);
//                 //         foreach (var property in entry.Properties)
//                 //         {
//                 //             var newValue = entry.Entity.GetType().GetProperty(property.Metadata.Name).GetValue(entity);
//                 //             if (!Equals(property.CurrentValue, newValue))
//                 //             {
//                 //                 property.CurrentValue = newValue;
//                 //                 property.IsModified = true;
//                 //             }
//                 //         }
//                 //     }
//                 // }
//             }
//
//             return entity;
//         }
//
//         public virtual async Task<T> Delete(Guid id)
//         {
//             var entity = await appDb.FindAsync<T>(id);
//             if (entity is null)
//                 return null;
//
//             appDb.Remove(entity);
//
//             return entity;
//         }
//
//         public virtual async Task SaveChanges()
//         {
//             await appDb.SaveChangesAsync();
//         }
//
//         public virtual async Task<T> FindById(Guid id)
//         {
//             var entity = await appDb.FindAsync<T>(id);
//
//             return entity;
//         }
//
//         public virtual async Task<T> FindBy(Expression<Func<T, bool>> where)
//         {
//             if (where == null)
//             {
//                 throw new ArgumentNullException(nameof(where), "where 表达式不能为空");
//             }
//
//             var entity = await table.Where(where).FirstOrDefaultAsync();
//
//             return entity;
//         }
//
//         public virtual async Task<List<T>> GetList(Expression<Func<T, bool>> where)
//         {
//             if (where == null)
//             {
//                 var list = await table.ToListAsync();
//                 return list;
//             }
//             else
//             {
//                 var list = await table.Where(where).ToListAsync();
//                 return list;
//             }
//         }
//
//         public virtual async Task<(int, List<T>)> Page(QueryRequestInput pageRequest)
//         {
//             var list = table.AsQueryable();
//             var where = pageRequest.GetExpression<T>();
//             if (where != null)
//             {
//                 list = list.Where(where);
//             }
//
//             // 根据排序字段和排序方向进行排序
//             if (!string.IsNullOrEmpty(pageRequest.SortField))
//             {
//                 var parameter = Expression.Parameter(typeof(T), "x");
//                 var property = Expression.Property(parameter, pageRequest.SortField);
//                 var lambda =
//                     Expression.Lambda<Func<T, object>>(Expression.Convert(property, typeof(object)), parameter);
//
//                 list = pageRequest.SortType ? list.OrderBy(lambda) : list.OrderByDescending(lambda);
//             }
//             else
//             {
//                 // 默认按创建时间倒序
//                 list = list.OrderByDescending(t => t.CreatedTime);
//             }
//
//             var count = await list.CountAsync();
//             if (count == 0)
//             {
//                 return (0, new List<T>());
//             }
//             var data = await list.Skip((pageRequest.Page - 1) * pageRequest.PageSize)
//                 .Take(pageRequest.PageSize)
//                 .ToListAsync();
//             return (count, data);
//         }
//
//         public virtual async Task<T> SetFlag(ParameterModel model)
//         {
//             var entity = appDb.Find<T>(model.Id);
//             if (entity is null)
//                 throw new CustomException("根据Id 查询不到数据");
//             ;
//
//             var update = appDb.Attach(entity);
//
//             foreach (var item in model.AdditionalData)
//             {
//                 try
//                 {
//                     var field = update.Properties.FirstOrDefault(p =>
//                         p.Metadata.Name.Equals(item.Key, StringComparison.OrdinalIgnoreCase));
//
//                     if (field != null)
//                     {
//                         field.CurrentValue = TypeConvertHelper.ConvertType(item.Value, field.Metadata.ClrType);
//                         field.IsModified = true;
//                     }
//                 }
//                 catch (Exception)
//                 {
//                     throw new FatalException($"模型{typeof(T)}中不存在属性 {item.Key}");
//                 }
//             }
//
//             return entity;
//         }
//     }
// }
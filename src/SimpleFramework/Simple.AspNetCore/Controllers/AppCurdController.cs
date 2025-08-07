using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Simple.Utils;
using Simple.Utils.Models;
using Simple.Utils.Models.Entity;
using Simple.Utils.Helper;
using Simple.Utils.Interface;
using Simple.Utils.Attributes;
using Simple.Utils.Models.Dto;

namespace Simple.AspNetCore.Controllers
{
    /// <summary>快速实现增删改查的控制器基类</summary>
    [ApiController]
    public abstract class AppCurdController<TEntity, TDto> : AppAuthController where TEntity : DefaultEntity, new()
    {
        private ICurdService<TEntity> _service;

        // ... existing code ...
        protected ICurdService<TEntity> Service
        {
            get
            {
                if (_service != null)
                {
                    return _service;
                }

                _service = HttpContext.RequestServices.GetRequiredService<ICurdService<TEntity>>();

                return _service;
            }
        }

        /// <summary>编辑</summary>
        /// <returns></returns>
        [HttpPost, Permission("edit", "编辑")]
        public virtual async Task<ApiResult> Save(ParameterModel model)
        {
            if (model == null || model.AdditionalData?.Count == 0)
                return ApiResult.Fail("没有获取到对象参数 model ");

            if (model.Id.HasValue)
            {
                var entity = await Service.FindAsync(model.Id.Value);
                if (entity != null)
                {
                    ObjectHelper.TryFromDict(entity, model.AdditionalData);
                    await Service.Save(entity);
                    var dto = entity.Adapt<TDto>();
                    return ApiResult.Success(dto);
                }
                else
                {
                    // 查询不到，默认是带Id新增模式
                    entity = model.Read<TEntity>();
                    await Service.Save(entity);
                    entity.Id = model.Id.Value;
                    var dto = entity.Adapt<TDto>();
                    return ApiResult.Success(dto);
                }
            }
            else
            {
                var entity = model.Read<TEntity>();
                await Service.Save(entity);
                var dto = entity.Adapt<TDto>();
                return ApiResult.Success(dto);
            }
        }

        /// <summary>物理删除</summary>
        /// <returns></returns>
        [HttpPost, Permission("delete", "删除")]
        public virtual async Task<ApiResult> Delete(ParameterModel model)
        {
            if (!model.Id.HasValue)
                return ApiResult.Fail("没有获取到参数 ");
            var entity = await Service.DeleteAsync(model.Id.Value);
            return ApiResult.Success(entity);
        }

        /// <summary>设置标量值</summary>
        /// <returns></returns>
        [HttpPost, Permission("edit", "设置")]
        public virtual async Task<ApiResult> SetFlag(ParameterModel model)
        {
            if (model == null || model.AdditionalData.Count == 0)
                return ApiResult.Fail("没有获取到对象参数 model ");
            var entity = await Service.SetFlag(model);
            var dto = entity.Adapt<TDto>();
            return ApiResult.Success(dto);
        }

        /// <summary>获取模型</summary>
        /// <returns></returns>
        [HttpPost, Permission("query", "查询")]
        public virtual async Task<ApiResult> Get(ParameterModel model)
        {
            if (!model.Id.HasValue)
                return ApiResult.Fail("没有获取到参数 ");
            var entity = await Service.FindAsync(model.Id.Value);
            if (entity is null)
                return ApiResult.Success();
            var dto = entity.Adapt<TDto>();
            return ApiResult.Success(dto);
        }

        /// <summary>获取列表</summary>
        /// <returns></returns>
        [HttpPost, Permission("query", "查询")]
        public virtual async Task<ApiResult> List(QueryRequestInput pageRequest)
        {
            var list = await Service.GetList(pageRequest);

            var dtoList = list.Adapt<List<TDto>>();
            if (typeof(ITreeDto).IsAssignableFrom(typeof(TDto)))
            {
                var treeList = dtoList.Cast<ITreeDto>().ToList();
                TreeHelper.BuildTreeProps(treeList);
            }

            return ApiResult.Success(dtoList);
        }

        /// <summary>获取分页列表</summary>
        /// <returns></returns>
        [HttpPost, Permission("query", "查询")]
        public virtual async Task<ApiResult> Page(QueryRequestInput pageRequest)
        {
            var (total, list) = await Service.Page(pageRequest);
            var dtoList = list.Adapt<List<TDto>>();
            if (typeof(ITreeDto).IsAssignableFrom(typeof(TDto)))
            {
                var treeList = dtoList.Cast<ITreeDto>().ToList();
                TreeHelper.BuildTreeProps(treeList);
            }

            return ApiResult.Success(new { total, data = dtoList });
        }

        /// <summary>获取树列表</summary>
        /// <returns></returns>
        [HttpPost, Permission("query", "查询")]
        public virtual async Task<ApiResult> Tree(QueryRequestInput pageRequest)
        {
            var list = await Service.GetList(pageRequest);

            var dtoList = list.Adapt<List<TDto>>();
            if (typeof(ITreeDto).IsAssignableFrom(typeof(TDto)))
            {
                var returnType = pageRequest.GetAttributeValue<string>("return");
                // 如果是树结构，转换为树结构
                var treeArrayList = dtoList.Cast<ITreeDto>().ToList();

                if (Equals(returnType, "list"))
                {
                    TreeHelper.BuildTreeProps(treeArrayList);
                    return ApiResult.Success(treeArrayList);
                }
                else
                {
                    var tree = TreeHelper.ListToTree(treeArrayList);
                    var withAll = pageRequest.GetAttributeValue<bool?>("withAll") ?? false;
                    if (withAll)
                    {
                        var root = new TreeNode<ITreeDto>()
                        {
                            Id = Guid.Empty,
                            Name = "全部",
                            Children = tree,
                            ParentId = null,
                            Data = null
                        };
                        return ApiResult.Success(new List<TreeNode<ITreeDto>>() { root });
                    }
                    return ApiResult.Success(tree);
                }
            }

            return ApiResult.Success(dtoList);
        }
        
         
    }
}
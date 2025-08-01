using Simple.Utils.Models.Entity;
using System.Collections.Generic;

namespace Simple.Utils.Helper;

public class TreeBuilder<T> where T : ITreeEntity
{
    private const int MaxLevel = 30;

    private readonly Dictionary<Guid, T> _dictionary;
    private readonly IList<T> _items;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="items"></param>
    public TreeBuilder(IList<T> items)
    {
        this._items = items.Where(model => model != null).ToList();
        _dictionary = ToDictionary(items);
    }
    
    /// <summary>
    /// 查询并且带出自身的递归父节点
    /// </summary>
    /// <returns></returns>
    public List<T> GetParents(Guid id, bool isSelfIncluded = true)
    {
        if (_dictionary.TryGetValue(id, out T item))
        {
            var children = new List<T>() { item };
            return GetParents(children, isSelfIncluded);
        }
        else
        {
            return new List<T>();
        }
    }
    
    /// <summary>
    /// 查询并且带出自身的递归子节点
    /// </summary>
    /// <returns></returns>
    public List<T> GetChildren(Guid id, bool isSelfIncluded = true)
    {
        if (_dictionary.TryGetValue(id, out T item))
        {
            var parents = new List<T>() { item };
            return GetChildren(parents, isSelfIncluded);
        }
        else
        {
            return new List<T>();
        }
    }

    /// <summary>
    /// 查询并且带出自身的递归父节点
    /// </summary>
    /// <returns></returns>
    public List<T> GetParents(Func<T, bool> predicate, bool isSelfIncluded = true)
    {
        List<T> children = this._items.Where(predicate).ToList();

        return children.Count == 0 ? children : GetParents(children, isSelfIncluded);
    }

    /// <summary>
    /// 查询并且带出自身的递归子节点
    /// </summary>
    /// <returns></returns>
    public List<T> GetChildren(Func<T, bool> predicate, bool isSelfIncluded = true)
    {
        List<T> parents = this._items.Where(predicate).ToList();

        return parents.Count == 0 ? parents : GetChildren(parents, isSelfIncluded);
    }

    /// <summary>
    /// 找出这些搜索对象的上下级所有递归节点
    /// </summary>
    /// <param name="searchobj"></param>
    /// <returns></returns>
    public List<T> GetFullTree(IList<T> searchobj)
    {
        //计数器，防止死循环，树的层级最大为30
        int counter = 0;
        var results = ToDictionary(searchobj);

        //找到所有的父级，包含自己
        IList<T> parents = searchobj;
        //当父列表为空的时（已经查到了所有根节点）或者当计数器到最大时停止循环
        while (parents.Count > 0 && counter < MaxLevel)
        {
            parents = GetDirectParents(parents).ToList();
            foreach (var parent in parents)
            {
                results.TryAdd(parent.Id, parent);
            }

            counter++;
        }

        //找到所有的子集，不包含自己
        counter = 0;
        IList<T> children = GetDirectChildren(searchobj).ToList();
        ToDictionary(children, results);
        //当父列表为空的时（已经查到了所有根节点）或者当计数器到最大时停止循环
        while (children.Count > 0 && counter < MaxLevel)
        {
            children = GetDirectChildren(children).ToList();
            foreach (var child in children)
            {
                results.TryAdd(child.Id, child);
            }

            counter++;
        }

        return results.Values.ToList();
    }

    /// <summary>
    /// 找出这些Children的所有递归父节点，isSelfIncluded：结果是或否也包含自己
    /// </summary>
    /// <param name="children"></param>
    /// <param name="isSelfIncluded"></param>
    /// <returns></returns>
    public List<T> GetParents(IList<T> children, bool isSelfIncluded)
    {
        //计数器，防止死循环，树的层级最大为30
        int counter = 0;

        //如果结果包含Children
        IList<T> parents = isSelfIncluded ? children : GetDirectParents(children).ToList();
        var results = ToDictionary(parents);

        //当父列表为空的时（已经查到了所有根节点）或者当计数器到最大时停止循环
        while (parents.Count > 0 && counter < MaxLevel)
        {
            parents = GetDirectParents(parents).ToList();
            foreach (var parent in parents)
            {
                results.TryAdd(parent.Id, parent);
            }

            counter++;
        }

        return results.Values.ToList();
    }

    /// <summary>
    /// 找出这些Parent所有的递归子节点，isSelfIncluded：结果是或否也包含自己
    /// </summary>
    /// <returns></returns>
    public List<T> GetChildren(IList<T> parents, bool isSelfIncluded)
    {
        //计数器，防止死循环，树的层级最大为30
        int counter = 0;
        var results = new Dictionary<Guid, T>();

        //如果结果包含Children           
        var children = isSelfIncluded ? parents : GetDirectChildren(parents).ToList();
        ToDictionary(children, results);

        //当父列表为空的时（已经查到了所有根节点）或者当计数器到最大时停止循环
        while (children.Count > 0 && counter < MaxLevel)
        {
            children = GetDirectChildren(children).ToList();
            foreach (var child in children)
            {
                results.TryAdd(child.Id, child);
            }

            counter++;
        }

        return results.Values.ToList();
    }

    /// <summary>
    /// 查找直接父级节点（没有递归）
    /// </summary>
    /// <param name="children"></param>
    /// <returns></returns>
    private IEnumerable<T> GetDirectParents(IList<T> children)
    {
        foreach (var child in children)
        {
            if (child.ParentId != null)
            {
                if (_dictionary.TryGetValue(child.ParentId.Value, out var parent))
                {
                    yield return parent;
                }
            }
        }
    }

    /// <summary>
    /// 查找直接子级节点（没有递归）
    /// </summary>
    /// <returns></returns>
    private IEnumerable<T> GetDirectChildren(IList<T> parents)
    {
        foreach (var parent in parents)
        {
            var children = this._items.Where(child => child.ParentId == parent.Id);
            foreach (var child in children)
            {
                yield return child;
            }
        }
    }

    private Dictionary<Guid, T> ToDictionary(IList<T> items)
    {
        Dictionary<Guid, T> dictionary = new Dictionary<Guid, T>();
        foreach (var item in items)
        {
            dictionary[item.Id] = item;
        }

        return dictionary;
    }

    private Dictionary<Guid, T> ToDictionary(IList<T> items, Dictionary<Guid, T> dict)
    {
        foreach (var item in items)
        {
            dict[item.Id] = item;
        }

        return dict;
    }
}
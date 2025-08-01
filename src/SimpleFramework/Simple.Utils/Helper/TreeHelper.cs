using Simple.Utils.Models.Dto;
using Simple.Utils.Models.Entity;

namespace Simple.Utils.Helper;

public class TreeHelper
{
    /// <summary>
    /// 获取子节点列表
    /// </summary>
    /// <param name="list"></param>
    /// <param name="parentId"></param>
    /// <param name="withParent">是否包含查询的父级</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static List<T> GetChildren<T>(List<T> list, Guid parentId, bool withParent) where T : ITreeDto
    {
        if (list == null || !list.Any())
            return new List<T>();

        var result = new List<T>();
        var parent = list.FirstOrDefault(x => x.Id == parentId);

        // 如果需要包含父节点本身
        if (withParent && parent != null)
        {
            result.Add(parent);
        }

        // 递归获取所有子节点
        GetChildrenRecursive(list, parentId, result);

        return result;
    }

    private static void GetChildrenRecursive<T>(List<T> list, Guid parentId, List<T> result) where T : ITreeDto
    {
        var children = list.Where(x => x.ParentId == parentId).ToList();
        foreach (var child in children)
        {
            result.Add(child);
            GetChildrenRecursive(list, child.Id, result);
        }
    }

    /// <summary>
    /// 为对象列表中的每个对象添加 isLeaf 属性
    /// </summary>
    public static List<T> BuildTreeProps<T>(List<T> list) where T : ITreeDto
    {
        foreach (var item in list)
        {
            var id = item.Id;

            var hasChild = list.Any(x => x.ParentId == id);
            var fullName = CalculateFullName(list, item);

            if (item is BaseDto dto)
            {
                dto.SetAttributeValue("isLeaf", !hasChild);
                dto.SetAttributeValue("fullName", fullName);
            }
            else if (item is DefaultEntity entity)
            {
                entity.SetAttributeValue("isLeaf", !hasChild);
                entity.SetAttributeValue("fullName", fullName);
            }
        }

        return list;
    }

    private static string CalculateFullName<T>(List<T> list, T item) where T : ITreeDto
    {
        var nameStack = new Stack<string>();
        var current = item;

        while (true)
        {
            var name = current.Name;
            nameStack.Push(name);

            var pId = current.ParentId;
            if (pId == Guid.Empty)
            {
                break;
            }

            var parent = list.FirstOrDefault(x => x.Id == pId);
            if (parent == null)
            {
                break;
            }

            current = parent;
        }

        return string.Join("-", nameStack);
    }

    private static string CalculateFullName<T>(List<T> list, Func<T, Guid> idFunc, Func<T, Guid?> pIdFunc,
        Func<T, string> nameFunc, T item)
    {
        var nameStack = new Stack<string>();
        var current = item;

        while (true)
        {
            var name = nameFunc(current);
            nameStack.Push(name);

            var pId = pIdFunc(current);
            if (pId == Guid.Empty)
            {
                break;
            }

            var parent = list.FirstOrDefault(x => idFunc(x) == pId);
            if (parent == null)
            {
                break;
            }

            current = parent;
        }

        return string.Join("-", nameStack);
    }

    // 列表转树（优化版本：自动识别根节点）
    public static List<TreeNode<T>> ListToTree<T>(List<T> list) where T : ITreeDto
    {
        if (list == null || !list.Any())
            return new List<TreeNode<T>>();

        // 创建节点字典并查找所有根节点
        var nodeDict = new Dictionary<object, TreeNode<T>>();
        var rootNodes = new HashSet<TreeNode<T>>();

        // 第一遍遍历：创建所有节点并标记潜在的根节点
        foreach (var item in list)
        {
            var id = item.Id;
            var parentId = item.ParentId;

            var node = new TreeNode<T>
            {
                Data = item,
                Id = id,
                Name = item.Name,
                ParentId = parentId
            };

            nodeDict[id] = node;

            // 如果父ID为null，或者父ID不在当前列表中，则视为根节点
            if (parentId == null || !list.Any(x => Equals(x.Id, parentId)))
            {
                rootNodes.Add(node);
            }
        }

        // 第二遍遍历：构建树结构
        foreach (var node in nodeDict.Values)
        {
            // 跳过根节点
            if (rootNodes.Contains(node))
                continue;

            // 找到父节点并添加到其子节点列表
            if (nodeDict.TryGetValue(node.ParentId, out var parentNode))
            {
                parentNode.Children ??= new List<TreeNode<T>>();
                parentNode.Children.Add(node);

                // 如果父节点之前被认为是根节点，则移除
                rootNodes.Remove(node);
            }
            else
            {
                // 如果找不到父节点，将其视为根节点
                rootNodes.Add(node);
            }
        }

        // 第三遍遍历：设置isLeaf属性
        foreach (var node in nodeDict.Values)
        {
            node.IsLeaf = node.Children == null || !node.Children.Any();
        }

        return rootNodes.ToList();
    }

    // 树转列表（深度优先遍历）
    public static List<T> TreeToList<T>(List<TreeNode<T>> tree) where T : ITreeDto
    {
        var list = new List<T>();

        foreach (var node in tree)
        {
            list.Add(node.Data);
            if (node.Children != null && node.Children.Any())
            {
                list.AddRange(TreeToList(node.Children));
            }
        }

        return list;
    }


    /// <summary>
    /// 递归遍历树形结构的每个节点（支持多个根节点）
    /// </summary>
    /// <typeparam name="T">节点类型</typeparam>
    /// <typeparam name="V">整个树形节点上下文</typeparam>
    /// <param name="nodes">根节点列表</param>
    /// <param name="getChildrenFunc">获取子节点集合的委托函数</param>
    /// <param name="getVFunc">构造树上下文类型</param>
    /// <param name="action">对每个节点执行的操作</param>
    /// <param name="maxDepth">最大递归深度，默认值为-1表示不限制</param>
    public static void TraverseTree<T, V>(
        IEnumerable<T> nodes,
        Func<T, IEnumerable<T>> getChildrenFunc,
        Func<V> getVFunc,
        Action<T, V, int> action,
        int maxDepth = -1)
    {
        if (nodes == null)
            return;

        foreach (var node in nodes)
        {
            var v = getVFunc();
            TraverseNode<T, V>(node, getChildrenFunc, action, maxDepth, 0, v);
        }
    }

    /// <summary>
    /// 递归遍历树形结构的每个节点
    /// </summary>
    /// <typeparam name="T">节点类型</typeparam>
    /// <param name="node">节点</param>
    /// <param name="getChildrenFunc">获取子节点集合的委托函数</param>
    /// <param name="action">对每个节点执行的操作</param>
    /// <param name="maxDepth">最大递归深度，默认值为-1表示不限制</param>
    private static void TraverseNode<T, V>(
        T node,
        Func<T, IEnumerable<T>> getChildrenFunc,
        Action<T, V, int> action,
        int maxDepth,
        int currentDepth,
        V treeContext)
    {
        // 检查是否超过最大深度限制
        if (maxDepth >= 0 && currentDepth > maxDepth)
            return;

        // 对当前节点执行操作
        action(node, treeContext, currentDepth);

        // 获取子节点集合并递归处理
        var children = getChildrenFunc(node);
        if (children != null)
        {
            foreach (var child in children)
            {
                TraverseNode(child, getChildrenFunc, action, maxDepth, currentDepth + 1, treeContext);
            }
        }
    }
}
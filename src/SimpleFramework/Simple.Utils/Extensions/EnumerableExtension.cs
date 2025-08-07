using System.Text;

namespace Simple.Utils.Extensions
{
    /// <summary>IEnumerable扩展方法</summary>
    public static class EnumerableExtension
    {
        // 判断IList<T> 是否都是空的
        public static bool IsAllNullOrEmpty<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0)
            {
                return true;
            }
            // 如果是string，空字符串也认为是空
            if (typeof(T) == typeof(string))
            {
                return list.All(item => string.IsNullOrEmpty(item as string));
            }
            return list.All(item => item == null || item.Equals(default(T)));
        }
    }
}
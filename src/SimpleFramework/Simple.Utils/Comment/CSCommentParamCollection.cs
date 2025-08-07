using System.Collections.Generic;

namespace Simple.Utils.Comment
{
    /// <summary> 用于描述注释中的 param 或者 typeparam 节点集合
    /// </summary>
    public sealed class CSCommentParamCollection : Dictionary<string, CSCommentParam>
    {
        /// <summary> 根据节点的name属性获取注释,如果没有返回 null
        /// </summary>
        /// <param name="pName"></param>
        public new CSCommentParam this[string pName]
        {
            get
            {
                var name = pName;
                if (name == null || name.Length == 0)
                {
                    return null;
                }
                else if (name[0] == '@')
                {
                    name = name.Remove(0, 1);
                }
                CSCommentParam value;
                if (TryGetValue(name, out value))
                {
                    return value;
                }
                return null;
            }
            set
            {
                var name = pName;
                if (name != null && name.Length > 0 && name[0] == '@')
                {
                    name = name.Remove(0, 1);
                }
                base[name] = value;
            }
        }

    }
}

using System.Xml;

namespace Simple.Utils.Comment
{
    /// <summary> 用于描述注释中的 exception 节点
    /// </summary>
    public sealed class CSCommentException
    {
        /// <summary> 初始化
        /// </summary>
        /// <param name="index"></param>
        /// <param name="node"></param>
        internal CSCommentException(int index, XmlNode node)
        {
            Index = index;
            var attr = node.Attributes["cref"];
            if (attr != null)
            {
                Cref = attr.InnerText;
                if (Cref != null)
                {
                    Cref = Cref.Remove(0, 2);
                }
            }
            Text = node.InnerText.Trim();
        }

        /// <summary> 节点在文档中的索引
        /// </summary>
        public int Index { get; private set; }
        /// <summary> 节点中的 cref 属性的值
        /// </summary>
        public string Cref { get; private set; }
        /// <summary> 节点中的内容
        /// </summary>
        public string Text { get; private set; }

        /// <summary> 返回Text属性或空字符串
        /// </summary>
        public override string ToString()
        {
            return Text ?? "";
        }
    }
}

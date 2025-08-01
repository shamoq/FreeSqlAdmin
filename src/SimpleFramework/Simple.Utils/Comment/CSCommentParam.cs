using System.Xml;

namespace Simple.Utils.Comment
{

    /// <summary> 用于描述注释中的 param 或者 typeparam 节点
    /// </summary>
    public sealed class CSCommentParam
    {

        /// <summary> 空对象
        /// </summary>
        public static readonly CSCommentParam Empty = new CSCommentParam();

        private CSCommentParam() { }

        /// <summary> 初始化 CSCommentParam
        /// </summary>
        /// <param name="node"></param>
        internal CSCommentParam(XmlNode node)
        {
            var attr = node.Attributes["name"];
            if (attr != null)
            {
                Name = attr.InnerText.Trim();
            }
            Text = node.InnerText;
        }
        /// <summary> 节点中的 name 属性的值
        /// </summary>
        public string Name { get; private set; }
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

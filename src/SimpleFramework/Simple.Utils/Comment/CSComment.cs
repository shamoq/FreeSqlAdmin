using System.Collections.Generic;
using System.Xml;

namespace Simple.Utils.Comment
{

    /// <summary> 用于描述一个成员的注释信息
    /// </summary>
    public sealed class CSComment
    {
        private CSComment()
        {
            Param = new CSCommentParamCollection();
            TypeParam = new CSCommentParamCollection();
            Exception = new CSCommentException[0];
        }

        internal void SetNodeAttr(XmlNode node)
        {
            if (node.Attributes != null && node.Attributes.Count > 0)
            {
                var dic = new Dictionary<string, string>();
                foreach (XmlAttribute n in node.Attributes)
                {
                    dic[n.Name] = n.Value;
                }
                Attributes[node.Name] = dic;
            }
        }

        /// <summary> 构造一个 CSComment 对象
        /// </summary>
        /// <param name="node">表示成员注释的节点</param>
        internal CSComment(XmlNode node)
        {
            var summary = node["summary"] ?? node["value"];
            if (summary != null)
            {
                Summary = summary.InnerText.Trim();
                SetNodeAttr(summary);
            }
            var remarks = node["remarks"];
            if (remarks != null)
            {
                Remarks = remarks.InnerText.Trim(); ;
                SetNodeAttr(remarks);
            }
            var returns = node["returns"];
            if (returns != null)
            {
                Returns = returns.InnerText.Trim();
                SetNodeAttr(returns);
            }
            var param = node.SelectNodes("param");
            Param = new CSCommentParamCollection();
            foreach (XmlNode item in param)
            {
                var p = new CSCommentParam(item);
                Param[p.Name] = p;
            }

            var typeparam = node.SelectNodes("typeparam");
            TypeParam = new CSCommentParamCollection();
            foreach (XmlNode item in typeparam)
            {
                var p = new CSCommentParam(item);
                TypeParam[p.Name] = p;
            }

            var exception = node.SelectNodes("exception");
            var index = 0;
            Exception = new CSCommentException[exception.Count];
            foreach (XmlNode ex in exception)
            {
                Exception[index] = new CSCommentException(index, ex);
                index++;
            }

        }

        /// <summary> 文档注释中的 summary 或 value 节点, 用于表示注释的主体说明
        /// </summary>
        public string Summary { get; private set; }

        /// <summary> 文档注释中的 remarks 节点, 用于表示备注信息
        /// </summary>
        public string Remarks { get; private set; }

        /// <summary> 文档注释中的 returns 节点, 用于表示返回值信息
        /// </summary>
        public string Returns { get; private set; }

        /// <summary> 文档注释中的 param 节点, 用于表示参数信息
        /// </summary>
        public CSCommentParamCollection Param { get; private set; }

        /// <summary> 文档注释中的 typeparam 节点, 用于表示泛型参数信息
        /// </summary>
        public CSCommentParamCollection TypeParam { get; private set; }

        /// <summary> 文档注释中的 exception 节点
        /// </summary>
        public CSCommentException[] Exception { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, Dictionary<string, string>> Attributes { get; private set; } =
            new Dictionary<string, Dictionary<string, string>>();


        /// <summary> 返回Summary属性或空字符串
        /// </summary>
        public override string ToString()
        {
            return Summary ?? "";
        }
    }





}

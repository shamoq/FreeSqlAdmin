using System.ComponentModel;
using Simple.Utils.Attributes;

namespace Simple.Interfaces.Enums
{
    [GenerateTypeScript]
    public static class TemplateTypeEnum
    {
        /// <summary>
        /// 线下文件模板
        /// </summary>
        [Description("word")]
        public static readonly string Word = "word";

        /// <summary>
        /// 富文本
        /// </summary>
        [Description("富文本")]
        public static readonly string Richtext = "richtext";

        /// <summary>
        /// Wps
        /// </summary>
        [Description("Wps")]
        public static readonly string Wps = "wps";
    }
}
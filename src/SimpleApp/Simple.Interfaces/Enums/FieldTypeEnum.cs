using System.ComponentModel;
using Simple.Utils.Attributes;

namespace Simple.AdminApplication.TemplateMng.Enums
{
    [GenerateTypeScript]
    public static class FieldTypeEnum
    {
        /// <summary>
        /// 文本
        /// </summary>
        [Description("文本")]
        public static readonly int Text = 1;

        /// <summary>
        /// 数字
        /// </summary>
        [Description("数字")]
        public static readonly int Number = 2;
        
        /// <summary>
        /// 日期
        /// </summary>
        [Description("日期")]
        public static readonly int Date = 3;
     
        /// <summary>
        /// 布尔
        /// </summary>
        [Description("布尔")]
        public static readonly int Bool = 4;
     
        /// <summary>
        /// 枚举
        /// </summary>
        [Description("枚举")]
        public static readonly int Enum = 5;
     
        /// <summary>
        /// 图片
        /// </summary>
        [Description("图片")]
        public static readonly int Image = 6;
    }
}
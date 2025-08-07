using System.ComponentModel;

namespace Simple.AdminApplication.SysMng.Enums
{
    /// <summary>
    /// 参数类型枚举
    /// </summary>
    [GenerateTypeScript]
    public static class ParamTypeEnum
    {
        /// <summary>
        /// 文本
        /// </summary>
        [Description("文本")]
        public static readonly int String = 1;

        /// <summary>
        /// 数值
        /// </summary>
        [Description("数值")]
        public static readonly int Number = 2;

        /// <summary>
        /// 日期
        /// </summary>
        [Description("日期")]
        public static readonly int Date = 3;

        /// <summary>
        /// 日期时间
        /// </summary>
        [Description("日期时间")]
        public static readonly int DateTime = 4;

        /// <summary>
        /// 时间
        /// </summary>
        [Description("时间")]
        public static readonly int Time = 5;

        /// <summary>
        /// 密码
        /// </summary>
        [Description("密码")]
        public static readonly int Password = 6;

        /// <summary>
        /// 选项
        /// </summary>
        [Description("选项")]
        public static readonly int Option = 7;

        /// <summary>
        /// 自定义
        /// </summary>
        [Description("自定义")]
        public static readonly int Customer = 8;
    }
}
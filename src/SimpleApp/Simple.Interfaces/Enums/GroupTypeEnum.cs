using System.ComponentModel;
using Simple.Utils.Attributes;

namespace Simple.Interfaces.Enums;

/// <summary>
/// 扩展字段类型
/// </summary>
[GenerateTypeScript]
public static class GroupTypeEnum
{
    /// <summary>
    /// 表单
    /// </summary>
    [Description("表单")]
    public static readonly string Form = "form";

    /// <summary>
    /// 表格
    /// </summary>
    [Description("表格")]
    public static readonly string Table = "table";

    /// <summary>
    /// 字段
    /// </summary>
    [Description("字段")]
    public static readonly string Field = "field";
}
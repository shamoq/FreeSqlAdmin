using System.ComponentModel;
using Simple.Utils.Attributes;

namespace Simple.Interfaces.Enums;

[GenerateTypeScript]
public static class ImageFieldTypeEnum
{
    /// <summary>
    /// 绝对路径
    /// </summary>
    [Description("绝对路径")]
    public static string AbsolutePath = "absolutePath";

    /// <summary>
    /// 绝对链接
    /// </summary>
    [Description("完整链接")]
    public static string AbsoluteHref = "absoluteHref";
    
    /// <summary>
    /// 条形码
    /// </summary>
    [Description("条形码")]
    public static string BarCode = "barCode";

    /// <summary>
    /// 二维码
    /// </summary>
    [Description("二维码")]
    public const string QrCode = "qrCode";
}
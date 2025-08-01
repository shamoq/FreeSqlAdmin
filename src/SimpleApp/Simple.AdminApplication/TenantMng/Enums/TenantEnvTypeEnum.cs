using System.ComponentModel;

namespace Simple.AdminApplication.TenantMng.Enums;

[GenerateTypeScript]
public static class TenantEnvTypeEnum
{
    /// <summary>
    /// 正式
    /// </summary>
    [Description("正式")]
    public static readonly string Prod = "Prod";

    /// <summary>
    /// 测试
    /// </summary>
    [Description("测试")]
    public static readonly string Test = "Test";
}
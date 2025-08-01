namespace Simple.Interfaces.Dtos.FieldProp;

/// <summary>
/// 枚举字段属性
/// </summary>
public class EnumProp : DataSetFieldProp
{
    public Dictionary<string, string> EnumValues { get; set; } = new Dictionary<string, string>();
}
namespace Simple.Interfaces.Dtos.FieldProp;

/// <summary>
/// 字符串字段属性
/// </summary>
public class StringProp : DataSetFieldProp
{
    public string TransformValue(object value)
    {
        return value?.ToString() ?? string.Empty;
    }
}
using Simple.Utils.Helper;

namespace Simple.Interfaces.Dtos.FieldProp;

/// <summary>
/// 布尔字段属性
/// </summary>
public class BooleanProp : DataSetFieldProp
{
    public string TrueText { get; set; } = "是";
    public string FalseText { get; set; } = "否";


    public string TransformValue(object value)
    {
        var boolValue = TypeConvertHelper.ConvertType<bool>(value);
        return boolValue ? TrueText : FalseText;
    }
}
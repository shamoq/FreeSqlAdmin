using Simple.Utils.Helper;

namespace Simple.Interfaces.Dtos.FieldProp;

/// <summary>
/// 日期字段属性
/// </summary>
public class DateProp : DataSetFieldProp
{
    /// <summary>
    /// 日期格式
    /// </summary>
    public string DateFormat { get; set; }

    public string TransformValue(object value)
    {
        var dateVal = TypeConvertHelper.ConvertType<DateTime?>(value);
        if(dateVal == null)
        {
            return string.Empty;
        }
            
        return dateVal.Value.ToString(DateFormat);
    }
}
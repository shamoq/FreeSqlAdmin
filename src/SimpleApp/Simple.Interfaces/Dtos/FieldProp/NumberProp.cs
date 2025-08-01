using Simple.Utils.Helper;

namespace Simple.Interfaces.Dtos.FieldProp;

/// <summary>
/// 数字字段属性
/// </summary>
public class NumberProp : DataSetFieldProp
{
    /// <summary>
    /// 是否启用千分位
    /// </summary>
    public bool IsThousands { get; set; }

    /// <summary>
    /// 小数位数
    /// </summary>
    public int? Digits { get; set; }

    /// <summary>
    /// 显示百分号
    /// </summary>
    public bool ShowPercent { get; set; }

    /// <summary>
    /// 单位
    /// </summary>
    public string Unit { get; set; }

    /// <summary>
    /// 转中文大写
    /// </summary>
    public bool Chinese { get; set; }
    
    /// <summary>
    /// 为null时显示的value
    /// </summary>
    public string NullValue { get; set; }

    /// <summary>
    /// 根据数字字段属性对输入值进行格式化
    /// </summary>
    /// <param name="value">需要格式化的输入值</param>
    /// <returns>格式化后的字符串</returns>
    public string TransformValue(object value)
    {
        if (value == null)
        {
            if (NullValue == "hide")
            {
                return string.Empty;
            }

            return "0";
        }

        var decimalValue = TypeConvertHelper.ConvertType<decimal>(value);
        if (Digits != null)
        {
            decimalValue = decimal.Round(decimalValue, Digits.Value);
        }

        var formatValue = string.Empty;

        if (Chinese)
        {
            formatValue = TypeConvertHelper.ToChinese(decimalValue, Digits ?? 2);
        }
        else if (IsThousands)
        {
            formatValue = decimalValue.ToString($"N{Digits ?? 2}");
        }
        else
        {
            formatValue = decimalValue.ToString();
        }

        // 后缀
        string suffix = ShowPercent ? "%" : Unit;

        // 格式化数字并添加单位
        return formatValue + suffix;
    }
}

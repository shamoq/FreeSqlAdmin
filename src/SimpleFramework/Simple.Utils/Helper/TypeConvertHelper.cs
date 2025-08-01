using Simple.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Simple.Utils.Helper
{
    public class TypeConvertHelper
    {
        /// <summary>
        /// 转换成GUID数组
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Guid[] AsGuidArray(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return Array.Empty<Guid>();
            }

            string[] parts = input.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<Guid> guids = new List<Guid>();

            foreach (string part in parts)
            {
                if (Guid.TryParse(part.Trim(), out Guid guid))
                {
                    guids.Add(guid);
                }
            }

            return guids.ToArray();
        }

        public static object ConvertType(object obj, Type targetType)
        {
            if (obj == null || obj == DBNull.Value)
            {
                return targetType.GetDefaultValue();
            }

            // 如果目标类型是字符串，直接调用 ToString()
            if (targetType == typeof(string))
            {
                if (obj is JArray || obj is JObject)
                {
                    return JsonConvert.SerializeObject(obj);
                }

                return obj.ToString();
            }


            // bool 类型
            if (targetType == typeof(bool))
            {
                if (obj is Boolean b)
                {
                    return b;
                }

                var s = obj.ToString();

                if (s == "1" || string.Compare(s, "true", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return true;
                }

                return false;
            }

            // 如果目标类型是枚举，尝试从字符串或数字转换
            if (targetType.IsEnum)
            {
                if (obj is string str)
                    return Enum.TryParse(targetType, str, out var result) ? result : targetType.GetDefaultValue();

                return Enum.ToObject(targetType, obj);
            }

            // 如果是时间类型
            if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
            {
                if (obj is long l || long.TryParse(obj.ToString(), out l))
                {
                    var time = DateTimeOffset.FromUnixTimeMilliseconds(l).LocalDateTime;
                    return time;
                }

                // 尝试使用 DateTime.TryParse 解析
                if (DateTime.TryParse(obj.ToString(), out var time2))
                {
                    return time2;
                }
            }

            // 如果源类型可以直接赋值给目标类型，直接返回
            if (targetType.IsAssignableFrom(obj.GetType()))
                return obj;

            // 逐个判断目标类型并调用对应的TryParse
            if (targetType == typeof(int) || targetType == typeof(int?))
            {
                return int.TryParse(obj.ToString(), out int result) ? result : null;
            }

            if (targetType == typeof(double) || targetType == typeof(double?))
            {
                return double.TryParse(obj.ToString(), out double result) ? result : null;
            }

            if (targetType == typeof(decimal) || targetType == typeof(decimal?))
            {
                return decimal.TryParse(obj.ToString(), out decimal result) ? result : null;
            }

            if (targetType == typeof(long) || targetType == typeof(long?))
            {
                return long.TryParse(obj.ToString(), out long result) ? result : null;
            }

            if (targetType == typeof(float) || targetType == typeof(float?))
            {
                return float.TryParse(obj.ToString(), out float result) ? result : null;
            }

            if (targetType == typeof(short) || targetType == typeof(short?))
            {
                return short.TryParse(obj.ToString(), out short result) ? result : null;
            }

            if (targetType == typeof(byte) || targetType == typeof(byte?))
            {
                return byte.TryParse(obj.ToString(), out byte result) ? result : null;
            }

            if (targetType == typeof(Guid) || targetType == typeof(Guid?))
            {
                return Guid.TryParse(obj.ToString(), out Guid result) ? result : null;
            }

            if (targetType == typeof(TimeSpan) || targetType == typeof(TimeSpan?))
            {
                return TimeSpan.TryParse(obj.ToString(), out TimeSpan result) ? result : null;
            }

            // 如果目标类型是可空类型，递归调用 ConvertType 处理基础类型
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var underlyingType = Nullable.GetUnderlyingType(targetType);
                return ConvertType(obj, underlyingType);
            }

            // 如果以上都不适用，尝试使用 Convert.ChangeType
            try
            {
                return System.Convert.ChangeType(obj, targetType);
            }
            catch
            {
                throw new InvalidOperationException($"Cannot convert {obj.GetType()} to {targetType}.");
            }
        }

        public static T ConvertType<T>(object obj)
        {
            var convertObj = ConvertType(obj, typeof(T));

            if (convertObj == null)
            {
                return default;
            }

            return (T)convertObj;
        }

        /// <summary>
        /// 将数字转换为中文大写
        /// </summary>
        /// <param name="number">要转换的数字</param>
        /// <param name="digits">要保留的小数位数</param>
        /// <returns>中文大写字符串</returns>
        public static string ToChinese(decimal number, int digits)
        {
            string[] numList = { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" };
            string[] unitList = { "", "拾", "佰", "仟", "万", "拾", "佰", "仟", "亿", "拾", "佰", "仟" };
            string[] decimalUnitList = { "角", "分", "厘", "毫" };

            string numberStr = number.ToString($"F{digits}");
            string[] parts = numberStr.Split('.');
            string integerPart = parts[0];
            string decimalPart = parts[1].TrimEnd('0');

            string integerResult = "";
            bool zeroFlag = false;
            for (int i = 0; i < integerPart.Length; i++)
            {
                int num = int.Parse(integerPart[i].ToString());
                int unitIndex = integerPart.Length - 1 - i;
                if (num == 0)
                {
                    zeroFlag = true;
                    if (unitIndex % 4 == 0) // 万、亿单位处理
                    {
                        integerResult += unitList[unitIndex];
                    }
                }
                else
                {
                    if (zeroFlag)
                    {
                        integerResult += "零";
                        zeroFlag = false;
                    }

                    integerResult += numList[num] + unitList[unitIndex];
                }
            }

            integerResult = integerResult.TrimEnd('零');
            if (integerResult == "")
            {
                integerResult = "零";
            }

            integerResult += "元";

            string decimalResult = "";
            // 限制小数部分处理的长度不超过 4 位
            int decimalLength = Math.Min(decimalPart.Length, decimalUnitList.Length);
            for (int i = 0; i < decimalLength; i++)
            {
                int num = int.Parse(decimalPart[i].ToString());
                decimalResult += numList[num] + decimalUnitList[i];
            }

            return integerResult + decimalResult;
        }
    }
}
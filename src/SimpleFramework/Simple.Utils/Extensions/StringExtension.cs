﻿using Simple.Utils.Extensions;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Simple.Utils.Extensions
{
    /// <summary>字符串扩展</summary>
    public static class StringExtension
    {
        public static object ParseTo(this string str, string type)
        {
            switch (type)
            {
                case "System.Boolean":
                    return str.ToBool();

                case "System.SByte":
                    return str.ToSByte();

                case "System.Byte":
                    return str.ToByte();

                case "System.UInt16":
                    return str.ToUInt16();

                case "System.Int16":
                    return str.ToInt16();

                case "System.uInt32":
                    return str.ToUInt32();

                case "System.Int32":
                    return str.ToInt32();

                case "System.UInt64":
                    return str.ToUInt64();

                case "System.Int64":
                    return str.ToInt64();

                case "System.Single":
                    return str.ToSingle();

                case "System.Double":
                    return str.ToDouble();

                case "System.Decimal":
                    return str.ToDecimal();

                case "System.DateTime":
                    return str.ToDateTime();

                case "System.Guid":
                    return str.ToGuid();
            }
            throw new NotSupportedException(string.Format("The string of \"{0}\" can not be parsed to {1}", str, type));
        }

        /// <summary>转为hex字符串</summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToHexString(this string value)
        {
            return string.Join("", value.Select(c => ((int)c).ToString("X2")));
        }

        public static sbyte? ToSByte(this string value)
        {
            sbyte value2;
            if (sbyte.TryParse(value, out value2))
            {
                return value2;
            }
            return null;
        }

        public static byte? ToByte(this string value)
        {
            byte value2;
            if (byte.TryParse(value, out value2))
            {
                return value2;
            }
            return null;
        }

        public static ushort? ToUInt16(this string value)
        {
            ushort value2;
            if (ushort.TryParse(value, out value2))
            {
                return value2;
            }
            return null;
        }

        public static short? ToInt16(this string value)
        {
            short value2;
            if (short.TryParse(value, out value2))
            {
                return value2;
            }
            return null;
        }

        public static uint? ToUInt32(this string value)
        {
            uint value2;
            if (uint.TryParse(value, out value2))
            {
                return value2;
            }
            return null;
        }

        public static ulong? ToUInt64(this string value)
        {
            ulong value2;
            if (ulong.TryParse(value, out value2))
            {
                return value2;
            }
            return null;
        }

        public static long? ToInt64(this string value)
        {
            long value2;
            if (long.TryParse(value, out value2))
            {
                return value2;
            }
            return null;
        }

        public static float? ToSingle(this string value)
        {
            float value2;
            if (float.TryParse(value, out value2))
            {
                return value2;
            }
            return null;
        }

        public static double? ToDouble(this string value)
        {
            double value2;
            if (double.TryParse(value, out value2))
            {
                return value2;
            }
            return null;
        }

        public static decimal? ToDecimal(this string value)
        {
            decimal value2;
            if (decimal.TryParse(value, out value2))
            {
                return value2;
            }
            return null;
        }

        public static bool ToBool(this string value)
        {
            if (string.Compare(value, "true", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            if (string.Compare(value, "false", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return false;
            }
            if (string.Compare(value, "1", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
            if (string.Compare(value, "0", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return false;
            }
            return value != null;
        }

        public static T? ToEnum<T>(this string str) where T : struct
        {
            T t;
            if (Enum.TryParse(str, true, out t) && Enum.IsDefined(typeof(T), t))
            {
                return t;
            }
            return null;
        }

        public static Guid ToGuid(this string str)
        {
            Guid value;
            if (Guid.TryParse(str, out value))
            {
                return value;
            }
            return Guid.Empty;
        }

        public static Guid? ToGuid2(this string str)
        {
            Guid value;
            if (Guid.TryParse(str, out value))
            {
                return value;
            }
            return null;
        }

        public static DateTime? ToDateTime(this string value)
        {
            DateTime value2;
            if (DateTime.TryParse(value, out value2))
            {
                return value2;
            }
            return null;
        }

        public static int? ToInt32(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }
            int value;
            if (int.TryParse(input, out value))
            {
                return value;
            }
            return null;
        }

        /// <summary>替换空格字符</summary>
        /// <param name="input"></param>
        /// <param name="replacement">替换为该字符</param>
        /// <returns>替换后的字符串</returns>
        public static string ReplaceWhitespace(this string input, string replacement = "")
        {
            return string.IsNullOrEmpty(input) ? null : Regex.Replace(input, "\\s", replacement, RegexOptions.Compiled);
        }

        /// <summary>返回一个值，该值指示指定的 String 对象是否出现在此字符串中。</summary>
        /// <param name="source"></param>
        /// <param name="value">要搜寻的字符串。</param>
        /// <param name="comparisonType">指定搜索规则的枚举值之一。</param>
        /// <returns>如果 value 参数出现在此字符串中则为 true；否则为 false。</returns>
        public static bool Contains(this string source, string value,
            StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            return source.IndexOf(value, comparisonType) >= 0;
        }

        /// <summary>清除 Html 代码，并返回指定长度的文本。(连续空行或空格会被替换为一个)</summary>
        /// <param name="text"></param>
        /// <param name="maxLength">返回的文本长度（为0返回所有文本）</param>
        /// <returns></returns>
        public static string StripHtml(this string text, int maxLength = 0)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            text = text.Trim();

            text = Regex.Replace(text, "[\\r\\n]{2,}", "<&rn>"); //替换回车和换行为<&rn>，防止下一行代码替换空格的时候被替换掉
            text = Regex.Replace(text, "[\\s]{2,}", " "); //替换 2 个以上的空格为 1 个
            text = Regex.Replace(text, "(<&rn>)+", "\n"); //还原 <&rn> 为 \n
            text = Regex.Replace(text, "(\\s*&[n|N][b|B][s|S][p|P];\\s*)+", " "); //&nbsp;

            text = Regex.Replace(text, "(<[b|B][r|R]/*>)+|(<[p|P](.|\\n)*?>)", "\n"); //<br>
            text = Regex.Replace(text, "<(.|\n)+?>", " ", RegexOptions.IgnoreCase); //any other tags

            if (maxLength > 0 && text.Length > maxLength)
                text = text.Substring(0, maxLength);

            return text;
        }

        /// <summary>是否为空</summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string text)
        {
            return string.IsNullOrEmpty(text);
        }

        /// <summary>是否是Base64字符串</summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool IsBase64(this string text)
        {
            var base64CodeArray = new char[]
            {
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
                '0', '1', '2', '3', '4',  '5', '6', '7', '8', '9', '+', '/', '='
            };

            if (string.IsNullOrEmpty(text))
                return false;
            else
            {
                if (text.Contains(","))
                    text = text.Split(',')[1];
                if (text.Length % 4 != 0)
                    return false;
                if (text.Any(c => !base64CodeArray.Contains(c)))
                    return false;
            }
            try
            {
                Convert.FromBase64String(text);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>将XML字符串转为实体对象</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static T XmlTo<T>(this string xml) where T : new()
        {
            var t = new T();
            if (xml.IsNullOrEmpty())
            {
                return t;
            }
            //将根替换
            xml = xml.Replace("<xml>", $"<{t.GetType().Name}>").Replace("</xml>", $"</{t.GetType().Name}>");
            xml = xml.Replace("<root>", $"<{t.GetType().Name}>").Replace("</root>", $"</{t.GetType().Name}>");
            using (var sr = new StringReader(xml))
            {
                XmlSerializer xmldes = new XmlSerializer(typeof(T));
                t = (T)xmldes.Deserialize(sr);
            }
            return t;
        }

        /// <summary>字符串分割后转int数组</summary>
        /// <param name="temp"></param>
        /// <param name="split"></param>
        /// <returns></returns>
        public static int[] SplitToInt(this string temp, string split = ",")
        {
            var strArray = temp.Split(',');
            if (strArray.Length == 0) return new int[] { 0 };

            return Array.ConvertAll(strArray, s => int.TryParse(s, out int i) ? i : 0);
        }

        /// <summary>
        /// 驼峰命名和蛇形命名转换成帕斯卡命名
        /// helloWorld → HelloWorld
        /// hello_world → HelloWorld
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToPascalCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            StringBuilder result = new StringBuilder();
            bool shouldCapitalize = true;

            foreach (char c in input)
            {
                if (c == '_')
                {
                    shouldCapitalize = true;
                    continue;
                }

                if (shouldCapitalize)
                {
                    result.Append(char.ToUpper(c));
                    shouldCapitalize = false;
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 将蛇形命名和帕斯卡命名转换为驼峰命名
        /// hello_world → helloWorld
        /// HelloWorld → helloWorld
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToCamelCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            StringBuilder result = new StringBuilder();
            bool shouldCapitalize = true;

            foreach (char c in input)
            {
                if (c == '_')
                {
                    shouldCapitalize = true;
                    continue;
                }

                if (shouldCapitalize)
                {
                    result.Append(char.ToLower(c));
                    shouldCapitalize = false;
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }
    }
}
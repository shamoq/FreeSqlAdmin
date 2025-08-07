﻿using Simple.Utils.Exceptions;
using Simple.Utils.Extensions;
using System.Collections;
using System.Data;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace Simple.Utils.Helper
{
    /// <summary>通用扩展方法</summary>
    public class ObjectHelper
    {
        /// <summary>将集合转换为数据集。</summary>
        /// <param name="list">集合。</param>
        /// <param name="t">转换的元素类型。</param>
        /// <param name="generic">是否生成泛型数据集。</param>
        /// <returns>转换后的数据集。</returns>
        private static DataSet ListToDataSet(IEnumerable list, Type t, bool generic)
        {
            DataSet ds = new DataSet("Data");
            if (t == null)
            {
                if (list != null)
                {
                    foreach (var i in list)
                    {
                        if (i == null)
                        {
                            continue;
                        }

                        t = i.GetType();
                        break;
                    }
                }

                if (t == null)
                {
                    return ds;
                }
            }

            ds.Tables.Add(t.Name);
            //如果集合中元素为DataSet扩展涉及到的基本类型时，进行特殊转换。
            if (t.IsValueType || t == typeof(string))
            {
                ds.Tables[0].TableName = "Info";
                ds.Tables[0].Columns.Add(t.Name);
                if (list != null)
                {
                    foreach (var i in list)
                    {
                        DataRow addRow = ds.Tables[0].NewRow();
                        addRow[t.Name] = i;
                        ds.Tables[0].Rows.Add(addRow);
                    }
                }

                return ds;
            }

            //处理模型的字段和属性。
            var fields = t.GetFields();
            var properties = t.GetProperties();
            foreach (var j in fields)
            {
                if (!ds.Tables[0].Columns.Contains(j.Name))
                {
                    if (generic)
                    {
                        ds.Tables[0].Columns.Add(j.Name, j.FieldType);
                    }
                    else
                    {
                        ds.Tables[0].Columns.Add(j.Name);
                    }
                }
            }

            foreach (var j in properties)
            {
                if (!ds.Tables[0].Columns.Contains(j.Name))
                {
                    if (generic)
                    {
                        ds.Tables[0].Columns.Add(j.Name, j.PropertyType);
                    }
                    else
                    {
                        ds.Tables[0].Columns.Add(j.Name);
                    }
                }
            }

            if (list == null)
            {
                return ds;
            }

            //读取list中元素的值。
            foreach (var i in list)
            {
                if (i == null)
                {
                    continue;
                }

                DataRow addRow = ds.Tables[0].NewRow();
                foreach (var j in fields)
                {
                    MemberExpression field = Expression.Field(Expression.Constant(i), j.Name);
                    LambdaExpression lambda = Expression.Lambda(field, new ParameterExpression[] { });
                    Delegate func = lambda.Compile();
                    object value = func.DynamicInvoke();
                    addRow[j.Name] = value;
                }

                foreach (var j in properties)
                {
                    MemberExpression property = Expression.Property(Expression.Constant(i), j);
                    LambdaExpression lambda = Expression.Lambda(property, new ParameterExpression[] { });
                    Delegate func = lambda.Compile();
                    object value = func.DynamicInvoke();
                    addRow[j.Name] = value;
                }

                ds.Tables[0].Rows.Add(addRow);
            }

            return ds;
        }

        /// <summary>将集合转换为数据集。</summary>
        /// <typeparam name="T">转换的元素类型。</typeparam>
        /// <param name="list">集合。</param>
        /// <param name="generic">是否生成泛型数据集。</param>
        /// <returns>数据集。</returns>
        private static DataSet ListToDataSet<T>(IEnumerable<T> list, bool generic)
        {
            return ListToDataSet(list, typeof(T), generic);
        }

        /// <summary>将集合转换为数据集。</summary>
        /// <param name="list">集合。</param>
        /// <param name="generic">是否转换为字符串形式。</param>
        /// <returns>转换后的数据集。</returns>
        private static DataSet ListToDataSet(IEnumerable list, bool generic)
        {
            return ListToDataSet(list, null, generic);
        }

        /// <summary>将集合转换为数据集。</summary>
        /// <typeparam name="T">转换的元素类型。</typeparam>
        /// <param name="list">集合。</param>
        /// <param name="generic">是否生成泛型数据集。</param>
        /// <returns>数据集。</returns>
        public static DataSet ToDataSet<T>(IEnumerable<T> list, bool generic = true)
        {
            return ListToDataSet(list, generic);
        }

        /// <summary>将集合转换为数据集。</summary>
        /// <param name="list">集合。</param>
        /// <param name="generic">是否生成泛型数据集。</param>
        /// <returns>数据集。</returns>
        public static DataSet ToDataSet(IEnumerable list, bool generic = true)
        {
            return ListToDataSet(list, generic);
        }

        /// <summary>将集合转换为数据集。</summary>
        /// <typeparam name="T">转换的元素类型。</typeparam>
        /// <param name="list">集合。</param>
        /// <param name="generic">是否生成泛型数据集。</param>
        /// <returns>数据集。</returns>
        public static DataSet ToDataSet<T>(IEnumerable list, bool generic = true)
        {
            return ListToDataSet(list, typeof(T), generic);
        }

        /// <summary>将实例转换为集合数据集。</summary>
        /// <typeparam name="T">实例类型。</typeparam>
        /// <param name="o">实例。</param>
        /// <param name="generic">是否生成泛型数据集。</param>
        /// <returns>数据集。</returns>
        public static DataSet ToListSet<T>(T o, bool generic = true)
        {
            if (o is IEnumerable)
            {
                return ListToDataSet(o as IEnumerable, generic);
            }
            else
            {
                return ListToDataSet(new T[] { o }, generic);
            }
        }

        /// <summary>将可序列化实例转换为XmlDocument。</summary>
        /// <typeparam name="T">实例类型。</typeparam>
        /// <param name="o">实例。</param>
        /// <returns>XmlDocument。</returns>
        public static XmlDocument ToXmlDocument<T>(T o)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.InnerXml = ToListSet(o).GetXml();
            return xmlDocument;
        }

        /// <summary>获取DataSet第一表，第一行，第一列的值。</summary>
        /// <param name="ds">DataSet数据集。</param>
        /// <returns>值。</returns>
        public static object GetData(DataSet ds)
        {
            if (
                ds == null
                || ds.Tables.Count == 0
            )
            {
                return string.Empty;
            }
            else
            {
                return GetData(ds.Tables[0]);
            }
        }

        /// <summary>获取DataTable第一行，第一列的值。</summary>
        /// <param name="dt">DataTable数据集表。</param>
        /// <returns>值。</returns>
        public static object GetData(DataTable dt)
        {
            if (
                dt.Columns.Count == 0
                || dt.Rows.Count == 0
            )
            {
                return string.Empty;
            }
            else
            {
                return dt.Rows[0][0];
            }
        }

        /// <summary>获取DataSet第一个匹配columnName的值。</summary>
        /// <param name="ds">数据集。</param>
        /// <param name="columnName">列名。</param>
        /// <returns>值。</returns>
        public static object GetData(DataSet ds, string columnName)
        {
            if (
                ds == null
                || ds.Tables.Count == 0
            )
            {
                return string.Empty;
            }

            foreach (DataTable dt in ds.Tables)
            {
                object o = GetData(dt, columnName);
                if (!string.IsNullOrEmpty(o.ToString()))
                {
                    return o;
                }
            }

            return string.Empty;
        }

        /// <summary>获取DataTable第一个匹配columnName的值。</summary>
        /// <param name="dt">数据表。</param>
        /// <param name="columnName">列名。</param>
        /// <returns>值。</returns>
        public static object GetData(DataTable dt, string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                return GetData(dt);
            }

            if (
                dt.Columns.Count == 0
                || dt.Columns.IndexOf(columnName) == -1
                || dt.Rows.Count == 0
            )
            {
                return string.Empty;
            }

            return dt.Rows[0][columnName];
        }

        /// <summary>将object转换为string类型信息。</summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>string。</returns>
        public static string ToString(object o, string t)
        {
            string info = string.Empty;
            if (o == null)
            {
                info = t;
            }
            else
            {
                info = o.ToString();
            }

            return info;
        }

        /// <summary>将DateTime?转换为string类型信息。</summary>
        /// <param name="o">DateTime?。</param>
        /// <param name="format">标准或自定义日期和时间格式的字符串。</param>
        /// <param name="t">默认值。</param>
        /// <returns>string。</returns>
        public static string ToString(DateTime? o, string format, string t)
        {
            string info = string.Empty;
            if (o == null)
            {
                info = t;
            }
            else
            {
                info = o.Value.ToString(format);
            }

            return info;
        }

        /// <summary>将TimeSpan?转换为string类型信息。</summary>
        /// <param name="o">TimeSpan?。</param>
        /// <param name="format">标准或自定义时间格式的字符串。</param>
        /// <param name="t">默认值。</param>
        /// <returns>string。</returns>
        public static string ToString(TimeSpan? o, string format, string t)
        {
            string info = string.Empty;
            if (o == null)
            {
                info = t;
            }
            else
            {
                info = o.Value.ToString(format);
            }

            return info;
        }

        /// <summary>将object转换为截取后的string类型信息。</summary>
        /// <param name="o">object。</param>
        /// <param name="startIndex">此实例中子字符串的起始字符位置（从零开始）。</param>
        /// <param name="length">子字符串中的字符数。</param>
        /// <param name="suffix">后缀。如果没有截取则不添加。</param>
        /// <returns>截取后的string类型信息。</returns>
        public static string ToSubString(object o, int startIndex, int length, string suffix = null)
        {
            string inputString = ToSafeString(o);
            startIndex = Math.Max(startIndex, 0);
            startIndex = Math.Min(startIndex, inputString.Length - 1);
            length = Math.Max(length, 1);
            if (startIndex + length > inputString.Length)
            {
                length = inputString.Length - startIndex;
            }

            if (inputString.Length == startIndex + length)
            {
                return inputString;
            }
            else
            {
                return inputString.Substring(startIndex, length) + suffix;
            }
        }

        /// <summary>将object转换为byte类型信息。</summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>byte。</returns>
        public static byte ToByte(object o, byte t = default)
        {
            byte info;
            if (!byte.TryParse(ToSafeString(o), out info))
            {
                info = t;
            }

            return info;
        }

        /// <summary>将object转换为char类型信息。</summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>char。</returns>
        public static char ToChar(object o, char t = default)
        {
            char info;
            if (!char.TryParse(ToSafeString(o), out info))
            {
                info = t;
            }

            return info;
        }

        /// <summary>将object转换为int类型信息。</summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>int。</returns>
        public static int ToInt(object o, int t = default)
        {
            int info;
            if (!int.TryParse(ToSafeString(o), out info))
            {
                info = t;
            }

            return info;
        }

        /// <summary>将object转换为double类型信息。</summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>double。</returns>
        public static double ToDouble(object o, double t = default)
        {
            double info;
            if (!double.TryParse(ToSafeString(o), out info))
            {
                info = t;
            }

            return info;
        }

        /// <summary>将object转换为decimal类型信息。</summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>decimal。</returns>
        public static decimal ToDecimal(object o, decimal t = default)
        {
            decimal info;
            if (!decimal.TryParse(ToSafeString(o), out info))
            {
                info = t;
            }

            return info;
        }

        /// <summary>将object转换为float类型信息。</summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>float。</returns>
        public static float ToFloat(object o, float t = default)
        {
            float info;
            if (!float.TryParse(ToSafeString(o), out info))
            {
                info = t;
            }

            return info;
        }

        /// <summary>将object转换为long类型信息。</summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>long。</returns>
        public static long ToLong(object o, long t = default)
        {
            long info;
            if (!long.TryParse(ToSafeString(o), out info))
            {
                info = t;
            }

            return info;
        }

        /// <summary>将object转换为bool类型信息。</summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>bool。</returns>
        public static bool ToBool(object o, bool t = default)
        {
            bool info;
            if (!bool.TryParse(ToSafeString(o), out info))
            {
                info = t;
            }

            return info;
        }

        /// <summary>将object转换为sbyte类型信息。</summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>sbyte。</returns>
        public static sbyte ToSbyte(object o, sbyte t = default)
        {
            sbyte info;
            if (!sbyte.TryParse(ToSafeString(o), out info))
            {
                info = t;
            }

            return info;
        }

        /// <summary>将object转换为short类型信息。</summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>short。</returns>
        public static short ToShort(object o, short t = default)
        {
            short info;
            if (!short.TryParse(ToSafeString(o), out info))
            {
                info = t;
            }

            return info;
        }

        /// <summary>将object转换为ushort类型信息。</summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>ushort。</returns>
        public static ushort ToUShort(object o, ushort t = default)
        {
            ushort info;
            if (!ushort.TryParse(ToSafeString(o), out info))
            {
                info = t;
            }

            return info;
        }

        /// <summary>将object转换为ulong类型信息。</summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>ulong。</returns>
        public static ulong ToULong(object o, ulong t = default)
        {
            ulong info;
            if (!ulong.TryParse(ToSafeString(o), out info))
            {
                info = t;
            }

            return info;
        }

        /// <summary>将object转换为Enum[T]类型信息。</summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>Enum[T]。</returns>
        public static T ToEnum<T>(object o, T t = default)
            where T : struct
        {
            T info;
            if (!Enum.TryParse(ToSafeString(o), out info))
            {
                info = t;
            }

            return info;
        }

        /// <summary>将object转换为DateTime类型信息。</summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>DateTime。</returns>
        public static DateTime ToDateTime(object o, DateTime t = default)
        {
            if (t == default)
            {
                t = new DateTime(1753, 1, 1);
            }

            DateTime info;
            if (!DateTime.TryParse(ToSafeString(o), out info))
            {
                info = t;
            }

            return info;
        }

        /// <summary>将object转换为TimeSpan类型信息。</summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>TimeSpan。</returns>
        public static TimeSpan ToTimeSpan(object o, TimeSpan t = default)
        {
            if (t == default)
            {
                t = new TimeSpan(0, 0, 0);
            }

            TimeSpan info;
            if (!TimeSpan.TryParse(ToSafeString(o), out info))
            {
                info = t;
            }

            return info;
        }

        /// <summary>将object转换为Guid类型信息。</summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>Guid。</returns>
        public static Guid ToGuid(object o, Guid t = default)
        {
            Guid info;
            if (!Guid.TryParse(ToSafeString(o), out info))
            {
                info = t;
            }

            return info;
        }

        /// <summary>从object中获取bool类型信息。</summary>
        /// <param name="o">object。</param>
        /// <returns>bool。</returns>
        public static bool? GetBool(object o)
        {
            Regex BoolRegex = new Regex("(?<info>(true|false))", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            bool info;
            if (!bool.TryParse(BoolRegex.Match(ToSafeString(o)).Groups["info"].Value, out info))
            {
                return null;
            }

            return info;
        }

        /// <summary>从object中获取int类型信息。</summary>
        /// <param name="o">object。</param>
        /// <returns>int。</returns>
        public static int? GetInt(object o)
        {
            Regex IntRegex = new Regex("(?<info>-?\\d+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            int info;
            if (!int.TryParse(IntRegex.Match(ToSafeString(o)).Groups["info"].Value, out info))
            {
                return null;
            }

            return info;
        }

        /// <summary>从object中获取decimal类型信息。</summary>
        /// <param name="o">object。</param>
        /// <returns>decimal。</returns>
        public static decimal? GetDecimal(object o)
        {
            Regex DecimalRegex =
             new Regex("(?<info>-?\\d+(\\.\\d+)?)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            decimal info;
            if (!decimal.TryParse(DecimalRegex.Match(ToSafeString(o)).Groups["info"].Value, out info))
            {
                return null;
            }

            return info;
        }

        /// <summary>从object中获取double类型信息。</summary>
        /// <param name="o">object。</param>
        /// <returns>double。</returns>
        public static double? GetDouble(object o)
        {
            Regex DecimalRegex =
            new Regex("(?<info>-?\\d+(\\.\\d+)?)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            double info;
            if (!double.TryParse(DecimalRegex.Match(ToSafeString(o)).Groups["info"].Value, out info))
            {
                return null;
            }

            return info;
        }

        /// <summary>从object中获取正数信息。</summary>
        /// <param name="o">object。</param>
        /// <returns>decimal。</returns>
        public static decimal? GetPositiveNumber(object o)
        {
            Regex DecimalRegex =
            new Regex("(?<info>-?\\d+(\\.\\d+)?)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            decimal info;
            if (!decimal.TryParse(DecimalRegex.Match(ToSafeString(o)).Groups["info"].Value, out info))
            {
                return null;
            }

            return Math.Abs(info);
        }

        /// <summary>从object中获取DateTime?类型信息。</summary>
        /// <param name="o">object。</param>
        /// <returns>DateTime?。</returns>
        public static DateTime? GetDateTime(object o)
        {
            Regex DateTimeRegex = new Regex(

             "(?<info>(((\\d+)[/年-](0?[13578]|1[02])[/月-](3[01]|[12]\\d|0?\\d)[日]?)|((\\d+)[/年-](0?[469]|11)[/月-](30|[12]\\d|0?\\d)[日]?)|((\\d+)[/年-]0?2[/月-](2[0-8]|1\\d|0?\\d)[日]?))(\\s((2[0-3]|[0-1]\\d)):[0-5]\\d:[0-5]\\d)?)",
             RegexOptions.IgnoreCase | RegexOptions.Singleline);

            DateTime info;
            if (!DateTime.TryParse(

                    DateTimeRegex.Match(ToSafeString(o)).Groups["info"].Value.Replace("年", "-").Replace("月", "-")

                        .Replace("/", "-").Replace("日", ""), out info))

            {
                return null;
            }

            return info;
        }

        /// <summary>从object中获取TimeSpan?类型信息。</summary>
        /// <param name="o">object。</param>
        /// <returns>TimeSpan?。</returns>
        public static TimeSpan? GetTimeSpan(object o)
        {
            Regex TimeSpanRegex =
             new Regex(
                 "(?<info>-?(\\d+\\.(([0-1]\\d)|(2[0-3])):[0-5]\\d:[0-5]\\d)|((([0-1]\\d)|(2[0-3])):[0-5]\\d:[0-5]\\d)|(\\d+))",
                 RegexOptions.IgnoreCase | RegexOptions.Singleline);

            TimeSpan info;
            if (!TimeSpan.TryParse(TimeSpanRegex.Match(ToSafeString(o)).Groups["info"].Value, out info))
            {
                return null;
            }

            return info;
        }

        /// <summary>从object中获取Guid?类型信息。</summary>
        /// <param name="o">object。</param>
        /// <returns>Guid?。</returns>
        public static Guid? GetGuid(object o)
        {
            Regex GuidRegex =

            new Regex(
                "(?<info>\\{{0,1}([0-9a-fA-F]){8}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){4}-([0-9a-fA-F]){12}\\}{0,1})",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            Guid info;
            if (!Guid.TryParse(GuidRegex.Match(ToSafeString(o)).Groups["info"].Value, out info))

            {
                return null;
            }

            return info;
        }

        /// <summary>将object转换为SqlServer中的DateTime?类型信息。</summary>
        /// <param name="o">object。</param>
        /// <param name="t">默认值。</param>
        /// <returns>DateTime?。</returns>
        public static DateTime? GetSqlDateTime(object o, DateTime t = default)
        {
            DateTime info;
            if (!DateTime.TryParse(ToSafeString(o), out info))
            {
                info = t;
            }

            if (info < new DateTime(1753, 1, 1) || info > new DateTime(9999, 12, 31))
            {
                return null;
            }

            return info;
        }

        /// <summary>读取XElement节点的文本内容。</summary>
        /// <param name="xElement">XElement节点。</param>
        /// <param name="t">默认值。</param>
        /// <returns>文本内容。</returns>
        public static string Value(XElement xElement, string t = default)
        {
            if (xElement == null)
            {
                return t;
            }
            else
            {
                return xElement.Value;
            }
        }

        /// <summary>获取与指定键相关的值。</summary>
        /// <typeparam name="TKey">键类型。</typeparam>
        /// <typeparam name="TValue">值类型。</typeparam>
        /// <param name="dictionary">表示键/值对象的泛型集合。</param>
        /// <param name="key">键。</param>
        /// <param name="t">默认值。</param>
        /// <returns>值。</returns>
        public static TValue GetValue<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key, TValue t = default)
        {
            TValue value = default;
            if (dictionary == null || key == null)
            {
                return t;
            }

            if (!dictionary.TryGetValue(key, out value))
            {
                value = t;
            }

            return value;
        }

        /// <summary>获取与指定键相关或者第一个的值。</summary>
        /// <typeparam name="TKey">键类型。</typeparam>
        /// <typeparam name="TValue">值类型。</typeparam>
        /// <param name="dictionary">表示键/值对象的泛型集合。</param>
        /// <param name="key">键。</param>
        /// <param name="t">默认值。</param>
        /// <returns>值。</returns>
        public static TValue GetFirstOrDefaultValue<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key,
            TValue t = default)
        {
            TValue value = default;
            if (dictionary == null || key == null)
            {
                return t;
            }

            if (!dictionary.TryGetValue(key, out value))
            {
                if (dictionary.Count() == 0)
                {
                    value = t;
                }
                else
                {
                    value = dictionary.FirstOrDefault().Value;
                }
            }

            return value;
        }

        /// <summary>获取具有指定 System.Xml.Linq.XName 的第一个（按文档顺序）子元素。</summary>
        /// <param name="xContainer">XContainer。</param>
        /// <param name="xName">要匹配的 System.Xml.Linq.XName。</param>
        /// <param name="t">是否返回同名默认值。</param>
        /// <returns>与指定 System.Xml.Linq.XName 匹配的 System.Xml.Linq.XElement，或者为 null。</returns>
        public static XElement Element(XContainer xContainer, XName xName, bool t)
        {
            XElement info;
            if (xContainer == null)
            {
                info = null;
            }
            else
            {
                info = xContainer.Element(xName);
            }

            if (t && info == null)
            {
                info = new XElement(xName);
            }

            return info;
        }

        /// <summary>按文档顺序返回此元素或文档的子元素集合。</summary>
        /// <param name="xContainer">XContainer。</param>
        /// <param name="t">是否返回非空默认值。</param>
        /// <returns>System.Xml.Linq.XElement 的按文档顺序包含此System.Xml.Linq.XContainer 的子元素，或者非空默认值。</returns>
        public static IEnumerable<XElement> Elements(XContainer xContainer, bool t)
        {
            IEnumerable<XElement> info;
            if (xContainer == null)
            {
                info = null;
            }
            else
            {
                info = xContainer.Elements();
            }

            if (t && info == null)
            {
                info = new List<XElement>();
            }

            return info;
        }

        /// <summary>按文档顺序返回此元素或文档的经过筛选的子元素集合。集合中只包括具有匹配 System.Xml.Linq.XName 的元素。</summary>
        /// <param name="xContainer">XContainer。</param>
        /// <param name="xName">要匹配的 System.Xml.Linq.XName。</param>
        /// <param name="t">是否返回非空默认值。</param>
        /// <returns>
        /// System.Xml.Linq.XElement 的按文档顺序包含具有匹配System.Xml.Linq.XName 的 System.Xml.Linq.XContainer 的子级，或者非空默认值。
        /// </returns>
        public static IEnumerable<XElement> Elements(XContainer xContainer, XName xName, bool t)
        {
            IEnumerable<XElement> info;
            if (xContainer == null)
            {
                info = null;
            }
            else
            {
                info = xContainer.Elements(xName);
            }

            if (t && info == null)
            {
                info = new List<XElement>();
            }

            return info;
        }

        /// <summary>删除html标签。</summary>
        /// <param name="html">输入的字符串。</param>
        /// <returns>没有html标签的字符串。</returns>
        public static string RemoveHTMLTags(string html)
        {
            return Regex
                .Replace(
                    Regex.Replace(
                        Regex.Replace(
                            (html ?? string.Empty).Replace("&nbsp;", " ").Replace("\r\n", " ").Replace("\n", " ")
                            .Replace("\r", " ").Replace("\t", " "), "<\\/?[^>]+>", "\r\n"), "(\r\n)+", "\r\n"),
                    "(\\s)+", " ").Trim();
        }

        /// <summary>字符串转换为文件名。</summary>
        /// <param name="s">字符串。</param>
        /// <returns>文件名。</returns>
        public static string ToFileName(string s)
        {
            return Regex.Replace(ToSafeString(s), @"[\\/:*?<>|]", "_").Replace("\t", " ").Replace("\r\n", " ")
                .Replace("\"", " ");
        }

        /// <summary>获取星期一的日期。</summary>
        /// <param name="dateTime">日期。</param>
        /// <returns>星期一的日期。</returns>
        public static DateTime? GetMonday(DateTime dateTime)
        {
            return dateTime.AddDays(-1 * (int)dateTime.AddDays(-1).DayOfWeek).ToString("yyyy-MM-dd").ToDateTime();
        }

        /// <summary>获取默认非空字符串。</summary>
        /// <param name="s">首选默认非空字符串。</param>
        /// <param name="args">依次非空字符串可选项。</param>
        /// <returns>默认非空字符串。若无可选项则返回string.Empty。</returns>
        public static string DefaultStringIfEmpty(string s, params string[] args)
        {
            if (string.IsNullOrEmpty(s))
            {
                foreach (string i in args)
                {
                    if (!string.IsNullOrEmpty(i) && !string.IsNullOrEmpty(i.Trim()))
                    {
                        return i;
                    }
                }
            }

            return s ?? string.Empty;
        }

        /// <summary>对 URL 字符串进行编码。</summary>
        /// <param name="s">要编码的文本。</param>
        /// <param name="regex">匹配要编码的文本。</param>
        /// <param name="encoding">指定编码方案的 System.Text.Encoding 对象。</param>
        /// <returns>一个已编码的字符串。</returns>
        public static string ToUrlEncodeString(string s, Regex regex = default, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            if (regex == null)
            {
                return HttpUtility.UrlEncode(s, encoding);
            }

            List<string> l = new List<string>();
            foreach (char i in s)
            {
                string t = i.ToString();
                l.Add(regex.IsMatch(t) ? HttpUtility.UrlEncode(t, encoding) : t);
            }

            return string.Join(string.Empty, l);
        }

        /// <summary>对 URL 字符串进行编码。</summary>
        /// <param name="s">要编码的文本。</param>
        /// <param name="regex">匹配要编码的文本。</param>
        /// <param name="encoding">指定编码方案的 System.Text.Encoding 对象。</param>
        /// <returns>一个已编码的字符串。</returns>
        public static string ToUrlEncodeString(string s, string regex, Encoding encoding = null)
        {
            return ToUrlEncodeString(s, new Regex(regex), encoding);
        }

        /// <summary>将日期转换为UNIX时间戳字符串</summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string ToUnixTimeStamp(DateTime date)
        {
            DateTime startTime = TimeZoneInfo.ConvertTimeToUtc(new DateTime(1970, 1, 1));
            string timeStamp = date.Subtract(startTime).Ticks.ToString();
            return timeStamp.Substring(0, timeStamp.Length - 7);
        }

        /// <summary>判断当前字符串是否是移动电话号码</summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public static bool IsMobile(string mobile)
        {
            Regex MobileRegex = new Regex("^1[3|4|5|7|8][0-9]\\d{4,8}$");

            return MobileRegex.IsMatch(mobile);
        }

        /// <summary>判断当前字符串是否为邮箱</summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool IsEmail(string email)
        {
            Regex EmailRegex =
           new Regex("^([a-zA-Z0-9_-])+@([a-zA-Z0-9_-])+((\\.[a-zA-Z0-9_-]{2,3}){1,2})$");

            return EmailRegex.IsMatch(email);
        }

        /// <summary>尝试释放对象占用的资源</summary>
        /// <param name="obj">要释放的对象</param>
        public static void TryDispose(IDisposable obj)
        {
            if (obj != null)
            {
                try
                {
                    obj.Dispose();
                }
                catch
                {
                }
            }
        }

        /// <summary> 从字典 获取模型值 </summary>
        /// <param name="obj"></param>
        /// <param name="dic"></param>
        public static void TryFromDict(object obj, Dictionary<string, object> dic)
        {
            try
            {
                var props = obj.GetType().GetProperties();
                //根据Key值设定 Columns
                foreach (KeyValuePair<string, object> item in dic)
                {
                    var prop = props.FirstOrDefault(x => x.Name.Equals(item.Key, StringComparison.OrdinalIgnoreCase));
                    if (prop != null && item.Value != null)
                    {
                        var value = item.Value;
                        //Nullable 获取Model类字段的真实类型
                        Type itemType = Nullable.GetUnderlyingType(prop.PropertyType) == null
                            ? prop.PropertyType
                            : Nullable.GetUnderlyingType(prop.PropertyType);

                        if (itemType.IsGenericType
                            || itemType.IsArray
                            || itemType.IsNotPublic) continue;

                        if (itemType.IsEnum)
                        {
                            var changeValue = Enum.Parse(itemType, value.ToString(), true);
                            prop.SetValue(obj, changeValue, null);
                            continue;
                        }

                        //根据Model类字段的真实类型进行转换
                        var typeValue = TypeConvertHelper.ConvertType(value, itemType);
                        // var currentValue = prop.GetValue(obj);
                        // if (!Equals(currentValue, typeValue))
                        // {
                        prop.SetValue(obj, typeValue, null);
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException("模型赋值异常", "模型赋值异常", ex);
            }
        }

        /// <summary>对象的无限链式调用</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <param name="fun"></param>
        /// <returns></returns>
        public static T Next<T>(T s, Action<T> fun = null)
        {
            fun?.Invoke(s);
            return s;
        }

        /// <summary>
        /// 转换成安全字符
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private static string ToSafeString(object o)
        {
            return o != null ? o.ToString() : string.Empty;
        }
    }
}
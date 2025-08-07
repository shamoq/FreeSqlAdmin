using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;

namespace Simple.Utils.Extensions
{
    /// <summary>
    /// 类型转换封装
    /// </summary>
    public static class TypeConvertExtension
    {
        /// <summary>
        /// 字符串转decimal
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="defaultVal">默认值</param>
        /// <returns></returns>
        public static decimal AsDecimal(this string str, decimal defaultVal = 0)
        {
            decimal d;
            return decimal.TryParse(str, out d) ? d : defaultVal;
        }

        /// <summary>
        /// ToDecimal，失败返回默认值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static decimal AsDecimal(this decimal? value, decimal defaultValue = 0)
        {
            return value == null ? defaultValue : value.Value;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static decimal? AsNullableDecimal(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            return str.AsDecimal();
        }

        /// <summary>
        /// 字符串转int
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="defaultVal">默认值</param>
        /// <returns></returns>
        public static int AsInt(this string str, int defaultVal = 0)
        {
            int d;
            return int.TryParse(str, out d) ? d : defaultVal;
        }

        /// <summary>
        /// 分割成整数数组
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static List<int> AsIntList(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return new List<int>();
            }

            return str.Split(',').Select(it => it.AsInt()).ToList();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="str"></param>
        /// <param name="defaultVal"></param>
        /// <returns></returns>
        public static long AsLong(this string str, int defaultVal = 0)
        {
            long d;
            return long.TryParse(str, out d) ? d : defaultVal;
        }

        /// <summary>
        /// Guid转资源锁
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string AsLockKey(this Guid value)
        {
            if (value == Guid.Empty)
            {
                return Guid.NewGuid().ToString().ToLowerInvariant();
            }

            return value.ToString();
        }

        /// <summary>
        /// 字符串转bool
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool AsBool(this string str)
        {
            bool d;
            return bool.TryParse(str, out d) ? d : false;
        }

        /// <summary>
        /// 将字符串转换为时间
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static DateTime AsDateTime(this string str)
        {
            DateTime time;
            return DateTime.TryParse(str, out time) ? time : DateTime.MinValue;
        }

        /// <summary>
        /// 重载字符串转时间，这里可以定义日期格式
        /// </summary>
        /// <param name="str"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static DateTime AsDateTime(this string str, string format)
        {
            DateTime time;
            DateTime.TryParseExact(str, format, CultureInfo.CurrentCulture, DateTimeStyles.None, out time);

            return time;
        }

        /// <summary>
        /// 将字符串转换为GUID
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static Guid AsGuid(this string str)
        {
            Guid valGuid;
            return Guid.TryParse(str, out valGuid) ? valGuid : Guid.Empty;
        }

        /// <summary>
        /// 将字符串转换为GUID
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static Guid? AsNullGuid(this string str)
        {
            Guid valGuid;
            return Guid.TryParse(str, out valGuid) ? (Guid?)valGuid : null;
        }

        /// <summary>
        /// 将字符串转换为时间
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static DateTime? AsNullableDateTime(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }

            DateTime dt;
            if (DateTime.TryParse(str, out dt))
            {
                return dt;
            }

            // 格式：43478.3333333333
            double d;
            if (double.TryParse(str, out d))
            {
                return DateTime.FromOADate(d);
            }

            // 格式：/OADate(43477)/
            var rg = new Regex(@"(?<=\()[^\(\)]+(?=\))", RegexOptions.Multiline | RegexOptions.Singleline);
            if (double.TryParse(rg.Match(str).Value, out d))
            {
                return DateTime.FromOADate(d);
            }

            return null;
        }

        /// <summary>
        /// 将时间转为yyyy-MM-dd
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string FormatShortString(this DateTime time)
        {
            return time.ToString("yyyy-MM-dd");
        }

        /// <summary>
        /// 将时间转为yyyy-MM-dd HH:mm:ss
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string FormatLongString(this DateTime time)
        {
            return time.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 将字符串切割成Guid数组
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Guid[] AsGuidArray(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return new Guid[] { };
            }

            return str.Split(',').Select(it => it.ToGuid()).Where(it => it != Guid.Empty).ToArray();
        }

        /// <summary>
        /// 将字符串按分隔符割成字符串数组
        /// </summary>
        /// <param name="str">源字符串</param>
        /// <param name="separator">分隔符，默认是英文逗号</param>
        /// <returns></returns>
        public static String[] AsStringArray(this string str, char separator = ',')
        {
            if (string.IsNullOrEmpty(str))
            {
                return new string[] { };
            }

            return str.Split(separator);
        }

        /// <summary>
        /// 将字符串切割成Guid数组
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static List<Guid> AsGuidList(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return new List<Guid>();
            }

            return str.Split(',').Select(it => it.ToGuid()).Where(it => it != Guid.Empty).ToList();
        }

        /// <summary>
        /// GUID?转GUID
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static Guid AsGuid(this Guid? guid)
        {
            return guid.HasValue ? guid.Value : Guid.Empty;
        }
    }
}
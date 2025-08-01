using System;
using System.Collections.Generic;
using System.Text;

namespace Simple.Utils.Helper
{
    public class DateTimeHelper
    {
        /// <summary>
        /// 时间戳转换为日期（时间戳单位秒）
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static DateTime UnixToTime(long timeStamp)
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return start.AddMilliseconds(timeStamp).AddHours(8);
        }

        /// <summary>
        /// 时间戳字符串转换为日期（时间戳单位秒）
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static DateTime UnixToTime(string timeStamp)
        {
            return UnixToTime(Convert.ToInt64(timeStamp));
        }

        /// <summary>
        /// 获取秒级时间戳
        /// </summary>
        /// <returns>当前时间戳</returns>
        public static long ToTimestamp(DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue)
            {
                dateTime = DateTime.Now;
            }

            return (dateTime.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }
    }
}
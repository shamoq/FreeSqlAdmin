using Simple.Utils.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Utils.Extensions
{
    /// <summary>
    /// 时间扩展
    /// </summary>
    public static class DateExtension
    {
        /// <summary>
        /// 获取unix时间戳
        /// </summary>
        /// <returns></returns>
        public static int ToUnix(this DateTime date)
        {
            return (int)((date.ToUniversalTime().Ticks - 621355968000000000) / 10000000);
        }
    }
}
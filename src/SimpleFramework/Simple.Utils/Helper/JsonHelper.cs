using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Simple.Utils.Exceptions;

namespace Simple.Utils.Helper
{
    /// <summary>
    /// Json 序列化帮助类
    /// </summary>
    public class JsonHelper
    {
        /// <summary>
        /// 转换为Json字符串
        /// </summary>
        /// <param name="value"></param>
        /// <param name="timeFormate"></param>
        /// <returns></returns>
        public static string Json(object value, string timeFormate = "yyyy-MM-dd HH:mm:ss")
        {
            return ToJson(value, timeFormate);
        }

        /// <summary>
        /// 转换为其他类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="timeFormate"></param>
        /// <returns></returns>
        public static T JsonMap<T>(object value, string timeFormate = "yyyy-MM-dd HH:mm:ss")
        {
            var json = ToJson(value, timeFormate);
            return FromJson<T>(json);
        }

        /// <summary>
        /// 序列化为字符串
        /// </summary>
        /// <returns></returns>
        public static string ToJson(object value, string timeFormate = "yyyy-MM-dd HH:mm:ss")
        {
            try
            {
                var setting = new JsonSerializerSettings();
                setting.DateFormatString = timeFormate;
                return JsonConvert.SerializeObject(value, setting);
            }
            catch (Exception ex)
            {
                throw new CustomException("类型转换为Json字符串失败", ex);
            }
        }

        /// <summary>
        /// 序列化为字符串
        /// </summary>
        /// <returns></returns>
        public static T FromJson<T>(string value, string timeFormate = "yyyy-MM-dd HH:mm:ss")
        {
            try
            {
                var setting = new JsonSerializerSettings();
                setting.DateFormatString = timeFormate;
                return JsonConvert.DeserializeObject<T>(value, setting);
            }
            catch (Exception ex)
            {
                throw new CustomException("从Json字符串获取类型失败", ex);
            }
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Utils.Helper
{
    /// <summary>全局唯一标识符 (GUID) 帮助类</summary>
    public class GuidHelper
    {
        #region 自动生成编号

        /// <summary>表示全局唯一标识符 (GUID)。</summary>
        /// <returns></returns>
        public static string GuId()
        {
            return Guid.NewGuid().ToString();
        }

        /// <summary>自动生成编号/唯一订单号生成，时间戳+随机数，时间戳精确到毫秒，形如2020052113254137177350</summary>
        /// <returns></returns>
        public static string CreateNo()
        {
            Random random = new Random();
            string strRandom = random.Next(1000, 10000).ToString().PadLeft(5, '0'); //生成随机编号
            string code = DateTime.Now.ToString("yyyyMMddHHmmssffff") + strRandom;//形如2020052113254137177350
            return code;
        }

        #endregion 自动生成编号

        /// <summary>获取一个大写的字符串</summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string upper(string str)
        {
            return str.ToUpper();
        }

        /// <summary>获取32位不包含“-”号的GUID字符串</summary>
        /// <returns></returns>
        public static string NewGuidFormatN(bool isUpper = false)
        {
            var guid = Guid.NewGuid().ToString("N");
            return isUpper == true ? upper(guid) : guid;
        }

        /// <summary>获取32位包含“-”号的GUID字符串</summary>
        /// <param name="isUpper"></param>
        /// <returns></returns>
        public static string NewGuidFormatD(bool isUpper = false)
        {
            var guid = Guid.NewGuid().ToString("D");
            return isUpper == true ? upper(guid) : guid;
        }

        /// <summary>获取32位包含“-”号的GUID被“(”、“)”包括的字符串</summary>
        /// <param name="isUpper"></param>
        /// <returns></returns>
        public static string NewGuidFormatP(bool isUpper = false)
        {
            var guid = Guid.NewGuid().ToString("P");
            return isUpper == true ? upper(guid) : guid;
        }

        /// <summary>获取32位包含“-”号的GUID被“{”、“}”包括的字符串</summary>
        /// <param name="isUpper"></param>
        /// <returns></returns>
        public static string NewGuidFormatB(bool isUpper = false)
        {
            var guid = Guid.NewGuid().ToString("B");
            return isUpper == true ? upper(guid) : guid;
        }

        /// <summary>获取4个被“{”、“}”包括的十六进制数，其中第四个值位8个被“{”、“}”包括的十六进制数字符串</summary>
        /// <param name="isUpper"></param>
        /// <returns></returns>
        public static string NewGuidFormatX(bool isUpper = false)
        {
            var guid = Guid.NewGuid().ToString("X");
            return isUpper == true ? upper(guid) : guid;
        }
        
        /// <summary>
        /// 将不带 '-' 的 GUID 字符串转换为 Guid 对象
        /// </summary>
        /// <param name="guidString">不带 '-' 的 GUID 字符串</param>
        /// <returns>转换后的 Guid 对象</returns>
        public static Guid ConvertToGuid(string guidString)
        {
            // 使用 ParseExact 方法将不带 '-' 的 GUID 字符串转换为 Guid 对象
            // "N" 格式说明符表示解析一个 32 位的十六进制字符串，不包含分隔符
            return Guid.TryParseExact(guidString, "N", out var guid) ? guid : Guid.Empty;
        }
        
    }
}
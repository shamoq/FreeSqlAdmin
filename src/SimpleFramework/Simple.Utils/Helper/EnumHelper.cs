using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Utils.Helper
{
    /// <summary>
    /// 枚举扩展
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        ///  获取枚举的中文描述
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetDescription(Enum obj)
        {
            string objName = obj.ToString();
            Type t = obj.GetType();
            FieldInfo fi = t.GetField(objName);
            DescriptionAttribute[] arrDesc = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return arrDesc[0].Description;
        }

        /// <summary>
        /// 获取枚举值的Description描述
        /// </summary>
        public static string GetDescription(object value, Type enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("enumType 必须是枚举类型");
            }

            if (value == null)
            {
                return string.Empty;
            }

            var enumValue = Enum.ToObject(enumType, value);
            if (!Enum.IsDefined(enumType, enumValue))
            {
                return string.Empty;
            }

            return GetDescription((Enum)enumValue);
        }
    }
}
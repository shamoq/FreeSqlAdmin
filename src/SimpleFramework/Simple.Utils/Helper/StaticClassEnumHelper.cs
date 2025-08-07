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
    /// 枚举静态类帮助类
    /// </summary>
    public class StaticClassEnumHelper
    {
        /// <summary>
        /// 根据value获取枚举的描述
        /// </summary>
        public static string GetDescription(object value, Type enumType)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException(nameof(enumType));
            }

            if (value == null)
            {
                return string.Empty;
            }

            var fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(int))
                {
                    var fieldValue = (int)field.GetValue(null);
                    if (fieldValue == (int)value)
                    {
                        var descriptionAttribute = field.GetCustomAttribute<DescriptionAttribute>();
                        return descriptionAttribute?.Description ?? string.Empty;
                    }
                }
                else if (field.FieldType == typeof(string))
                {
                    var fieldValue = field.GetValue(null)?.ToString();
                    if (fieldValue == value.ToString())
                    {
                        var descriptionAttribute = field.GetCustomAttribute<DescriptionAttribute>();
                        return descriptionAttribute?.Description ?? string.Empty;
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 根据描述获取枚举的value
        /// </summary>
        public static T GetEnumValue<T>(string description, Type enumType)
        {
            if (enumType == null)
            {
                throw new ArgumentNullException(nameof(enumType));
            }
            
            if (string.IsNullOrEmpty(description))
            {
                return default(T);
            }

            var fields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            foreach (var field in fields)
            {
                if (field.FieldType == typeof(int))
                {
                    var fieldValue = (int)field.GetValue(null);
                    var descriptionAttribute = field.GetCustomAttribute<DescriptionAttribute>();
                    if (descriptionAttribute != null && descriptionAttribute.Description == description)
                    {
                        return (T)(object)fieldValue;
                    }
                }
                if (field.FieldType == typeof(string))
                {
                    var fieldValue = field.GetValue(null).ToString();
                    var descriptionAttribute = field.GetCustomAttribute<DescriptionAttribute>();
                    if (descriptionAttribute != null && descriptionAttribute.Description == description)
                    {
                        return (T)(object)fieldValue;
                    }
                }
            }

            return default(T);
        }

        /// <summary>
        /// 获取枚举列表
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<OptionObjectDto> GetOptions(Type type)
        {
            var result = new List<OptionObjectDto>();

            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Static;
            foreach (FieldInfo info in type.GetFields(bindingAttr))
            {
                if (info.FieldType == typeof(int))
                {
                    var desc = string.Empty;
                    int enumValue = Convert.ToInt32(info.GetValue(null));

                    var descriptionAttribute = info.GetCustomAttribute<DescriptionAttribute>();
                    desc = descriptionAttribute?.Description ?? string.Empty;

                    result.Add(new OptionObjectDto(enumValue, desc));
                }
                else if (info.FieldType == typeof(string))
                {
                    var desc = string.Empty;
                    var enumValue = info.GetValue(null);

                    var descriptionAttribute = info.GetCustomAttribute<DescriptionAttribute>();
                    desc = descriptionAttribute?.Description ?? string.Empty;

                    result.Add(new OptionObjectDto(enumValue, desc));
                }
            }

            return result.OrderBy(t => t.Id).ToList();
        }
    }
}
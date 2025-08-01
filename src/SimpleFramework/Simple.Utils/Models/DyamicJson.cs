using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Simple.Utils.Extensions;
using Simple.Utils.Helper;

namespace Simple.Utils.Models;

public class DyamicJson
{
    [Newtonsoft.Json.JsonExtensionData]
    [System.Text.Json.Serialization.JsonExtensionData]
    [NotMapped]
    public Dictionary<string, object> AdditionalData { get; private set; } =
        new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    
    public T GetAttributeValue<T>(string key)
    {
        if (AdditionalData.TryGetValue(key, out var val))
        {
            if (val is JObject jobject)
            {
                return jobject.ToObject<T>();
            }

            if (val is JArray jArray)
            {
                return jArray.ToObject<T>();
            }

            return (T)TypeConvertHelper.ConvertType(val, typeof(T));
        }

        PropertyInfo property = GetType()
            .GetProperty(key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (property != null)
        {
            var value = property.GetValue(this);
            if (property.PropertyType == typeof(T))
            {
                return (T)value;
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }

        return default;
    }

    public object GetAttributeValue(string key)
    {
        return GetAttributeValue<object>(key);
    }

    public void SetAttributeValue(string key, object value, bool isConvertName = true)
    {
        PropertyInfo property = GetType()
            .GetProperty(key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (property != null)
        {
            // 类型转换
            object convertedValue = TypeConvertHelper.ConvertType(value, property.PropertyType);
            property.SetValue(this, convertedValue);
        }
        else
        {
            var newName = isConvertName ? key.ToCamelCase() : key;
            AdditionalData[newName] = value;
        }
    }

    public (T1, T2, T3, T4, T5) GetAttributeValue<T1, T2, T3, T4, T5>(string key1, string key2, string key3,
        string key4, string key5)
    {
        return (
            GetAttributeValue<T1>(key1),
            GetAttributeValue<T2>(key2),
            GetAttributeValue<T3>(key3),
            GetAttributeValue<T4>(key4),
            GetAttributeValue<T5>(key5)
        );
    }


    public (T1, T2, T3, T4) GetAttributeValue<T1, T2, T3, T4>(string key1, string key2, string key3, string key4)
    {
        return (
            GetAttributeValue<T1>(key1),
            GetAttributeValue<T2>(key2),
            GetAttributeValue<T3>(key3),
            GetAttributeValue<T4>(key4)
        );
    }

    public (T1, T2, T3) GetAttributeValue<T1, T2, T3>(string key1, string key2, string key3)
    {
        return (
            GetAttributeValue<T1>(key1),
            GetAttributeValue<T2>(key2),
            GetAttributeValue<T3>(key3)
        );
    }

    public (T1, T2) GetAttributeValue<T1, T2>(string key1, string key2)
    {
        return (
            GetAttributeValue<T1>(key1),
            GetAttributeValue<T2>(key2)
        );
    }
}
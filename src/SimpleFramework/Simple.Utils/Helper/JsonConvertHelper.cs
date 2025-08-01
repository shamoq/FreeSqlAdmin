using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Simple.Utils.Helper;

public static class JsonConvertHelper
{
    /// <summary>
    /// 转换成字典
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, object> ToDictionary(string objJson)
    {
        var jObject = JObject.Parse(objJson);
        var dict = new Dictionary<string, object>();
        foreach (var property in jObject.Properties())
        {
            var value = property.Value;
            if (value is JObject itemObject)
            {
                ToFlatObject(property.Name, itemObject, dict);
            }
            else if (value is JArray itemArray)
            {
                var list = new List<Dictionary<string, object>>();
                foreach (var item in itemArray)
                {
                    if (item is JObject arrayItemObject)
                    {
                        var itemDict = new Dictionary<string, object>();
                        ToFlatObject(property.Name, arrayItemObject, itemDict);
                        list.Add(itemDict);
                    }
                }
                dict[property.Name] = list;
            }
            else
            {
                dict[property.Name] = value.ToObject<object>();
            }
        }
        return dict;
    }

    private static void ToFlatObject(string propertyName, JObject jObject, Dictionary<string, object> dict)
    {
        foreach (var property in jObject.Properties())
        {
            var value = property.Value;
            if (value is JObject nestedObject)
            {
                ToFlatObject(property.Name, nestedObject, dict);
            }
            else if (value is JArray nestedArray)
            {
                var list = new List<Dictionary<string, object>>();
                foreach (var item in nestedArray)
                {
                    if (item is JObject arrayItemObject)
                    {
                        var itemDict = new Dictionary<string, object>();
                        ToFlatObject(property.Name, arrayItemObject, itemDict);
                        list.Add(itemDict);
                    }
                }
                dict[propertyName] = list;
            }
            else
            {
                dict[property.Name] = value.ToObject<object>();
            }
        }
    }
}
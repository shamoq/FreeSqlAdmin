namespace Simple.Utils.Json;

using Newtonsoft.Json;
using System;

public class DateTimeJsonConverter : JsonConverter
{
    private readonly string _format;

    public DateTimeJsonConverter(string format)
    {
        _format = format;
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(DateTime) || objectType == typeof(DateTime?);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        if (value is DateTime dateTime)
        {
            writer.WriteValue(dateTime.ToString(_format));
        }
        else
        {
            writer.WriteNull();
        }
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return reader.Value == null ? null : DateTime.Parse(reader.Value.ToString());
    }
}
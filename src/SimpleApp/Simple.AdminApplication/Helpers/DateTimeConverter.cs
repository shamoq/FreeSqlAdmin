using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Simple.AdminApplication.Helpers;

public class DateTimeConverter: JsonConverter<DateTime>
{
    private readonly string _dateFormat;

    public DateTimeConverter(string dateFormat)
    {
        _dateFormat = dateFormat;
    }

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String && DateTime.TryParseExact(reader.GetString(), _dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime dateTime))
        {
            return dateTime;
        }

        return default;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(_dateFormat));
    }
}
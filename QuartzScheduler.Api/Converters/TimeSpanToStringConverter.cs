using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuartzScheduler.Api.Converters
{
    public class TimeSpanToStringConverter : JsonConverter<TimeSpan?>
    {
        public static string Format => "00:00:00";

        public override TimeSpan? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (!TimeSpan.TryParseExact(value, @"hh\:mm\:ss", null, out TimeSpan result))
                throw new ArgumentException($"Incorrect TimeSpan value: {value}");

            return result;
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan? value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}

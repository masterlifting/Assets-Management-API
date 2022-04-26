using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IM.Service.Common.Net.Helpers;

public static class JsonHelper
{
    public static JsonSerializerOptions Options { get; }

    static JsonHelper()
    {
        Options = new(JsonSerializerDefaults.Web);
        Options.Converters.Add(new DateOnlyConverter());
        Options.Converters.Add(new TimeOnlyConverter());
    }
    public static bool TryDeserialize<T>(string data, out T? result) where T : class
    {
        result = null;

        try
        {
            result = JsonSerializer.Deserialize<T>(data, Options);
            return true;
        }
        catch
        {
            return false;
        }
    }
    public static bool TrySerialize<T>(T data, out string result) where T : class
    {
        if (data is string stringData)
        {
            result = stringData;
            return true;
        }

        try
        {
            result = JsonSerializer.Serialize(data, Options);
            return true;
        }
        catch
        {
            result = string.Empty;
            return false;
        }
    }

    public class DateOnlyConverter : JsonConverter<DateOnly>
    {
        private readonly string serializationFormat;

        public DateOnlyConverter() : this(null)
        {
        }

        public DateOnlyConverter(string? serializationFormat)
        {
            this.serializationFormat = serializationFormat ?? "yyyy-MM-dd";
        }

        public override DateOnly Read(ref Utf8JsonReader reader,
            Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return DateOnly.Parse(value!);
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value,
            JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(serializationFormat));
    }
    public class TimeOnlyConverter : JsonConverter<TimeOnly>
    {
        private readonly string serializationFormat;

        public TimeOnlyConverter() : this(null)
        {
        }

        public TimeOnlyConverter(string? serializationFormat)
        {
            this.serializationFormat = serializationFormat ?? "HH:mm:ss";
        }

        public override TimeOnly Read(ref Utf8JsonReader reader,
            Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return TimeOnly.Parse(value!);
        }

        public override void Write(Utf8JsonWriter writer, TimeOnly value,
            JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(serializationFormat));
    }
}
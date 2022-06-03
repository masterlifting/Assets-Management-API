using System;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IM.Service.Shared.Helpers;

public static class JsonHelper
{
    public static JsonSerializerOptions Options { get; }

    static JsonHelper()
    {
        Options = new(JsonSerializerDefaults.Web);
        Options.Converters.Add(new DateOnlyConverter());
        Options.Converters.Add(new TimeOnlyConverter());
    }
    public static T Deserialize<T>(string data) where T : class
    {
        try
        {
            var result = JsonSerializer.Deserialize<T>(data, Options);
            return result ?? throw new NullReferenceException("Serialization result is NULL");
        }
        catch (Exception exception)
        {
            throw new SerializationException("Error by deserialization object: '" + typeof(T).Name + "'.Message: " + exception.Message);
        }
    }
    public static string Serialize<T>(T data) where T : class
    {
        if (data is string stringData)
            return stringData;

        try
        {
           return JsonSerializer.Serialize(data, Options);
        }
        catch (Exception exception)
        {
            throw new SerializationException("Error by serialization object: '" + typeof(T).Name + "'.Message: " + exception.Message);
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
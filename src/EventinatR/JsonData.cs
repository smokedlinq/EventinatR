using System.Text.Json.Serialization;
using EventinatR.Serialization;

namespace EventinatR;

public record JsonData(JsonDataType Type, [property: JsonConverter(typeof(BinaryDataConverter))] BinaryData Value)
{
    private static readonly JsonDataOptions DefaultOptions = new();

    public override string ToString()
        => Value.ToString();

    public static JsonData From<T>(T value, JsonSerializerOptions serializerOptions)
        => From(value, new JsonDataOptions(serializerOptions));

    public static JsonData From<T>(T value, JsonDataOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(value);

        options ??= DefaultOptions;

        var type = JsonDataType.For(value);
        var valueAsJson = JsonSerializer.Serialize(value, value.GetType(), options.SerializerOptions);
        var jsonValue = BinaryData.FromString(valueAsJson);

        return new JsonData(type, jsonValue);
    }

    public T? As<T>(JsonSerializerOptions serializerOptions)
        => As<T>(new JsonDataOptions(serializerOptions));

    public T? As<T>(JsonDataOptions? options = null)
        => JsonDataDeserializer<T>.Deserialize(Type, Value, options ?? DefaultOptions);

    public T As<T>(T defaultValue, JsonSerializerOptions serializerOptions)
        => As<T>(defaultValue, new JsonSerializerOptions(serializerOptions));

    public T As<T>(T defaultValue, JsonDataOptions? options = null)
        => JsonDataDeserializer<T>.Deserialize(Type, Value, options ?? DefaultOptions) ?? defaultValue;
}

using System.Text.Json.Serialization;
using System.Collections.Concurrent;

namespace EventinatR;

public record JsonData(JsonDataType Type, [property: JsonConverter(typeof(BinaryDataConverter))] BinaryData Value)
{
    private static readonly JsonSerializerOptions DefaultSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public override string ToString()
        => Value.ToString();

    public static JsonData From<T>(T value, JsonSerializerOptions? serializerOptions = null)
    {
        _ = value ?? throw new ArgumentNullException(nameof(value));

        var type = JsonDataType.For(value);
        var valueAsJson = JsonSerializer.Serialize(value, value.GetType(), serializerOptions ?? DefaultSerializerOptions);
        var jsonValue = BinaryData.FromString(valueAsJson);

        return new JsonData(type, jsonValue);
    }

    public T? As<T>(JsonSerializerOptions? serializerOptions = null)
        => JsonDataDeserializer<T>.Deserialize(Type, Value, serializerOptions ?? DefaultSerializerOptions);
}

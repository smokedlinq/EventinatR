using System.Text.Json.Serialization;

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
    {
        serializerOptions ??= DefaultSerializerOptions;
        var type = typeof(T);
        object? value;

        if (type == typeof(JsonDocument) || type == typeof(JsonElement))
        {
            value = JsonDocument.Parse(Value, new JsonDocumentOptions
            {
                AllowTrailingCommas = serializerOptions.AllowTrailingCommas,
                CommentHandling = serializerOptions.ReadCommentHandling,
                MaxDepth = serializerOptions.MaxDepth
            });

            if (value is T document)
            {
                return document;
            }

            value = ((JsonDocument)value).RootElement;

            if (value is T element)
            {
                return element;
            }
        }

        var desiredType = JsonDataType.For(typeof(T));

        if (desiredType == Type)
        {
            return Value.ToObjectFromJson<T>(serializerOptions);
        }

        type = System.Type.GetType(Type.Name, false, true);

        if (type is null)
        {
            type = System.Type.GetType($"{Type.Name}, {Type.Assembly}", false, true);
        }

        if (type is null)
        { 
            return Value.ToObjectFromJson<T>(serializerOptions);
        }

        value = JsonSerializer.Deserialize(Value, type, serializerOptions);

        if (value is T result)
        {
            return result;
        }

        return default;
    }
}

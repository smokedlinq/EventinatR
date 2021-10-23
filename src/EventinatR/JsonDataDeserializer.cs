using System.Collections.Concurrent;

namespace EventinatR;

internal static class JsonDataDeserializer<T>
{
    private static readonly ConcurrentDictionary<JsonDataType, Func<BinaryData, JsonSerializerOptions, T?>> Types = new ();

    public static T? Deserialize(JsonDataType type, BinaryData data, JsonSerializerOptions options)
    {
        if (typeof(T) == typeof(JsonDocument))
        {
            return FromJsonDocument(data, options);
        }

        if (typeof(T) == typeof(JsonElement))
        {
            return FromJsonElement(data, options);
        }

        if (!Types.TryGetValue(type, out var func))
        {
            func = Create(type);
            Types.TryAdd(type, func);
        }

        return func(data, options);
    }

    private static Func<BinaryData, JsonSerializerOptions, T?> Create(JsonDataType type)
    {
        var desiredType = JsonDataType.For(typeof(T));

        if (desiredType == type)
        {
            return (data, options) => data.ToObjectFromJson<T>(options);
        }

        var actualType = Type.GetType(type.Name, false, true);

        if (actualType is null)
        {
            actualType = Type.GetType($"{type.Name}, {type.Assembly}", false, true);
        }

        if (actualType is null)
        {
            return (data, options) => data.ToObjectFromJson<T>(options);
        }

        return (data, options) =>
        {
            var value = JsonSerializer.Deserialize(data, actualType, options);
            if (value is T result)
            {
                return result;
            }
            return default;
        };
    }

    private static JsonDocument ParseJsonDocument(BinaryData data, JsonSerializerOptions options)
        => JsonDocument.Parse(data, new JsonDocumentOptions
        {
            AllowTrailingCommas = options.AllowTrailingCommas,
            CommentHandling = options.ReadCommentHandling,
            MaxDepth = options.MaxDepth
        });

    private static T? FromJsonDocument(BinaryData data, JsonSerializerOptions options)
    {
        var value = ParseJsonDocument(data, options);

        if (value is T document)
        {
            return document;
        }

        return default;
    }

    private static T? FromJsonElement(BinaryData data, JsonSerializerOptions options)
    {
        var value = ParseJsonDocument(data, options);

        if (value.RootElement is T element)
        {
            return element;
        }

        return default;
    }
}

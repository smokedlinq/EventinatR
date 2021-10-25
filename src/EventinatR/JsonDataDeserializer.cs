using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace EventinatR;

internal static class JsonDataDeserializer<T>
{
    private delegate T? JsonDataConverter(BinaryData data, JsonSerializerOptions options);
    private static readonly ConcurrentDictionary<JsonDataType, JsonDataConverter> Types = new();

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

    private static JsonDataConverter Create(JsonDataType type)
    {
        var desiredType = JsonDataType.For(typeof(T));

        if (desiredType == type)
        {
            return (data, options) => data.ToObjectFromJson<T>(options);
        }

        if (typeof(T) == typeof(BinaryData))
        {
            return (data, options) => From<BinaryData>(data);
        }

        if (typeof(T) == typeof(Stream))
        {
            return (data, options) => From<Stream>(data.ToStream());
        }

        if (typeof(T) == typeof(byte[]))
        {
            return (data, options) => From<byte[]>(data.ToArray());
        }

        if (typeof(T) == typeof(string))
        {
            return (data, options) => From<string>(data.ToString());
        }

        if (typeof(T) == typeof(ReadOnlyMemory<byte>))
        {
            return (data, options) => From<ReadOnlyMemory<byte>>(data.ToMemory());
        }

        if (type.TryToType(out var actualType))
        {
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

        return (data, options) => data.ToObjectFromJson<T>(options);
    }

    private static T? From<TSource>(TSource source)
        => source is T result ? result : default;

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

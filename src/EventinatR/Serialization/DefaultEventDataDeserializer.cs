using System;
using System.Linq.Expressions;
using System.Text.Json;

namespace EventinatR.Serialization
{
    public static class DefaultEventDataDeserializer
    {
        public static IEventDeserializerBuilder UseDefault<T>(this IEventDeserializerBuilder builder, JsonSerializerOptions? serializerOptions = null)
        {
            var type = typeof(T);
            var options = Expression.Constant(serializerOptions ?? new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var converterType = typeof(EventTypeDeserializer<>).MakeGenericType(type);
            var ctor = converterType.GetConstructor(new[] { typeof(JsonSerializerOptions) });
            var call = Expression.New(ctor!, options);
            var cast = Expression.Convert(call, typeof(IEventDataDeserializer));
            var deserializer = Expression.Lambda<Func<IEventDataDeserializer>>(cast).Compile().Invoke();

            return builder.Use(deserializer);
        }

        private class EventTypeDeserializer<T> : IEventDataDeserializer
        {
            private static readonly string TypeName = typeof(T).FullName ?? typeof(T).Name;
            private readonly JsonSerializerOptions _serializerOptions;

            public EventTypeDeserializer(JsonSerializerOptions? serializerOptions = null)
                => _serializerOptions = serializerOptions ?? new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

            public bool CanDeserialize(Event @event)
                => string.Equals(@event.Type, TypeName, StringComparison.OrdinalIgnoreCase);

            object? IEventDataDeserializer.Deserialize(BinaryData data)
                => Deserialize(data);

            public virtual T? Deserialize(BinaryData data)
                => data.ToObjectFromJson<T>(_serializerOptions);
        }
    }
}

using System;
using System.Linq.Expressions;
using System.Text.Json;

namespace EventinatR.Serialization
{
    public static class DefaultEventDataConverter
    {
        public static IEventDataDeserializerBuilder UseDefault<T>(this IEventDataDeserializerBuilder builder, JsonSerializerOptions? serializerOptions = null)
        {
            var type = typeof(T);
            var options = Expression.Constant(serializerOptions ?? new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var converterType = typeof(EventDataConverter<>).MakeGenericType(type);
            var ctor = converterType.GetConstructor(new[] { typeof(JsonSerializerOptions) });
            var call = Expression.New(ctor!, options);
            var cast = Expression.Convert(call, typeof(IEventDataConverter));
            var deserializer = Expression.Lambda<Func<IEventDataConverter>>(cast).Compile().Invoke();

            return builder.Use(deserializer);
        }

        private class EventDataConverter<T> : IEventDataConverter
        {
            private static readonly string TypeName = typeof(T).FullName ?? typeof(T).Name;
            private readonly JsonSerializerOptions _serializerOptions;

            public EventDataConverter(JsonSerializerOptions? serializerOptions = null)
                => _serializerOptions = serializerOptions ?? new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

            public bool CanConverter(Event @event)
                => string.Equals(@event.Type, TypeName, StringComparison.OrdinalIgnoreCase);

            object? IEventDataConverter.Convert(BinaryData data)
                => Deserialize(data);

            public virtual T? Deserialize(BinaryData data)
                => data.ToObjectFromJson<T>(_serializerOptions);
        }
    }
}

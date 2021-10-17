using System;
using System.Linq.Expressions;
using System.Text.Json;

namespace EventinatR.Serialization
{
    public static class DefaultEventDataConverter
    {
        public static IEventConverterBuilder UseDefault<T>(this IEventConverterBuilder builder, JsonSerializerOptions? serializerOptions = null)
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
            var converter = Expression.Lambda<Func<IEventDataConverter>>(cast).Compile().Invoke();

            return builder.Use(converter);
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
                => Convert(data);

            public virtual T? Convert(BinaryData data)
                => data.ToObjectFromJson<T>(_serializerOptions);
        }
    }
}

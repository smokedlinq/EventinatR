using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text.Json;

namespace EventinatR
{
    public class EventConverter
    {
        private readonly Dictionary<string, IEventConverter> _converters = new();

        public EventConverter(Action<Builder> configure)
        {
            var builder = new Builder(this);
            configure(builder);
        }

        public bool TryConvert<T>(EventinatR.Event @event, [MaybeNullWhen(false)] out T result)
        {
            result = default;

            if (_converters.ContainsKey(@event.Type))
            {
                var obj = _converters[@event.Type].Convert(@event.Data);

                if (obj is T value)
                {
                    result = value;
                    return true;
                }
            }

            return false;
        }

        public class Builder
        {
            private readonly EventConverter _converter;

            public Builder(EventConverter converter)
                => _converter = converter ?? throw new ArgumentNullException(nameof(converter));

            public void Register<T>(JsonSerializerOptions? serializerOptions = null)
                => Register(typeof(T), serializerOptions);

            private void Register(Type type, JsonSerializerOptions? serializerOptions = null)
            {
                var options = Expression.Constant(serializerOptions ?? new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var converterType = typeof(EventConverter<>).MakeGenericType(type);
                var ctor = converterType.GetConstructor(new[] { typeof(JsonSerializerOptions) });
                var call = Expression.New(ctor!, options);
                var cast = Expression.Convert(call, typeof(IEventConverter));
                var converter = Expression.Lambda<Func<IEventConverter>>(cast).Compile().Invoke();

                _converter._converters.Add(type.FullName!, converter);
            }

            public void AddConverter<T>(EventConverter<T> converter)
                => _converter._converters.Add(typeof(T).FullName!, converter);
        }
    }

    public class EventConverter<T> : IEventConverter
    {
        private readonly JsonSerializerOptions _serializerOptions;

        public EventConverter(JsonSerializerOptions? serializerOptions = null)
            => _serializerOptions = serializerOptions ?? new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

        object? IEventConverter.Convert(BinaryData data)
            => Convert(data);

        public virtual T? Convert(BinaryData data)
            => data.ToObjectFromJson<T>(_serializerOptions);
    }
}

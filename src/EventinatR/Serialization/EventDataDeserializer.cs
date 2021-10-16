using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;

namespace EventinatR.Serialization
{
    public class EventDataDeserializer
    {
        private readonly List<IEventDataConverter> _converters = new();

        public EventDataDeserializer(Action<IEventDataDeserializerBuilder> configure)
        {
            var builder = new EventDeserializerBuilder(_converters);
            configure(builder);
            _converters.Reverse();
        }

        protected EventDataDeserializer()
        {
        }

        internal IEnumerable<IEventDataConverter> Converters => _converters.AsEnumerable();

        public virtual bool TryDeserialize<T>(Event @event, [MaybeNullWhen(false)] out T result)
        {
            result = default;

            var deserializer = _converters.FirstOrDefault(x => x.CanConverter(@event));
            var obj = deserializer?.Convert(@event.Data);

            if (obj is T value)
            {
                result = value;
                return true;
            }

            return false;
        }

        private class EventDeserializerBuilder : IEventDataDeserializerBuilder
        {
            private readonly ICollection<IEventDataConverter> _converters;

            public EventDeserializerBuilder(ICollection<IEventDataConverter> converters)
                => _converters = converters ?? throw new ArgumentNullException(nameof(converters));

            public IEventDataDeserializerBuilder Use<T>()
                where T : IEventDataConverter, new()
                => Use(new T());

            public IEventDataDeserializerBuilder Use<T>(T converter)
                where T : IEventDataConverter
            {
                _converters.Add(converter ?? throw new ArgumentNullException(nameof(converter)));
                return this;
            }
        }
    }
}

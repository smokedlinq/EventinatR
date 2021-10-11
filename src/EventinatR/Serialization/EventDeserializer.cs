using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;

namespace EventinatR.Serialization
{
    public class EventDeserializer
    {
        private readonly List<IEventDataDeserializer> _deserializers = new();

        public EventDeserializer(Action<IEventDeserializerBuilder> configure)
        {
            var builder = new EventDeserializerBuilder(_deserializers);
            configure(builder);
            _deserializers.Reverse();
        }

        protected EventDeserializer()
        {
        }

        public bool TryDeserialize<T>(EventinatR.Event @event, [MaybeNullWhen(false)] out T result)
        {
            result = default;

            var deserializer = _deserializers.FirstOrDefault(x => x.CanDeserialize(@event));
            var obj = deserializer?.Deserialize(@event.Data);

            if (obj is T value)
            {
                result = value;
                return true;
            }

            return false;
        }

        private class EventDeserializerBuilder : IEventDeserializerBuilder
        {
            private readonly ICollection<IEventDataDeserializer> _deserializers;

            public EventDeserializerBuilder(ICollection<IEventDataDeserializer> deserializers)
                => _deserializers = deserializers ?? throw new ArgumentNullException(nameof(deserializers));

            public IEventDeserializerBuilder Use<T>()
                where T : IEventDataDeserializer, new()
                => Use(new T());

            public IEventDeserializerBuilder Use<T>(T deserializer)
                where T : IEventDataDeserializer
            {
                _deserializers.Add(deserializer ?? throw new ArgumentNullException(nameof(deserializer)));
                return this;
            }
        }
    }
}

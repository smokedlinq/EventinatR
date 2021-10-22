using System.Diagnostics.CodeAnalysis;

namespace EventinatR.Serialization;

public class EventConverter
{
    private readonly List<IEventDataConverter> _converters = new();

    public EventConverter(Action<IEventConverterBuilder> configure)
    {
        var builder = new EventConverterBuilder(_converters);
        configure(builder);
        _converters.Reverse();
    }

    protected EventConverter()
    {
    }

    internal IEnumerable<IEventDataConverter> Converters => _converters.AsEnumerable();

    public virtual bool TryConvert<T>(Event @event, [MaybeNullWhen(false)] out T result)
    {
        result = default;

        var converter = _converters.FirstOrDefault(x => x.CanConverter(@event));
        var obj = converter?.Convert(@event.Data);

        if (obj is T value)
        {
            result = value;
            return value is not null;
        }

        return false;
    }

    private class EventConverterBuilder : IEventConverterBuilder
    {
        private readonly ICollection<IEventDataConverter> _converters;

        public EventConverterBuilder(ICollection<IEventDataConverter> converters)
            => _converters = converters ?? throw new ArgumentNullException(nameof(converters));

        public IEventConverterBuilder Use<T>()
            where T : IEventDataConverter, new()
            => Use(new T());

        public IEventConverterBuilder Use<T>(T converter)
            where T : IEventDataConverter
        {
            _converters.Add(converter ?? throw new ArgumentNullException(nameof(converter)));
            return this;
        }
    }
}

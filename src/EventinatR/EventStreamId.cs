using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using EventinatR.Serialization;

namespace EventinatR;

[JsonConverter(typeof(EventStreamIdJsonConverter))]
public record EventStreamId(string Value)
{
    public static readonly EventStreamId None = new(string.Empty);

    public EventStreamId Concat(EventStreamId id)
        => new($"{Value}:{id.Value}");

    public override string ToString()
        => Value;

    public static implicit operator EventStreamId(string value)
        => new(value);

    public static EventStreamId operator +(EventStreamId x, EventStreamId y)
        => x.Concat(y);

    public static EventStreamId ConvertTo<T>()
        => ConvertTo(typeof(T));

    public static EventStreamId ConvertTo(Type type)
        => new(Regex.Replace(type.Name, "([A-Z])", "-$1", RegexOptions.Compiled).ToLower(CultureInfo.InvariantCulture).TrimStart('-'));
}

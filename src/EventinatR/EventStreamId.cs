using System.Globalization;
using System.Text.RegularExpressions;

namespace EventinatR;

public record EventStreamId(string Value)
{
    public static readonly EventStreamId None = new(string.Empty);

    public EventStreamId Concat(EventStreamId id)
        => new($"{Value}:{id.Value}");

    public static implicit operator EventStreamId(string value)
        => new(value);

    public static EventStreamId operator +(EventStreamId x, EventStreamId y)
        => x.Concat(y);

    public static EventStreamId ConvertTo<T>()
        => ConvertTo(typeof(T));

    public static EventStreamId ConvertTo(Type type)
        => new(Regex.Replace(type.FullName ?? type.Name, "([A-Z])", "-$1", RegexOptions.Compiled).ToLower(CultureInfo.InvariantCulture).TrimStart('-'));
}

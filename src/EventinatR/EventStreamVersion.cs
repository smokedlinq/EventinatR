using System.Globalization;
using System.Text.Json.Serialization;
using EventinatR.Serialization;

namespace EventinatR;

[JsonConverter(typeof(EventStreamVersionJsonConverter))]
public record EventStreamVersion(long Value)
{
    public static readonly EventStreamVersion None = new(0L);

    public override string ToString()
        => Value.ToString("D0", CultureInfo.CurrentCulture);

    public static implicit operator EventStreamVersion(int value)
        => new((long)value);

    public static implicit operator EventStreamVersion(long value)
        => new(value);

    public static implicit operator long(EventStreamVersion value)
        => value.Value;

    public static EventStreamVersion operator +(EventStreamVersion x, EventStreamVersion y)
        => new(x.Value + y.Value);

    public static EventStreamVersion operator -(EventStreamVersion x, EventStreamVersion y)
        => new(x.Value - y.Value);

    public static EventStreamVersion operator ++(EventStreamVersion x)
        => new(x.Value + 1);

    public static EventStreamVersion operator --(EventStreamVersion x)
        => new(x.Value - 1);

    public static bool operator <(EventStreamVersion x, EventStreamVersion y)
        => x.Value < y.Value;

    public static bool operator <=(EventStreamVersion x, EventStreamVersion y)
        => x.Value <= y.Value;

    public static bool operator >(EventStreamVersion x, EventStreamVersion y)
        => x.Value > y.Value;

    public static bool operator >=(EventStreamVersion x, EventStreamVersion y)
        => x.Value >= y.Value;
}

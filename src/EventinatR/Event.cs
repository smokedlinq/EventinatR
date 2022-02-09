using System.Diagnostics.CodeAnalysis;

namespace EventinatR;

public record Event(EventStreamId StreamId, EventStreamVersion Version, EventStreamTransaction Transaction, DateTimeOffset Timestamp, JsonData Data)
{
    public bool TryConvert<T>([MaybeNullWhen(false)] out T result, JsonDataOptions? options = null)
        where T : class
    {
        result = Data.As<T>(options);
        return result is not null;
    }
}

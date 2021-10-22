namespace EventinatR;

public abstract class EventStreamSnapshot<T> : IEventStreamReader
{
    public EventStreamSnapshot(EventStreamId streamId, EventStreamVersion? version = null, T? state = default)
    {
        StreamId = streamId ?? throw new ArgumentNullException(nameof(streamId));
        Version = version ?? EventStreamVersion.None;
        State = state;
    }

    public EventStreamId StreamId { get; }
    public EventStreamVersion Version { get; }
    public T? State { get; }

    public abstract IAsyncEnumerable<Event> ReadAsync(CancellationToken cancellationToken = default);
}

namespace EventinatR;

public abstract class EventStreamSnapshot<T>
{
    public EventStreamSnapshot(EventStreamId streamId, EventStreamVersion? version = null, T? state = default)
    {
        if (streamId == EventStreamId.None)
        {
            throw new ArgumentException("The value cannot be None.", nameof(streamId));
        }

        StreamId = streamId;
        Version = version ?? EventStreamVersion.None;
        State = state;
    }

    public EventStreamId StreamId { get; }
    public EventStreamVersion Version { get; }
    public T? State { get; }

    public abstract IAsyncEnumerable<Event> ReadAsync(CancellationToken cancellationToken = default);
}

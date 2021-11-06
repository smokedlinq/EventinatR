namespace EventinatR;

public abstract class EventStream
{
    protected EventStream()
        : this(EventStreamId.None)
    {
    }

    public EventStream(EventStreamId id)
        => Id = id ?? throw new ArgumentNullException(nameof(id));

    public EventStreamId Id { get; }

    public abstract Task<EventStreamVersion> GetVersionAsync(CancellationToken cancellationToken = default);

    public virtual Task<EventStreamVersion> AppendAsync<T>(IEnumerable<T> events, CancellationToken cancellationToken = default)
        where T : class
        => AppendAsync<T, object>(events, null!, cancellationToken);

    public abstract Task<EventStreamVersion> AppendAsync<TEvent, TState>(IEnumerable<TEvent> events, TState state, CancellationToken cancellationToken = default)
        where TEvent : class;

    public abstract IAsyncEnumerable<Event> ReadAsync(CancellationToken cancellationToken = default);

    public abstract Task<EventStreamSnapshot<T>> ReadSnapshotAsync<T>(CancellationToken cancellationToken = default);

    public abstract Task<EventStreamSnapshot<T>> WriteSnapshotAsync<T>(T state, EventStreamVersion version, CancellationToken cancellationToken = default);
}

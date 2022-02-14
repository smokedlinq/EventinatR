namespace EventinatR;

public abstract class EventStream
{
    protected EventStream()
        : this(EventStreamId.None)
    {
    }

    public EventStream(EventStreamId id)
    {
        if (id == EventStreamId.None)
        {
            throw new ArgumentException("The value cannot be None.", nameof(id));
        }

        Id = id;
    }

    public EventStreamId Id { get; }
    public abstract EventStreamSnapshotStore Snapshots { get; }

    public abstract Task<EventStreamVersion> GetVersionAsync(CancellationToken cancellationToken = default);

    public virtual Task<EventStreamVersion> AppendAsync<T>(IEnumerable<T> events, CancellationToken cancellationToken = default)
        where T : class
        => AppendAsync<T, object>(events, null!, cancellationToken);

    public virtual Task<EventStreamVersion> AppendAsync<TEvent, TState>(IEnumerable<TEvent> events, TState state, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        if (events.Any(x => x is null))
        {
            throw new ArgumentException("The collection must not contain a null item.", nameof(events));
        }

        return AppendAsync();

        async Task<EventStreamVersion> AppendAsync()
        {
            var version = await GetVersionAsync(cancellationToken).ConfigureAwait(false);

            if (!events.Any())
            {
                return version;
            }

            var context = CreateTransactionContext(version, events);
            
            await AppendAsync<TState>(context, state, cancellationToken).ConfigureAwait(false);

            return new EventStreamVersion(version.Value);
        }
    }

    protected virtual TransactionContext CreateTransactionContext<T>(EventStreamVersion version, IEnumerable<T> events)
    {
        if (!events.TryGetNonEnumeratedCount(out var count))
        {
            count = events.Count();
        }

        var transaction = new EventStreamTransaction(version + count, count);
        var streamEvents = events.Select(x => new Event(Id, ++version, transaction, DateTimeOffset.Now, JsonData.From(x)));

        return new(version, transaction, streamEvents);
    }

    protected abstract Task AppendAsync<TState>(TransactionContext context, TState state, CancellationToken cancellationToken);

    public abstract IAsyncEnumerable<Event> ReadAsync(CancellationToken cancellationToken = default);

    protected record TransactionContext(EventStreamVersion CurrentVersion, EventStreamTransaction Transaction, IEnumerable<Event> Events);
}

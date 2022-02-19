namespace EventinatR.InMemory;

internal class InMemoryEventStream : EventStream, IDisposable
{
    private readonly List<Event> _events = new();
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly InMemoryEventStreamSnapshotStore _snapshots;

    public InMemoryEventStream(EventStreamId id)
        : base(id)
    {
        _snapshots = new InMemoryEventStreamSnapshotStore(this);
    }

    public override EventStreamSnapshotStore Snapshots => _snapshots;

    public override Task<EventStreamVersion> GetVersionAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(new EventStreamVersion(_events.Count));

    public override IAsyncEnumerable<Event> ReadAsync(CancellationToken cancellationToken = default)
        => _events.ToAsyncEnumerable();

    public override async Task<EventStreamVersion> AppendAsync<TEvent, TState>(IEnumerable<TEvent> events, TState state, CancellationToken cancellationToken = default)
    {
        // Lock to prevent race condition on event version
        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            return await base.AppendAsync(events, state, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _lock.Release();
        }
    }

    protected override async Task AppendAsync<TState>(TransactionContext context, TState state, CancellationToken cancellationToken = default)
    {
        _events.AddRange(context.Events);

        if (state is not null)
        {
            await _snapshots.SaveAsync(state, context.Transaction.Version, cancellationToken).ConfigureAwait(false);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _lock.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

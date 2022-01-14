using System.Collections.Concurrent;

namespace EventinatR.InMemory;

internal class InMemoryEventStream : EventStream, IDisposable
{
    private readonly List<Event> _events = new();
    private readonly ConcurrentDictionary<Type, object> _snapshots = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public InMemoryEventStream(EventStreamId id)
        : base(id)
    {
    }

    public override Task<EventStreamVersion> GetVersionAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(new EventStreamVersion(_events.Count));

    public override IAsyncEnumerable<Event> ReadAsync(CancellationToken cancellationToken = default)
        => _events.ToAsyncEnumerable();

    public override Task<EventStreamSnapshot<T>> ReadSnapshotAsync<T>(CancellationToken cancellationToken = default)
    {
        var type = typeof(T);
        var snapshot = _snapshots.ContainsKey(type)
            ? (EventStreamSnapshot<T>)_snapshots[type]
            : new InMemoryEventStreamSnapshot<T>(this);

        return Task.FromResult(snapshot);
    }

    public override Task<EventStreamSnapshot<T>> WriteSnapshotAsync<T>(T state, EventStreamVersion version, CancellationToken cancellationToken = default)
    {
        _ = state ?? throw new ArgumentNullException(nameof(state));

        var type = typeof(T);
        var snapshot = new InMemoryEventStreamSnapshot<T>(this, version, state);
        _snapshots[type] = snapshot;
        return Task.FromResult<EventStreamSnapshot<T>>(snapshot);
    }

    public override Task<EventStreamVersion> AppendAsync<TEvent, TState>(IEnumerable<TEvent> collection, TState state, CancellationToken cancellationToken = default)
    {
        _ = collection ?? throw new ArgumentNullException(nameof(collection));

        if (collection.Any(x => x is null))
        {
            throw new ArgumentException("The collection must not contain a null item.", nameof(collection));
        }

        return AppendAsync();

        async Task<EventStreamVersion> AppendAsync()
        {
            // Lock to prevent race condition on event version
            await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);

            if (!collection.TryGetNonEnumeratedCount(out var count))
            {
                count = collection.Count();
            }

            var transaction = new EventStreamTransaction(_events.Count + count, count);

            try
            {
                foreach (var data in collection)
                {
                    var @event = new Event(Id, _events.Count + 1, transaction, DateTimeOffset.Now, JsonData.From(data));
                    _events.Add(@event);
                }

                var version = new EventStreamVersion(_events.Count);

                if (state is not null)
                {
                    await WriteSnapshotAsync(state, version, cancellationToken).ConfigureAwait(false);
                }

                return version;
            }
            finally
            {
                _lock.Release();
            }
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

using System.Collections.Concurrent;

namespace EventinatR.InMemory;

internal class InMemoryEventStreamSnapshotStore : EventStreamSnapshotStore
{
    private readonly ConcurrentDictionary<Type, object> _snapshots = new();
    private readonly InMemoryEventStream _stream;

    public InMemoryEventStreamSnapshotStore(InMemoryEventStream stream)
        => _stream = stream ?? throw new ArgumentNullException(nameof(stream));

    public override Task<EventStreamSnapshot<T>> GetAsync<T>(CancellationToken cancellationToken = default)
    {
        var type = typeof(T);
        var snapshot = _snapshots.TryGetValue(type, out var value)
            ? (EventStreamSnapshot<T>)value
            : new InMemoryEventStreamSnapshot<T>(_stream);

        return Task.FromResult(snapshot);
    }

    public override Task<EventStreamSnapshot<T>> SaveAsync<T>(T state, EventStreamVersion version, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);

        var type = typeof(T);
        var snapshot = new InMemoryEventStreamSnapshot<T>(_stream, version, state);
        _snapshots[type] = snapshot;
        return Task.FromResult<EventStreamSnapshot<T>>(snapshot);
    }
}

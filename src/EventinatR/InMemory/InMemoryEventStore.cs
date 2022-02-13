using System.Collections.Concurrent;

namespace EventinatR.InMemory;

public class InMemoryEventStore : EventStore
{
    private readonly ConcurrentDictionary<EventStreamId, EventStream> _streams = new();

    public override Task<EventStream> GetStreamAsync(EventStreamId id, CancellationToken cancellationToken = default)
    {
        var stream = _streams.GetOrAdd(id, _ => new InMemoryEventStream(id));
        return Task.FromResult(stream);
    }
}

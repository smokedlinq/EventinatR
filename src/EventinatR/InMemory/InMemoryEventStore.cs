using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace EventinatR.InMemory
{
    public class InMemoryEventStore : EventStore
    {
        private readonly ConcurrentDictionary<EventStreamId, EventStream> _streams = new();

        public override Task<EventStream> GetStreamAsync(EventStreamId id, CancellationToken cancellationToken = default)
        {
            EventStream? stream;

            if (!_streams.ContainsKey(id))
            {
                stream = new InMemoryEventStream(id);

                if (_streams.TryAdd(id, stream))
                {
                    return Task.FromResult(stream);
                }
            }

            if (_streams.TryGetValue(id, out stream))
            {
                return Task.FromResult(stream);
            }

            throw new InvalidOperationException("Stream does not exist and could not create it.");
        }
    }
}

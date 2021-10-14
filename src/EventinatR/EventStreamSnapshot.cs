using System.Collections.Generic;
using System.Threading;

namespace EventinatR
{
    public abstract class EventStreamSnapshot<T> : IEventStreamReader
    {
        public EventStreamSnapshot(EventStreamVersion? version = null, T? state = default)
        {
            Version = version ?? EventStreamVersion.None;
            State = state;
        }

        public EventStreamVersion Version { get; }
        public T? State { get; }

        public abstract IAsyncEnumerable<Event> ReadAsync(CancellationToken cancellationToken = default);
    }
}

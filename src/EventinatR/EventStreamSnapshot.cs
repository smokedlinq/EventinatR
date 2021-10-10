using System.Collections.Generic;
using System.Threading;

namespace EventinatR
{
    public abstract class EventStreamSnapshot<T>
    {
        public EventStreamSnapshot(long version = default, T? state = default)
        {
            Version = version;
            State = state;
        }

        public long Version { get; }
        public T? State { get; }

        public abstract IAsyncEnumerable<Event> ReadAsync(CancellationToken cancellationToken = default);
    }
}

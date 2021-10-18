using System;
using System.Threading;
using System.Threading.Tasks;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("EventinatR.Tests")]

namespace EventinatR
{
    public abstract class EventStore
    {
        public virtual Task<EventStream> GetStreamAsync(string id, CancellationToken cancellationToken = default)
            => GetStreamAsync(new EventStreamId(id ?? throw new ArgumentNullException(nameof(id))), cancellationToken);

        public abstract Task<EventStream> GetStreamAsync(EventStreamId id, CancellationToken cancellationToken = default);
    }
}

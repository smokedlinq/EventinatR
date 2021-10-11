using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventinatR
{
    public abstract class EventStream
    {
        protected EventStream()
            : this(string.Empty)
        {
        }

        public EventStream(string id)
            => Id = id ?? throw new ArgumentNullException(nameof(id));

        public string Id { get; }

        public abstract Task<EventStreamVersion> AppendAsync<T>(IEnumerable<T> events, CancellationToken cancellationToken = default)
            where T : class;

        public abstract IAsyncEnumerable<Event> ReadAsync(CancellationToken cancellationToken = default);

        public abstract Task<EventStreamSnapshot<T>> ReadSnapshotAsync<T>(CancellationToken cancellationToken = default);

        public abstract Task<EventStreamSnapshot<T>> WriteSnapshotAsync<T>(T state, EventStreamVersion version, CancellationToken cancellationToken = default);
    }
}

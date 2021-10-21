using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventinatR
{
    public abstract class EventStream : IEventStreamReader
    {
        protected EventStream()
            : this(EventStreamId.None)
        {
        }

        public EventStream(EventStreamId id)
            => Id = id ?? throw new ArgumentNullException(nameof(id));

        public EventStreamId Id { get; }

        public virtual Task<EventStreamVersion> AppendAsync<T>(T @event, CancellationToken cancellationToken = default)
            where T : class
            => AppendAsync<T>(new[] { @event ?? throw new ArgumentNullException(nameof(@event)) }, cancellationToken);

        public abstract Task<EventStreamVersion> AppendAsync<T>(IEnumerable<T> events, CancellationToken cancellationToken = default)
            where T : class;

        public abstract IAsyncEnumerable<Event> ReadAsync(CancellationToken cancellationToken = default);

        public abstract Task<EventStreamSnapshot<T>> ReadSnapshotAsync<T>(CancellationToken cancellationToken = default);

        public abstract Task<EventStreamSnapshot<T>> WriteSnapshotAsync<T>(T state, EventStreamVersion version, CancellationToken cancellationToken = default);
    }
}

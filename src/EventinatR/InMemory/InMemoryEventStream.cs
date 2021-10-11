using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventinatR.InMemory
{
    internal class InMemoryEventStream : EventStream, IDisposable
    {
        private readonly List<Event> _events = new();
        private readonly ConcurrentDictionary<Type, object> _snapshots = new();
        private readonly SemaphoreSlim _lock = new(1, 1);

        public InMemoryEventStream(string id)
            : base(id)
        {
        }

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
            var type = typeof(T);
            var snapshot = new InMemoryEventStreamSnapshot<T>(this, version, state);
            _snapshots[type] = snapshot;
            return Task.FromResult<EventStreamSnapshot<T>>(snapshot);
        }

        public override async Task<EventStreamVersion> AppendAsync<T>(IEnumerable<T> collection, CancellationToken cancellationToken = default)
        {
            // Lock to prevent race condition on event version
            await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                foreach (var data in collection)
                {
                    var @event = new Event(_events.Count, DateTimeOffset.Now, data.GetType().FullName!, new BinaryData(data));
                    _events.Add(@event);
                }

                return new EventStreamVersion(_events.Count);
            }
            finally
            {
                _lock.Release();
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
}

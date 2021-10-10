using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace EventinatR.InMemory
{
    internal class InMemoryEventStreamSnapshot<T> : EventStreamSnapshot<T>
    {
        private readonly InMemoryEventStream _stream;

        public InMemoryEventStreamSnapshot(InMemoryEventStream stream, long version = default, T? state = default)
            : base(version, state)
            => _stream = stream ?? throw new ArgumentNullException(nameof(stream));

        public override IAsyncEnumerable<Event> ReadAsync(CancellationToken cancellationToken = default)
            => _stream.ReadAsync(cancellationToken).Where(x => x.Version > Version);
    }
}

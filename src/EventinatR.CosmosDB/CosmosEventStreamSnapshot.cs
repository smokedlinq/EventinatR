using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;

namespace EventinatR.CosmosDB
{
    internal class CosmosEventStreamSnapshot<T> : EventStreamSnapshot<T>
    {
        private readonly CosmosEventStream _stream;

        public CosmosEventStreamSnapshot(CosmosEventStream stream, long version = 0, T? state = default)
            : base(version, state)
            => _stream = stream ?? throw new ArgumentNullException(nameof(stream));

        public override IAsyncEnumerable<Event> ReadAsync(CancellationToken cancellationToken = default)
            => _stream.ReadFromVersionAsync(Version, cancellationToken);

        public static string CreateSnapshotId(string streamId)
            => $"{streamId}:{GetSnapshotNameFromType()}";

        private static string GetSnapshotNameFromType()
            => Regex.Replace(typeof(T).Name ?? typeof(T).Name, "([A-Z])", "-$1", RegexOptions.Compiled).ToLower(CultureInfo.InvariantCulture).TrimStart('-');
    }
}

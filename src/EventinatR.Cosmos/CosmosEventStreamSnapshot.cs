using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;

namespace EventinatR.Cosmos
{
    internal class CosmosEventStreamSnapshot<T> : EventStreamSnapshot<T>
    {
        private readonly CosmosEventStream _stream;

        public CosmosEventStreamSnapshot(CosmosEventStream stream, EventStreamVersion? version = null, T? state = default)
            : base(version, state)
            => _stream = stream ?? throw new ArgumentNullException(nameof(stream));

        public override IAsyncEnumerable<Event> ReadAsync(CancellationToken cancellationToken = default)
            => _stream.ReadFromVersionAsync(Version.Value, cancellationToken);

        public static string CreateSnapshotId(string streamId)
            => $"{streamId}:{GetSnapshotNameFromType()}";

        private static string GetSnapshotNameFromType()
            => Regex.Replace(typeof(T).Name ?? typeof(T).Name, "([A-Z])", "-$1", RegexOptions.Compiled).ToLower(CultureInfo.InvariantCulture).TrimStart('-');
    }
}

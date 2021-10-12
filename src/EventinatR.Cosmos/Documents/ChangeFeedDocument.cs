using System;
using Newtonsoft.Json.Linq;

namespace EventinatR.Cosmos.Documents
{
    internal record ChangeFeedDocument(string StreamId, string Id, long Version, string Type, DateTimeOffset Timestamp, string DataType, JToken Data, string StateType, JToken State) : Document(StreamId, Id, Version, Type)
    {
        public bool IsEvent
            => string.Equals(Type, DocumentTypes.Event, StringComparison.OrdinalIgnoreCase);

        public bool IsSnapshot
            => string.Equals(Type, DocumentTypes.Snapshot, StringComparison.OrdinalIgnoreCase);

        public bool IsStream
            => string.Equals(Type, DocumentTypes.Stream, StringComparison.OrdinalIgnoreCase);

        public EventDocument? ToEventDocument()
            => IsEvent ? new EventDocument(StreamId, Id, Version, Timestamp, DataType, Data) : null;

        public SnapshotDocument<T>? ToSnapshotDocument<T>()
            => IsSnapshot ? new SnapshotDocument<T>(StreamId, Id, Version, StateType, State.ToObject<T>()) : null;

        public StreamDocument? ToStreamDocument()
            => IsStream ? new StreamDocument(StreamId, Id, Version) : null;
    }
}

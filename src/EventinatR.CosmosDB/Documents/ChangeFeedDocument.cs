using System;
using Newtonsoft.Json.Linq;

namespace EventinatR.CosmosDB.Documents
{
    public record ChangeFeedDocument(string StreamId, string Id, long Version, string Type, DateTimeOffset Timestamp, string DataType, JObject Data, string StateType, JObject State) : Document(StreamId, Id, Version, Type)
    {
        public bool IsEvent
            => string.Equals(Type, DocumentTypes.Event, StringComparison.OrdinalIgnoreCase);

        public bool IsSnapshot
            => string.Equals(Type, DocumentTypes.Snapshot, StringComparison.OrdinalIgnoreCase);

        public bool IsStream
            => string.Equals(Type, DocumentTypes.Stream, StringComparison.OrdinalIgnoreCase);

        public EventDocument? ToEvent()
            => IsEvent ? new EventDocument(StreamId, Id, Version, Timestamp, DataType, Data) : null;

        public SnapshotDocument<T>? ToSnapshot<T>()
            => IsSnapshot ? new SnapshotDocument<T>(StreamId, Id, Version, StateType, State.ToObject<T>()) : null;

        public StreamDocument? ToStream()
            => IsStream ? new StreamDocument(StreamId, Id, Version) : null;
    }
}

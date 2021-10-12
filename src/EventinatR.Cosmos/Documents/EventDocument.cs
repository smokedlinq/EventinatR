using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EventinatR.Cosmos.Documents
{
    internal record EventDocument(string StreamId, string Id, long Version, DateTimeOffset Timestamp, string DataType, JToken Data) : Document(StreamId, Id, Version, DocumentTypes.Event)
    {
        public Event AsEvent()
            => new(Version, Timestamp, DataType, new BinaryData(Data.ToString(Formatting.None)));
    }
}

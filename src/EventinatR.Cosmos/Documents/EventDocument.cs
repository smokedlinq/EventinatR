using System.Text.Json.Serialization;

namespace EventinatR.Cosmos.Documents;

internal record EventDocument(
    string StreamId,
    string Id,
    long Version,
    DateTimeOffset Timestamp,
    string DataType,
    [property: JsonConverter(typeof(BinaryDataConverter))] BinaryData Data)
    : Document(StreamId, Id, Version, DocumentTypes.Event)
{
    public Event ToEvent()
        => new(new EventStreamId(StreamId), Version, Timestamp, DataType, Data);
}

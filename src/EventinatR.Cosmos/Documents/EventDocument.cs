namespace EventinatR.Cosmos.Documents;

internal record EventDocument(
    string StreamId,
    string Id,
    long Version,
    DateTimeOffset Timestamp,
    JsonData Data)
    : Document(StreamId, Id, Version, DocumentTypes.Event)
{
    public Event ToEvent()
        => new(new EventStreamId(StreamId), Version, Timestamp, Data);
}

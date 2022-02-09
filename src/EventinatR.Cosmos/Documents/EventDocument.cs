namespace EventinatR.Cosmos.Documents;

internal record EventDocument(
    string Stream,
    string Id,
    long Version,
    EventStreamTransaction Transaction,
    DateTimeOffset Timestamp,
    JsonData Data)
    : Document(Stream, Id, Version, DocumentTypes.Event)
{
    public Event ToEvent()
        => new(new EventStreamId(Stream), Version, Transaction, Timestamp, Data);
}

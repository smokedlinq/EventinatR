namespace EventinatR.Cosmos.Documents;

internal record EventDocument(
    EventStreamId Stream,
    EventStreamId Id,
    EventStreamVersion Version,
    EventStreamTransaction Transaction,
    DateTimeOffset Timestamp,
    JsonData Data)
    : Document(Stream, Id, Version, DocumentTypes.Event)
{
    public Event ToEvent()
        => new(Stream, Version, Transaction, Timestamp, Data);
}

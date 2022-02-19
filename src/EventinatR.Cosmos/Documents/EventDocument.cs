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
    public static EventDocument FromEvent(EventStreamId stream, EventStreamId id, Event e)
        => new(stream, id, e.Version, e.Transaction, e.Timestamp, e.Data);

    public Event ToEvent()
        => new(Stream, Version, Transaction, Timestamp, Data);
}

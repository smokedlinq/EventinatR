namespace EventinatR.Cosmos.Documents;

internal record ChangeFeedDocument(
    EventStreamId Stream,
    EventStreamId Id,
    EventStreamVersion Version,
    string Type,
    DateTimeOffset Timestamp,
    EventStreamTransaction Transaction,
    JsonData? Data,
    JsonData? State)
{
    public bool IsEvent
        => string.Equals(Type, DocumentTypes.Event, StringComparison.OrdinalIgnoreCase);

    public bool IsSnapshot
        => string.Equals(Type, DocumentTypes.Snapshot, StringComparison.OrdinalIgnoreCase);

    public bool IsStream
        => string.Equals(Type, DocumentTypes.Stream, StringComparison.OrdinalIgnoreCase);

    public EventDocument ToEventDocument()
        => IsEvent ? new EventDocument(Stream, Id, Version, Transaction, Timestamp, Data!) : throw new InvalidOperationException("The document is not an event.");

    public SnapshotDocument ToSnapshotDocument()
        => IsSnapshot ? new SnapshotDocument(Stream, Id, Version, State!) : throw new InvalidOperationException("The document is not a snapshot.");

    public StreamDocument ToStreamDocument()
        => IsStream ? new StreamDocument(Stream, Id, Version) : throw new InvalidOperationException("The document is not a stream.");
}

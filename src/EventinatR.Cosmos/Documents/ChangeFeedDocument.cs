namespace EventinatR.Cosmos.Documents;

internal record ChangeFeedDocument(
    string Stream,
    string Id,
    long Version,
    string Type,
    DateTimeOffset Timestamp,
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
        => IsEvent ? new EventDocument(Stream, Id, Version, Timestamp, Data!) : throw new InvalidOperationException("The document is not an event.");

    public SnapshotDocument ToSnapshotDocument()
        => IsSnapshot ? new SnapshotDocument(Stream, Id, Version, State!) : throw new InvalidOperationException("The document is not a snapshot.");

    public StreamDocument ToStreamDocument()
        => IsStream ? new StreamDocument(Stream, Id, Version) : throw new InvalidOperationException("The document is not a stream.");
}

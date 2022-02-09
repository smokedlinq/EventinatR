namespace EventinatR.Cosmos.Documents;

internal record SnapshotDocument(
    EventStreamId Stream,
    EventStreamId Id,
    EventStreamVersion Version,
    JsonData State)
    : Document(Stream, Id, Version, DocumentTypes.Snapshot);

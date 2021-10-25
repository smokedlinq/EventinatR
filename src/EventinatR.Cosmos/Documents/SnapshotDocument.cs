namespace EventinatR.Cosmos.Documents;

internal record SnapshotDocument(
    string Stream,
    string Id,
    long Version,
    JsonData State)
    : Document(Stream, Id, Version, DocumentTypes.Snapshot);

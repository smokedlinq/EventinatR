using System.Text.Json.Serialization;

namespace EventinatR.Cosmos.Documents;

internal record SnapshotDocument(
    string StreamId,
    string Id,
    long Version,
    JsonData State)
    : Document(StreamId, Id, Version, DocumentTypes.Snapshot);

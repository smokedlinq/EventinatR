namespace EventinatR.Cosmos.Documents
{
    internal record SnapshotDocument<T>(string StreamId, string Id, long Version, string StateType, T? State) : Document(StreamId, Id, Version, DocumentTypes.Snapshot);
}

namespace EventinatR.Cosmos.Documents
{
    internal record StreamDocument(string StreamId, string Id, long Version) : Document(StreamId, Id, Version, DocumentTypes.Stream);
}

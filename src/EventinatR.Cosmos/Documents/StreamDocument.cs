namespace EventinatR.Cosmos.Documents
{
    public record StreamDocument(string StreamId, string Id, long Version) : Document(StreamId, Id, Version, DocumentTypes.Stream);
}

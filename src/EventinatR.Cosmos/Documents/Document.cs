namespace EventinatR.Cosmos.Documents
{
    public abstract record Document(string StreamId, string Id, long Version, string Type);
}

namespace EventinatR.Cosmos.Documents;

internal abstract record Document(string StreamId, string Id, long Version, string Type);

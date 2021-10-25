namespace EventinatR.Cosmos.Documents;

internal abstract record Document(string Stream, string Id, long Version, string Type);

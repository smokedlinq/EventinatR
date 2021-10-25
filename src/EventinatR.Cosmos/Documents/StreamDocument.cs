namespace EventinatR.Cosmos.Documents;

internal record StreamDocument(string Stream, string Id, long Version)
    : Document(Stream, Id, Version, DocumentTypes.Stream);

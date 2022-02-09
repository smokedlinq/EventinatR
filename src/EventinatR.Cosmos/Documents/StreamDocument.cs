namespace EventinatR.Cosmos.Documents;

internal record StreamDocument(EventStreamId Stream, EventStreamId Id, EventStreamVersion Version)
    : Document(Stream, Id, Version, DocumentTypes.Stream);

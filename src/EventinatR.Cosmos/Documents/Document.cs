namespace EventinatR.Cosmos.Documents;

internal abstract record Document(EventStreamId Stream, EventStreamId Id, EventStreamVersion Version, string Type);

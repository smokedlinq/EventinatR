using System.Text.Json.Serialization;

namespace EventinatR.Cosmos.Documents;

internal record ChangeFeedDocument(
    string StreamId,
    string Id,
    long Version,
    string Type,
    DateTimeOffset Timestamp,
    string DataType,
    [property: JsonConverter(typeof(BinaryDataConverter))] BinaryData Data,
    string StateType,
    [property: JsonConverter(typeof(BinaryDataConverter))] BinaryData State)
{
    public bool IsEvent
        => string.Equals(Type, DocumentTypes.Event, StringComparison.OrdinalIgnoreCase);

    public bool IsSnapshot
        => string.Equals(Type, DocumentTypes.Snapshot, StringComparison.OrdinalIgnoreCase);

    public bool IsStream
        => string.Equals(Type, DocumentTypes.Stream, StringComparison.OrdinalIgnoreCase);

    public EventDocument ToEventDocument()
        => IsEvent ? new EventDocument(StreamId, Id, Version, Timestamp, DataType, Data) : throw new InvalidOperationException("The document is not an event.");

    public SnapshotDocument ToSnapshotDocument()
        => IsSnapshot ? new SnapshotDocument(StreamId, Id, Version, StateType, State) : throw new InvalidOperationException("The document is not a snapshot.");

    public StreamDocument ToStreamDocument()
        => IsStream ? new StreamDocument(StreamId, Id, Version) : throw new InvalidOperationException("The document is not a stream.");
}

namespace EventinatR.Cosmos;

internal class CosmosEventStreamSnapshot<T> : EventStreamSnapshot<T>
{
    private readonly CosmosEventStreamReader _reader;

    public CosmosEventStreamSnapshot(EventStreamId streamId, CosmosEventStreamReader reader, T? state = default)
        : base(streamId ?? throw new ArgumentNullException(nameof(streamId)), reader.Version, state)
        => _reader = reader ?? throw new ArgumentNullException(nameof(reader));

    public override IAsyncEnumerable<Event> ReadAsync(CancellationToken cancellationToken = default)
        => _reader.ReadAsync(cancellationToken);

    internal static EventStreamId CreateSnapshotId(EventStreamId streamId)
        => streamId + EventStreamId.ConvertTo<T>();

}

namespace EventinatR.Cosmos;

internal class CosmosEventStreamSnapshot<T> : EventStreamSnapshot<T>
{
    private readonly CosmosEventStream _stream;

    public CosmosEventStreamSnapshot(CosmosEventStream stream, EventStreamVersion? version = null, T? state = default)
        : base(stream?.Id ?? throw new ArgumentNullException(nameof(stream)), version, state)
        => _stream = stream;

    public override IAsyncEnumerable<Event> ReadAsync(CancellationToken cancellationToken = default)
        => _stream.ReadFromVersionAsync(Version.Value, cancellationToken);

    internal static EventStreamId CreateSnapshotId(EventStreamId streamId)
        => streamId + EventStreamId.ConvertTo<T>();

}

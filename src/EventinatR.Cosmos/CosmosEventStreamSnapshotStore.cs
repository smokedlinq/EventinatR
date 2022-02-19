namespace EventinatR.Cosmos;

internal class CosmosEventStreamSnapshotStore : EventStreamSnapshotStore
{
    private readonly EventStreamId _streamId;
    private readonly Container _container;
    private readonly PartitionKey _partitionKey;
    private readonly JsonSerializerOptions _serializerOptions;

    public CosmosEventStreamSnapshotStore(EventStreamId streamId, Container container, PartitionKey partitionKey, JsonSerializerOptions serializerOptions)
    {
        _streamId = streamId ?? throw new ArgumentNullException(nameof(streamId));
        _container = container ?? throw new ArgumentNullException(nameof(container));
        _partitionKey = partitionKey;
        _serializerOptions = serializerOptions ?? throw new ArgumentNullException(nameof(serializerOptions));
    }

    public override async Task<EventStreamSnapshot<T>> GetAsync<T>(CancellationToken cancellationToken = default)
    {
        var id = CosmosEventStreamSnapshot<T>.CreateSnapshotId(_streamId.Value);
        var snapshot = await ReadAsync<T>(id, cancellationToken).ConfigureAwait(false);
        var reader = new CosmosEventStreamReader(_container, _partitionKey, snapshot?.Resource?.Version);
        var state = snapshot?.Resource?.State;
        var stateOfT = state is null ? default : state.As<T>();

        return new CosmosEventStreamSnapshot<T>(_streamId, reader, stateOfT);
    }

    public override Task<EventStreamSnapshot<T>> SaveAsync<T>(T state, EventStreamVersion version, CancellationToken cancellationToken = default)
        => CreateAsync(state, version, (document, eTag) =>
            _container.UpsertItemAsync(document, _partitionKey, new ItemRequestOptions
            {
                EnableContentResponseOnWrite = false,
                IfMatchEtag = eTag
            }, cancellationToken), cancellationToken);

    internal async Task<EventStreamSnapshot<T>> CreateAsync<T>(T state, EventStreamVersion version, Func<SnapshotDocument, string?, Task> callback, CancellationToken cancellationToken)
    {
        var id = CosmosEventStreamSnapshot<T>.CreateSnapshotId(_streamId.Value);
        var snapshot = await ReadAsync<T>(id, cancellationToken).ConfigureAwait(false);
        var document = new SnapshotDocument(_streamId.Value, id.Value, version.Value, JsonData.From(state, _serializerOptions));

        await callback(document, snapshot?.ETag).ConfigureAwait(false);

        var reader = new CosmosEventStreamReader(_container, _partitionKey, version);

        return new CosmosEventStreamSnapshot<T>(_streamId, reader, state);
    }

    private async Task<ItemResponse<SnapshotDocument>?> ReadAsync<T>(EventStreamId id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _container.ReadItemAsync<SnapshotDocument>(id.Value, _partitionKey, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}

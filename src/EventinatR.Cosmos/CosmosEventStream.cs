using System.Runtime.CompilerServices;

namespace EventinatR.Cosmos;

internal class CosmosEventStream : EventStream
{
    private readonly Container _container;
    private readonly PartitionKey _partitionKey;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly CosmosEventStreamReader _reader;
    private readonly CosmosEventStreamSnapshotStore _snapshots;

    public CosmosEventStream(Container container, PartitionKey partitionKey, EventStreamId id, JsonSerializerOptions serializerOptions)
        : base(id)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
        _partitionKey = partitionKey;
        _serializerOptions = serializerOptions ?? throw new ArgumentNullException(nameof(serializerOptions));
        _reader = new CosmosEventStreamReader(_container, _partitionKey);
        _snapshots = new CosmosEventStreamSnapshotStore(Id, _container, _partitionKey, _serializerOptions);
    }

    public override EventStreamSnapshotStore Snapshots => _snapshots;

    public override async Task<EventStreamVersion> GetVersionAsync(CancellationToken cancellationToken = default)
    {
        var document = await GetCosmosEventStreamResourceAsync(cancellationToken).ConfigureAwait(false);

        if (document is null)
        {
            return EventStreamVersion.None;
        }

        return new CosmosEventStreamVersion(document.Resource.Version, document.ETag);
    }

    public override IAsyncEnumerable<Event> ReadAsync(CancellationToken cancellationToken = default)
        => _reader.ReadAsync(cancellationToken);

    protected override async Task AppendAsync<TState>(TransactionContext context, TState state, CancellationToken cancellationToken = default)
    {
        var eTag = (context.CurrentVersion as CosmosEventStreamVersion)?.ETag;

        var batch = _container.CreateTransactionalBatch(_partitionKey);
        
        var options = new TransactionalBatchItemRequestOptions
        {
            EnableContentResponseOnWrite = false
        };

        foreach (var e in context.Events)
        {
            var id = $"{Id.Value}:{e.Version}";
            var eventResource = new EventDocument(Id.Value, id, e.Version, e.Transaction, e.Timestamp, e.Data);
            batch.CreateItem(eventResource, options);
        }

        batch.UpsertItem(new StreamDocument(Id.Value, Id.Value, context.Transaction.Version), new TransactionalBatchItemRequestOptions
        {
            IfMatchEtag = eTag,
            EnableContentResponseOnWrite = false
        });

        if (state is not null)
        {
            await _snapshots.CreateAsync(state, context.Transaction.Version, (document, eTag) =>
            {
                batch.UpsertItem(document, new TransactionalBatchItemRequestOptions
                {
                    IfMatchEtag = eTag,
                    EnableContentResponseOnWrite = false
                });

                return Task.CompletedTask;
            }, cancellationToken).ConfigureAwait(false);
        }

        using var response = await batch.ExecuteAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new CosmosEventStreamAppendException(response);
        }
    }

    private async Task<ItemResponse<StreamDocument>?> GetCosmosEventStreamResourceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _container.ReadItemAsync<StreamDocument>(Id.Value, _partitionKey, cancellationToken: cancellationToken);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}

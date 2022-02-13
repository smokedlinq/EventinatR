using System.Runtime.CompilerServices;

namespace EventinatR.Cosmos;

internal class CosmosEventStream : EventStream
{
    private readonly CosmosEventStreamSnapshotStore _snapshots;

    public CosmosEventStream(Container container, PartitionKey partitionKey, EventStreamId id, JsonSerializerOptions serializerOptions)
        : base(id)
    {
        Container = container ?? throw new ArgumentNullException(nameof(container));
        PartitionKey = partitionKey;
        SerializerOptions = serializerOptions ?? throw new ArgumentNullException(nameof(serializerOptions));
        _snapshots = new CosmosEventStreamSnapshotStore(this);
    }

    internal Container Container { get; }
    internal PartitionKey PartitionKey { get; }
    internal JsonSerializerOptions SerializerOptions { get; }

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
        => ReadFromVersionAsync(default, cancellationToken);

    protected override async Task AppendAsync<TState>(TransactionContext context, TState state, CancellationToken cancellationToken = default)
    {
        var eTag = (context.CurrentVersion as CosmosEventStreamVersion)?.ETag;

        var batch = Container.CreateTransactionalBatch(PartitionKey);
        
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

    internal async IAsyncEnumerable<Event> ReadFromVersionAsync(long version, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.type = @type AND c.version > @version ORDER BY c.version")
            .WithParameter("@type", DocumentTypes.Event)
            .WithParameter("@version", version);

        var options = new QueryRequestOptions
        {
            PartitionKey = PartitionKey
        };

        var iterator = Container.GetItemQueryIterator<EventDocument>(query, null, options);

        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);

            foreach (var doc in page)
            {
                yield return doc.ToEvent();
            }
        }
    }

    private async Task<ItemResponse<StreamDocument>?> GetCosmosEventStreamResourceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await Container.ReadItemAsync<StreamDocument>(Id.Value, PartitionKey, cancellationToken: cancellationToken);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}

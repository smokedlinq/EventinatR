using System.Runtime.CompilerServices;

namespace EventinatR.Cosmos;

internal class CosmosEventStream : EventStream
{
    private readonly Container _container;
    private readonly PartitionKey _partitionKey;
    private readonly JsonSerializerOptions _serializerOptions;

    public CosmosEventStream(Container container, PartitionKey partitionKey, EventStreamId id, JsonSerializerOptions serializerOptions)
        : base(id)
    {
        _container = container ?? throw new ArgumentNullException(nameof(container));
        _partitionKey = partitionKey;
        _serializerOptions = serializerOptions ?? throw new ArgumentNullException(nameof(serializerOptions));
    }

    internal virtual async IAsyncEnumerable<Event> ReadFromVersionAsync(long version, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.type = @type AND c.version > @version ORDER BY c.version")
            .WithParameter("@type", DocumentTypes.Event)
            .WithParameter("@version", version);

        var options = new QueryRequestOptions
        {
            PartitionKey = _partitionKey
        };

        var iterator = _container.GetItemQueryIterator<EventDocument>(query, null, options);

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
            return await _container.ReadItemAsync<StreamDocument>(Id.Value, _partitionKey, cancellationToken: cancellationToken);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public override async Task<EventStreamVersion> GetVersionAsync(CancellationToken cancellationToken = default)
    {
        var document = await GetCosmosEventStreamResourceAsync(cancellationToken).ConfigureAwait(false);

        if (document is null)
        {
            return EventStreamVersion.None;
        }

        return document.Resource.Version;
    }

    public override IAsyncEnumerable<Event> ReadAsync(CancellationToken cancellationToken = default)
        => ReadFromVersionAsync(default, cancellationToken);

    private async Task<ItemResponse<SnapshotDocument>?> ReadSnapshotAsync<T>(EventStreamId id, CancellationToken cancellationToken = default)
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

    public override async Task<EventStreamSnapshot<T>> ReadSnapshotAsync<T>(CancellationToken cancellationToken = default)
    {
        var id = CosmosEventStreamSnapshot<T>.CreateSnapshotId(Id.Value);
        var snapshot = await ReadSnapshotAsync<T>(id, cancellationToken).ConfigureAwait(false);

        return snapshot is null
            ? new CosmosEventStreamSnapshot<T>(this)
            : new CosmosEventStreamSnapshot<T>(this, new EventStreamVersion(snapshot.Resource.Version), snapshot.Resource.State.As<T>());
    }

    public override Task<EventStreamSnapshot<T>> WriteSnapshotAsync<T>(T state, EventStreamVersion version, CancellationToken cancellationToken = default)
        => CreateSnapshotAsync(state, version, (document, eTag) =>
            _container.UpsertItemAsync(document, _partitionKey, new ItemRequestOptions
            {
                EnableContentResponseOnWrite = false,
                IfMatchEtag = eTag
            }, cancellationToken), cancellationToken);

    private async Task<EventStreamSnapshot<T>> CreateSnapshotAsync<T>(T state, EventStreamVersion version, Func<SnapshotDocument, string?, Task> callback, CancellationToken cancellationToken)
    {
        var id = CosmosEventStreamSnapshot<T>.CreateSnapshotId(Id.Value);
        var snapshot = await ReadSnapshotAsync<T>(id, cancellationToken).ConfigureAwait(false);
        var document = new SnapshotDocument(Id.Value, id.Value, version.Value, JsonData.From(state, _serializerOptions));

        await callback(document, snapshot?.ETag).ConfigureAwait(false);

        return new CosmosEventStreamSnapshot<T>(this, version, state);
    }

    public override Task<EventStreamVersion> AppendAsync<TEvent, TState>(IEnumerable<TEvent> collection, TState state, CancellationToken cancellationToken = default)
    {
        if (collection.Any(x => x is null))
        {
            throw new ArgumentException("The collection must not contain a null item.", nameof(collection));
        }

        return AppendAsync();

        async Task<EventStreamVersion> AppendAsync()
        {
            var stream = await GetCosmosEventStreamResourceAsync(cancellationToken).ConfigureAwait(false);
            var batch = _container.CreateTransactionalBatch(_partitionKey);
            var version = stream?.Resource?.Version ?? default;

            if (!collection.Any())
            {
                return new EventStreamVersion(version);
            }

            var options = new TransactionalBatchItemRequestOptions
            {
                EnableContentResponseOnWrite = false
            };

            foreach (var item in collection)
            {
                version++;
                var id = $"{Id.Value}:{version}";
                var data = JsonData.From(item, _serializerOptions);
                var eventResource = new EventDocument(Id.Value, id, version, DateTimeOffset.Now, data);
                batch.CreateItem(eventResource, options);
            }

            batch.UpsertItem(new StreamDocument(Id.Value, Id.Value, version), new TransactionalBatchItemRequestOptions
            {
                IfMatchEtag = stream?.ETag,
                EnableContentResponseOnWrite = false
            });

            if (state is not null)
            {
                await CreateSnapshotAsync(state, version, (document, eTag) =>
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

            return new EventStreamVersion(version);
        }
    }
}

using System.Runtime.CompilerServices;

namespace EventinatR.Cosmos
{
    internal class CosmosEventStream : EventStream
    {
        private readonly Container _container;
        private readonly PartitionKey _partitionKey;
        private readonly JsonSerializerOptions _serializerOptions;

        public CosmosEventStream(Container container, EventStreamId id, JsonSerializerOptions serializerOptions)
            : base(id)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _serializerOptions = serializerOptions ?? throw new ArgumentNullException(nameof(serializerOptions));
            _partitionKey = new PartitionKey(Id.Value);
        }

        public virtual async IAsyncEnumerable<Event> ReadFromVersionAsync(long version, [EnumeratorCancellation] CancellationToken cancellationToken = default)
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

        public override IAsyncEnumerable<Event> ReadAsync(CancellationToken cancellationToken = default)
            => ReadFromVersionAsync(default, cancellationToken);

        protected async Task<ItemResponse<SnapshotDocument>?> ReadSnapshotAsync<T>(EventStreamId id, CancellationToken cancellationToken = default)
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
            => WriteSnapshotAsync<T>((document, eTag) =>
            {
                if (string.IsNullOrEmpty(eTag))
                {
                    return _container.CreateItemAsync(document, _partitionKey, cancellationToken: cancellationToken);
                }
                else
                {
                    var options = new ItemRequestOptions
                    {
                        IfMatchEtag = eTag
                    };

                    return _container.ReplaceItemAsync(document, document.Id, _partitionKey, options, cancellationToken);
                }
            }, state, version, cancellationToken);

        protected async Task<EventStreamSnapshot<T>> WriteSnapshotAsync<T>(Func<SnapshotDocument, string?, Task> callback, T state, EventStreamVersion version, CancellationToken cancellationToken)
        {
            var id = CosmosEventStreamSnapshot<T>.CreateSnapshotId(Id.Value);
            var snapshot = await ReadSnapshotAsync<T>(id, cancellationToken).ConfigureAwait(false);
            var document = new SnapshotDocument(Id.Value, id.Value, version.Value, JsonData.From(state, _serializerOptions));

            if (snapshot is null)
            {
                await callback(document, null).ConfigureAwait(false);
            }
            else
            {
                await callback(document, snapshot.ETag).ConfigureAwait(false);
            }

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

                foreach (var item in collection)
                {
                    if (item is not null)
                    {
                        version++;
                        var id = $"{Id.Value}:{version}";
                        var data = JsonData.From(item, _serializerOptions);
                        var eventResource = new EventDocument(Id.Value, id, version, DateTimeOffset.Now, data);
                        batch = batch.CreateItem(eventResource);
                    }
                }

                var streamResource = new StreamDocument(Id.Value, Id.Value, version);

                if (stream?.Resource is null)
                {
                    batch = batch.CreateItem(streamResource);
                }
                else
                {
                    batch = batch.ReplaceItem(streamResource.Id, streamResource, new TransactionalBatchItemRequestOptions
                    {
                        IfMatchEtag = stream.ETag
                    });
                }

                if (state is not null)
                {
                    await WriteSnapshotAsync((document, eTag) =>
                    {
                        if (string.IsNullOrEmpty(eTag))
                        {
                            batch.CreateItem(document);
                        }
                        else
                        {
                            var options = new TransactionalBatchItemRequestOptions
                            {
                                IfMatchEtag = eTag
                            };

                            batch.ReplaceItem(document.Id, document, options);
                        }

                        return Task.CompletedTask;
                    }, state, version, cancellationToken).ConfigureAwait(false);
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
}

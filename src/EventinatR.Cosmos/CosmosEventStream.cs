using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EventinatR.Cosmos.Documents;
using Microsoft.Azure.Cosmos;

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
                    yield return new Event(Id, doc.Version, doc.Timestamp, doc.DataType, new BinaryData(doc.Data.ToString()));
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

        protected async Task<ItemResponse<SnapshotDocument>?> ReadSnapshotAsync<T>(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _container.ReadItemAsync<SnapshotDocument>(id, _partitionKey, cancellationToken: cancellationToken).ConfigureAwait(false);
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
                : new CosmosEventStreamSnapshot<T>(this, new EventStreamVersion(snapshot.Resource.Version), snapshot.Resource.State.ToObjectFromJson<T>(_serializerOptions));
        }

        public override async Task<EventStreamSnapshot<T>> WriteSnapshotAsync<T>(T state, EventStreamVersion version, CancellationToken cancellationToken = default)
        {
            var id = CosmosEventStreamSnapshot<T>.CreateSnapshotId(Id.Value);
            var snapshot = await ReadSnapshotAsync<T>(id, cancellationToken).ConfigureAwait(false);
            var resource = new SnapshotDocument(Id.Value, id, version.Value, typeof(T).FullName!, BinaryData.FromObjectAsJson(state, _serializerOptions));

            if (snapshot is null)
            {
                _ = await _container.CreateItemAsync(resource, _partitionKey, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var options = new ItemRequestOptions
                {
                    IfMatchEtag = snapshot.ETag
                };

                _ = await _container.ReplaceItemAsync(resource, id, _partitionKey, options, cancellationToken).ConfigureAwait(false);
            }

            return new CosmosEventStreamSnapshot<T>(this, version, state);
        }

        public override async Task<EventStreamVersion> AppendAsync<T>(IEnumerable<T> collection, CancellationToken cancellationToken = default)
            where T : class
        {
            var stream = await GetCosmosEventStreamResourceAsync(cancellationToken).ConfigureAwait(false);
            var batch = _container.CreateTransactionalBatch(_partitionKey);
            var version = stream?.Resource?.Version ?? default;

            foreach (var item in collection)
            {
                if (item is not null)
                {
                    version++;
                    var id = $"{Id.Value}:{version}";
                    var dataType = item.GetType().FullName ?? item.GetType().Name;
                    var json = JsonSerializer.SerializeToUtf8Bytes(item, item.GetType(), _serializerOptions);
                    var data = new BinaryData(json);
                    var eventResource = new EventDocument(Id.Value, id, version, DateTimeOffset.Now, dataType, data);
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

            using var response = await batch.ExecuteAsync(cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new CosmosEventStreamAppendException(response);
            }

            return new EventStreamVersion(version);
        }
    }
}

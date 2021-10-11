using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EventinatR.CosmosDB.Documents;
using Microsoft.Azure.Cosmos;

namespace EventinatR.CosmosDB
{
    internal class CosmosEventStream : EventStream
    {
        private readonly Container _container;
        private readonly PartitionKey _partitionKey;

        public CosmosEventStream(Container container, string id)
            : base(id.ToLowerInvariant())
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _partitionKey = new PartitionKey(Id!);
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
                    yield return new Event(doc.Version, doc.Timestamp, doc.DataType, new BinaryData(doc.Data.ToString()));
                }
            }
        }

        private async Task<ItemResponse<StreamDocument>?> GetCosmosEventStreamResourceAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _container.ReadItemAsync<StreamDocument>(Id, _partitionKey, cancellationToken: cancellationToken);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public override IAsyncEnumerable<Event> ReadAsync(CancellationToken cancellationToken = default)
            => ReadFromVersionAsync(default, cancellationToken);

        public override async Task<EventStreamSnapshot<T>> ReadSnapshotAsync<T>(CancellationToken cancellationToken = default)
        {
            var id = CosmosEventStreamSnapshot<T>.CreateSnapshotId(Id);

            try
            {
                var snapshot = await _container.ReadItemAsync<SnapshotDocument<T>>(id, _partitionKey, cancellationToken: cancellationToken).ConfigureAwait(false);
                return new CosmosEventStreamSnapshot<T>(this, new EventStreamVersion(snapshot.Resource.Version), snapshot.Resource.State);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return new CosmosEventStreamSnapshot<T>(this);
            }
        }

        public override async Task<EventStreamSnapshot<T>> WriteSnapshotAsync<T>(T state, EventStreamVersion version, CancellationToken cancellationToken = default)
        {
            var id = CosmosEventStreamSnapshot<T>.CreateSnapshotId(Id);
            var resource = new SnapshotDocument<T>(Id!, id, version.Value, typeof(T).FullName!, state);
            var options = new ItemRequestOptions
            {
                IfMatchEtag = "*"
            };

            _ = await _container.UpsertItemAsync(resource, _partitionKey, options, cancellationToken).ConfigureAwait(false);

            return new CosmosEventStreamSnapshot<T>(this, version, state);
        }

        public override async Task<EventStreamVersion> AppendAsync<T>(IEnumerable<T> collection, CancellationToken cancellationToken = default)
            where T : class
        {
            var stream = await GetCosmosEventStreamResourceAsync(cancellationToken).ConfigureAwait(false);
            var batch = _container.CreateTransactionalBatch(_partitionKey);
            var version = stream?.Resource?.Version ?? default;

            foreach (var data in collection)
            {
                version++;
                var id = $"{Id}:{version}";
                var eventResource = new ObjectEventDocument(Id!, id, version, DateTimeOffset.Now, data.GetType().FullName!, data);
                batch = batch.CreateItem(eventResource);
            }

            var streamResource = new StreamDocument(Id!, Id!, version);

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

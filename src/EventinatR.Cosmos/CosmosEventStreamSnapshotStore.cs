using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventinatR.Cosmos;

internal class CosmosEventStreamSnapshotStore : EventStreamSnapshotStore
{
    private readonly CosmosEventStream _stream;

    public CosmosEventStreamSnapshotStore(CosmosEventStream stream)
        => _stream = stream ?? throw new ArgumentNullException(nameof(stream));

    public override async Task<EventStreamSnapshot<T>> GetAsync<T>(CancellationToken cancellationToken = default)
    {
        var id = CosmosEventStreamSnapshot<T>.CreateSnapshotId(_stream.Id.Value);
        var snapshot = await ReadSnapshotAsync<T>(id, cancellationToken).ConfigureAwait(false);

        return snapshot is null
            ? new CosmosEventStreamSnapshot<T>(_stream)
            : new CosmosEventStreamSnapshot<T>(_stream, snapshot.Resource.Version, snapshot.Resource.State.As<T>());
    }

    public override Task<EventStreamSnapshot<T>> SaveAsync<T>(T state, EventStreamVersion version, CancellationToken cancellationToken = default)
        => CreateAsync(state, version, (document, eTag) =>
            _stream.Container.UpsertItemAsync(document, _stream.PartitionKey, new ItemRequestOptions
            {
                EnableContentResponseOnWrite = false,
                IfMatchEtag = eTag
            }, cancellationToken), cancellationToken);

    internal async Task<EventStreamSnapshot<T>> CreateAsync<T>(T state, EventStreamVersion version, Func<SnapshotDocument, string?, Task> callback, CancellationToken cancellationToken)
    {
        var id = CosmosEventStreamSnapshot<T>.CreateSnapshotId(_stream.Id.Value);
        var snapshot = await ReadSnapshotAsync<T>(id, cancellationToken).ConfigureAwait(false);
        var document = new SnapshotDocument(_stream.Id.Value, id.Value, version.Value, JsonData.From(state, _stream.SerializerOptions));

        await callback(document, snapshot?.ETag).ConfigureAwait(false);

        return new CosmosEventStreamSnapshot<T>(_stream, version, state);
    }

    private async Task<ItemResponse<SnapshotDocument>?> ReadSnapshotAsync<T>(EventStreamId id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _stream.Container.ReadItemAsync<SnapshotDocument>(id.Value, _stream.PartitionKey, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}

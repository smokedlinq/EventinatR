using Microsoft.Extensions.Options;

namespace EventinatR.Cosmos;

public class CosmosEventStore : EventStore, IAsyncDisposable
{
    private readonly CosmosEventStoreClient _client;
    private readonly CosmosEventStoreOptions _options;

    public CosmosEventStore(CosmosEventStoreClient client, IOptions<CosmosEventStoreOptions> options)
        : this(client, options.Value)
    {
    }

    public CosmosEventStore(CosmosEventStoreClient client, CosmosEventStoreOptions options)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public override async Task<EventStream> GetStreamAsync(EventStreamId id, CancellationToken cancellationToken = default)
    {
        var container = await _client.GetContainerAsync(cancellationToken).ConfigureAwait(false);
        return new CosmosEventStream(container, _options.PartitionStrategy.GetPartitionKey(id), id, _options.SerializerOptions);
    }

    public async ValueTask DisposeAsync()
    {
        await _client.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
}

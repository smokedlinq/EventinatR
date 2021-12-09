using Azure.Core;
using Microsoft.Extensions.Options;

namespace EventinatR.Cosmos;

public class CosmosEventStore : EventStore, IAsyncDisposable
{
    private readonly AsyncLazy<CosmosClient> _client;
    private readonly AsyncLazy<Container> _container;

    public CosmosEventStoreOptions Options { get; }

    public CosmosEventStore(TokenCredential? credential, IOptions<CosmosEventStoreOptions> options)
        : this(credential, options.Value)
    {
    }

    public CosmosEventStore(TokenCredential? credential, CosmosEventStoreOptions options)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));

        _client = new AsyncLazy<CosmosClient>(() => Options.CreateAndInitializeCosmosClientAsync(credential));
        _container = new AsyncLazy<Container>(async () =>
        {
            var client = await _client.ConfigureAwait(false);
            return client.GetContainer(Options.DatabaseId, Options.Container.Id);
        });
    }

    public override async Task<EventStream> GetStreamAsync(EventStreamId id, CancellationToken cancellationToken = default)
    {
        var container = await _container.ConfigureAwait(false);
        return new CosmosEventStream(container, Options.PartitionStrategy.GetPartitionKey(id), id, Options.SerializerOptions);
    }

    public async ValueTask DisposeAsync()
    {
        if (_client.IsValueCreated)
        {
            var client = await _client.ConfigureAwait(false);
            client.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}

using Azure.Core;
using Microsoft.Extensions.Options;

namespace EventinatR.Cosmos;

public class CosmosEventStoreClient : IAsyncDisposable
{
    private readonly CosmosEventStoreOptions _options;
    private readonly AsyncLazy<CosmosClient> _client;
    private readonly AsyncLazy<Container> _container;

    protected CosmosEventStoreClient()
        : this(null, Options.Create(new CosmosEventStoreOptions()))
    {
    }

    public CosmosEventStoreClient(TokenCredential? credential, IOptions<CosmosEventStoreOptions> options)
        : this(credential, options.Value)
    {
    }

    public CosmosEventStoreClient(TokenCredential? credential, CosmosEventStoreOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _client = new AsyncLazy<CosmosClient>(() => CreateAndInitializeCosmosClientAsync(credential));
        _container = new AsyncLazy<Container>(async () =>
        {
            var client = await _client.ConfigureAwait(false);
            return client.GetContainer(_options.DatabaseId, _options.Container.Id);
        });
    }

    public virtual async Task<Container> GetContainerAsync(CancellationToken cancellationToken = default)
        => await _container.ConfigureAwait(false);

    public virtual async ValueTask DisposeAsync()
    {
        if (_client.IsValueCreated)
        {
            var client = await _client.ConfigureAwait(false);
            client.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    private CosmosClient CreateCosmosClient(TokenCredential? credential = null)
    {
        CosmosClient client;

        if (!string.IsNullOrEmpty(_options.ConnectionString))
        {
            client = new CosmosClient(_options.ConnectionString, _options.CosmosClientOptions);
        }
        else if (string.IsNullOrEmpty(_options.AccountEndpoint))
        {
            throw new InvalidOperationException("The AccountEndpoint property must be set if ConnectionString is not provided.");
        }
        else if (string.IsNullOrEmpty(_options.AuthKeyOrTokenResource) && credential is null)
        {
            throw new ArgumentNullException(nameof(credential), "ConnectionString or AuthKeyOrTokenResource must be provided if TokenCredential is null.");
        }
        else if (credential is null)
        {
            client = new CosmosClient(_options.AccountEndpoint, _options.AuthKeyOrTokenResource, _options.CosmosClientOptions);
        }
        else
        {
            client = new CosmosClient(_options.AccountEndpoint, credential, _options.CosmosClientOptions);
        }

        return client;
    }

    private async Task<CosmosClient> CreateAndInitializeCosmosClientAsync(TokenCredential? credential = null, CancellationToken cancellationToken = default)
    {
        var client = CreateCosmosClient(credential);

        _ = await client.CreateDatabaseIfNotExistsAsync(_options.DatabaseId, _options.Throughput, cancellationToken: cancellationToken).ConfigureAwait(false);

        var db = client.GetDatabase(_options.DatabaseId);

        _ = await db.CreateContainerIfNotExistsAsync(_options.Container.Id, "/stream", _options.Container.Throughput, cancellationToken: cancellationToken).ConfigureAwait(false);

        return client;
    }
}

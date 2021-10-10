using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.Azure.Cosmos;

namespace EventinatR.CosmosDB
{
    public class CosmosEventStoreOptions : CosmosClientOptions
    {
        public CosmosEventStoreOptions()
            => SerializerOptions = new CosmosSerializationOptions
            {
                IgnoreNullValues = true,
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            };

        public string? AccountEndpoint { get; set; }
        public string? AuthKeyOrTokenResource { get; set; }
        public string? ConnectionString { get; set; }
        public string DatabaseId { get; set; } = "event-store";
        public int? Throughput { get; set; }
        public CosmosEventStoreContainerOptions Container { get; } = new("events");

        public CosmosClient CreateCosmosClient(TokenCredential? credential = null)
        {
            CosmosClient client;

            if (!string.IsNullOrEmpty(ConnectionString))
            {
                client = new CosmosClient(ConnectionString, this);
            }
            else if (string.IsNullOrEmpty(AccountEndpoint))
            {
                throw new InvalidOperationException("The AccountEndpoint property must be set if ConnectionString is not provided.");
            }
            else if (string.IsNullOrEmpty(AuthKeyOrTokenResource) && credential is null)
            {
                throw new ArgumentNullException(nameof(credential), "ConnectionString or AuthKeyOrTokenResource must be provided if TokenCredential is null.");
            }
            else if (credential is null)
            {
                client = new CosmosClient(AccountEndpoint, AuthKeyOrTokenResource, this);
            }
            else
            {
                client = new CosmosClient(AccountEndpoint, credential, this);
            }

            return client;
        }

        public async Task<CosmosClient> CreateAndInitializeCosmosClientAsync(TokenCredential? credential = null, CancellationToken cancellationToken = default)
        {
            var client = CreateCosmosClient(credential);

            _ = await client.CreateDatabaseIfNotExistsAsync(DatabaseId, Throughput, cancellationToken: cancellationToken).ConfigureAwait(false);

            var db = client.GetDatabase(DatabaseId);

            _ = await db.CreateContainerIfNotExistsAsync(Container.Id, "/streamId", Container.Throughput, cancellationToken: cancellationToken).ConfigureAwait(false);

            return client;
        }
    }
}

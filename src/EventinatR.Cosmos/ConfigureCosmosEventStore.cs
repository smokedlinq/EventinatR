using Azure.Core;
using EventinatR;
using EventinatR.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureCosmosEventStore
{
    public static IServiceCollection AddCosmosEventStore(this IServiceCollection services, TokenCredential? credential = null)
        => AddCosmosEventStore(services, credential, _ => { });

    public static IServiceCollection AddCosmosEventStore(this IServiceCollection services, TokenCredential? credential, Action<CosmosEventStoreOptions> configure)
         => AddCosmosEventStore(services, credential, (_, options) => configure(options));

    public static IServiceCollection AddCosmosEventStore(this IServiceCollection services, Action<CosmosEventStoreOptions> configure)
         => AddCosmosEventStore(services, null, (_, options) => configure(options));

    public static IServiceCollection AddCosmosEventStore(this IServiceCollection services, TokenCredential? credential, Action<IServiceProvider, CosmosEventStoreOptions> configure)
        => services.AddOptions<CosmosEventStoreOptions>()
            .Configure<IConfiguration>((options, configuration) => configuration.GetSection(nameof(CosmosEventStore)).Bind(options))
            .Services
            .AddSingleton<EventStore>(serviceProvider =>
            {
                var options = serviceProvider.GetService<IOptions<CosmosEventStoreOptions>>()?.Value ?? new CosmosEventStoreOptions();

                configure(serviceProvider, options);

                return new CosmosEventStore(credential, options);
            });
}

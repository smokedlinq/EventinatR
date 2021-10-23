using Azure.Core;
using EventinatR;
using EventinatR.Cosmos;
using EventinatR.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureCosmosEventStore
{
    public static IEventStoreBuilder UseCosmosEventStore(this IEventStoreBuilder builder, TokenCredential? credential = null)
        => UseCosmosEventStore(builder, credential, _ => { });

    public static IEventStoreBuilder UseCosmosEventStore(this IEventStoreBuilder builder, TokenCredential? credential, Action<CosmosEventStoreOptions> configure)
         => UseCosmosEventStore(builder, credential, (_, options) => configure(options));

    public static IEventStoreBuilder UseCosmosEventStore(this IEventStoreBuilder builder, Action<CosmosEventStoreOptions> configure)
         => UseCosmosEventStore(builder, null, (_, options) => configure(options));

    public static IEventStoreBuilder UseCosmosEventStore(this IEventStoreBuilder builder, TokenCredential? credential, Action<IServiceProvider, CosmosEventStoreOptions> configure)
    {
        builder.Services.AddOptions<CosmosEventStoreOptions>()
                   .Configure<IConfiguration>((options, configuration) => configuration.GetSection(nameof(CosmosEventStore)).Bind(options))
                   .Services
                   .AddSingleton<EventStore>(serviceProvider =>
                   {
                       var options = serviceProvider.GetService<IOptions<CosmosEventStoreOptions>>()?.Value ?? new CosmosEventStoreOptions();

                       configure(serviceProvider, options);

                       return new CosmosEventStore(credential, options);
                   });

        return builder;
    }
}

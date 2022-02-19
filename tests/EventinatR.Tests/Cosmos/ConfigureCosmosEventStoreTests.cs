using EventinatR.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventinatR.Tests.Cosmos;

public class ConfigureCosmosEventStoreTests
{
    [Fact]
    public void UseCosmosEventStore_ShoudlResolveFromServiceProvider_WhenRegisteredWithEventStoreBuilder()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddEventinatR()
            .UseCosmosEventStore();

        var serviceProvider = services.BuildServiceProvider();

        var store = serviceProvider.GetRequiredService<EventStore>();

        store.Should().BeOfType<CosmosEventStore>();
    }
}

using EventinatR.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureEventStore
{
    public static IEventStoreBuilder AddEventinatR(this IServiceCollection services)
        => new EventStoreBuilder(services);
}

using EventinatR;
using EventinatR.InMemory;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureInMemoryEventStore
{
    public static IServiceCollection AddInMemoryEventStore(this IServiceCollection services)
        => services.AddSingleton<EventStore>(_ => new InMemoryEventStore());
}

using Microsoft.Extensions.DependencyInjection;

namespace EventinatR.DependencyInjection;

internal class EventStoreBuilder : IEventStoreBuilder
{
    public EventStoreBuilder(IServiceCollection services)
        => Services = services;

    public IServiceCollection Services { get; }
}

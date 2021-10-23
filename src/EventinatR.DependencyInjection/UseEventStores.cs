using EventinatR;
using EventinatR.DependencyInjection;
using EventinatR.InMemory;

namespace Microsoft.Extensions.DependencyInjection;

public static class UseEventStores
{
    public static IEventStoreBuilder UseEventStore<T>(this IEventStoreBuilder builder)
        where T : EventStore
    {
        builder.Services.AddSingleton<EventStore, T>();
        return builder;
    }

    public static IEventStoreBuilder UseInMemoryEventStore(this IEventStoreBuilder builder)
        => builder.UseEventStore<InMemoryEventStore>();
}

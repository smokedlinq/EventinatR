using Microsoft.Extensions.DependencyInjection;

namespace EventinatR.Serialization;

public static class ConfigureEventConverter
{
    public static IServiceCollection AddEventConverter(this IServiceCollection services, Action<IEventConverterBuilder> configure)
        => AddEventConverter(services, (_, builder) => configure(builder));

    public static IServiceCollection AddEventConverter(this IServiceCollection services, Action<IServiceProvider, IEventConverterBuilder> configure)
    {
        services.AddSingleton(serviceProvider => new EventConverter(builder => configure(serviceProvider, builder)));
        return services;
    }
}


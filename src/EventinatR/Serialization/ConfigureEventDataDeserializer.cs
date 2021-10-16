using System;
using Microsoft.Extensions.DependencyInjection;

namespace EventinatR.Serialization
{
    public static class ConfigureEventDataDeserializer
    {
        public static IServiceCollection AddEventDataDeserializer(this IServiceCollection services, Action<IEventDataDeserializerBuilder> configure)
            => AddEventDataDeserializer(services, (_, builder) => configure(builder));

        public static IServiceCollection AddEventDataDeserializer(this IServiceCollection services, Action<IServiceProvider, IEventDataDeserializerBuilder> configure)
        {
            services.AddSingleton(serviceProvider => new EventDataDeserializer(builder => configure(serviceProvider, builder)));
            return services;
        }
    }
}


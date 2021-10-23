using Microsoft.Extensions.DependencyInjection;

namespace EventinatR.DependencyInjection;

public interface IEventStoreBuilder
{
    IServiceCollection Services { get; }
}

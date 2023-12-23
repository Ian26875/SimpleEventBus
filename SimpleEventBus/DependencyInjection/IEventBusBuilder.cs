using Microsoft.Extensions.DependencyInjection;

namespace SimpleEventBus.DependencyInjection;

public interface IEventBusBuilder
{
    IServiceCollection Services { get; }
}
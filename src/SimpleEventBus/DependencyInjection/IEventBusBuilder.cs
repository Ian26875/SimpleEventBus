using Microsoft.Extensions.DependencyInjection;

namespace SimpleEventBus.DependencyInjection;

public interface IEventBusBuilder
{
    IServiceCollection Services { get; }
}

public class EventBusBuilder : IEventBusBuilder
{
    public EventBusBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IServiceCollection Services { get; }
    
}
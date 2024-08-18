using Microsoft.Extensions.DependencyInjection;

namespace SimpleEventBus;

public class DefaultEventHandlerResolver : IEventHandlerResolver
{
    private readonly IServiceProvider _serviceProvider;

    public DefaultEventHandlerResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IEnumerable<IEventHandler<TEvent>> GetHandlersForEvent<TEvent>(TEvent @event) where TEvent : class
    {
        using var serviceScope = _serviceProvider.CreateScope();
        
        var handlers = serviceScope.ServiceProvider.GetServices<IEventHandler<TEvent>>();
        
        return handlers;
    }
}
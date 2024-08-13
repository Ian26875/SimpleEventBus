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
        var handlers = _serviceProvider.GetServices<IEventHandler<TEvent>>();
        return handlers;
    }
}
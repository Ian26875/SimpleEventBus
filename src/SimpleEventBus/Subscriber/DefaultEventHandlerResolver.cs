using Microsoft.Extensions.DependencyInjection;
using SimpleEventBus.Subscriber;

namespace SimpleEventBus;

/// <summary>
/// The default event handler resolver class
/// </summary>
/// <seealso cref="IEventHandlerResolver"/>
public class DefaultEventHandlerResolver : IEventHandlerResolver
{
    /// <summary>
    /// The service provider
    /// </summary>
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public DefaultEventHandlerResolver(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// Gets the handlers for event using the specified event
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="event">The event</param>
    /// <returns>The handlers</returns>
    public IEnumerable<IEventHandler<TEvent>> GetHandlersForEvent<TEvent>(TEvent @event) where TEvent : class
    {
        using var serviceScope = _serviceScopeFactory.CreateAsyncScope();
        
        var handlers = serviceScope.ServiceProvider.GetServices<IEventHandler<TEvent>>();
        
        return handlers;
    }
}
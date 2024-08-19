using Microsoft.Extensions.DependencyInjection;

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
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultEventHandlerResolver"/> class
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    public DefaultEventHandlerResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets the handlers for event using the specified event
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="@event">The event</param>
    /// <returns>The handlers</returns>
    public IEnumerable<IEventHandler<TEvent>> GetHandlersForEvent<TEvent>(TEvent @event) where TEvent : class
    {
        using var serviceScope = _serviceProvider.CreateScope();
        
        var handlers = serviceScope.ServiceProvider.GetServices<IEventHandler<TEvent>>();
        
        return handlers;
    }
}
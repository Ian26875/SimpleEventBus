using Microsoft.Extensions.DependencyInjection;

namespace SimpleEventBus.ExceptionHandlers;

/// <summary>
/// The handler exception handler resolver interface
/// </summary>
public interface IEventExceptionHandlerResolver
{
    /// <summary>
    /// Gets the handler exception handlers
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <returns>An enumerable of i handler exception handler</returns>
    IEnumerable<IEventExceptionHandler> GetHandlerExceptionHandlers<TEvent>() where TEvent : class;
}

/// <summary>
/// The default handler exception handler resolver class
/// </summary>
/// <seealso cref="IEventExceptionHandlerResolver"/>
public class DefaultEventExceptionHandlerResolver : IEventExceptionHandlerResolver
{
    /// <summary>
    /// The service provider
    /// </summary>
    private readonly IServiceScopeFactory _serviceScopeFactory;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultEventExceptionHandlerResolver"/> class
    /// </summary>
    /// <param name="serviceScopeFactory">The service scope factory</param>
    public DefaultEventExceptionHandlerResolver(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }
    
    /// <summary>
    /// Gets the handler exception handlers
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <returns>The handlers</returns>
    public IEnumerable<IEventExceptionHandler> GetHandlerExceptionHandlers<TEvent>() where TEvent : class
    {
        using var serviceScope = _serviceScopeFactory.CreateAsyncScope();
        
        var handlers = serviceScope.ServiceProvider.GetServices<IEventExceptionHandler>();
        
        return handlers;
    }
}
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.DependencyInjection;
using SimpleEventBus.Profile;

namespace SimpleEventBus.ExceptionHandlers;

/// <summary>
/// The exception handler pipeline class
/// </summary>
/// <seealso cref="IExceptionHandlerPipeline"/>
public class ExceptionHandlerPipeline : IExceptionHandlerPipeline
{
    
    /// <summary>
    /// The subscription profile manager
    /// </summary>
    private readonly ISubscriptionProfileManager _subscriptionProfileManager;

    /// <summary>
    /// The service scope factory
    /// </summary>
    private readonly IServiceScopeFactory _serviceScopeFactory;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlerPipeline"/> class
    /// </summary>
    /// <param name="subscriptionProfileManager">The subscription profile manager</param>
    /// <param name="serviceScopeFactory">The service scope factory</param>
    public ExceptionHandlerPipeline(ISubscriptionProfileManager subscriptionProfileManager,
                                    IServiceScopeFactory serviceScopeFactory)
    {
        _subscriptionProfileManager = subscriptionProfileManager;
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// Executes the context
    /// </summary>
    /// <param name="context">The context</param>
    public void Execute(ExceptionContext context)
    {
        var errorHandlerTypes = _subscriptionProfileManager.GetErrorHandlersForEvent(context.Event.GetType());
        
        using var serviceScope = _serviceScopeFactory.CreateAsyncScope();

        var serviceProvider = serviceScope.ServiceProvider;
        
        foreach (var exceptionHandler in errorHandlerTypes.Select(errorHandlerType => (IEventExceptionHandler)serviceProvider.GetRequiredService(errorHandlerType)))
        {
            exceptionHandler.OnException(context);
        }
    }
}
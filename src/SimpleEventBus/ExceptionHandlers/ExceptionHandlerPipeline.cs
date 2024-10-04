using System.Runtime.ExceptionServices;
using Microsoft.Extensions.DependencyInjection;
using SimpleEventBus.Profile;

namespace SimpleEventBus.ExceptionHandlers;

public class ExceptionHandlerPipeline : IExceptionHandlerPipeline
{
    
    private readonly ISubscriptionProfileManager _subscriptionProfileManager;

    private readonly IServiceScopeFactory _serviceScopeFactory;
    
    public ExceptionHandlerPipeline(ISubscriptionProfileManager subscriptionProfileManager,
                                    IServiceScopeFactory serviceScopeFactory)
    {
        _subscriptionProfileManager = subscriptionProfileManager;
        _serviceScopeFactory = serviceScopeFactory;
    }

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
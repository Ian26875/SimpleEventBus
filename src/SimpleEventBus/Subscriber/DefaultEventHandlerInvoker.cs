using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleEventBus.Event;
using SimpleEventBus.ExceptionHandlers;
using SimpleEventBus.Profile;
using SimpleEventBus.Subscriber;
using SimpleEventBus.Subscriber.Executors;

namespace SimpleEventBus;

/// <summary>
///     The default event handler invoker class
/// </summary>
/// <seealso cref="IEventHandlerInvoker" />
internal class DefaultEventHandlerInvoker : IEventHandlerInvoker
{
    /// <summary>
    ///     The service provider
    /// </summary>
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<DefaultEventHandlerInvoker> _logger;
    private readonly ISubscriptionProfileManager _subscriptionProfileManager;
    
    private static readonly ConcurrentDictionary<(Type EventType, Type HandlerType), Func<object, object, Headers, CancellationToken, Task>> CachedHandlers 
        = new ConcurrentDictionary<(Type, Type), Func<object, object, Headers, CancellationToken, Task>>();
    
    public DefaultEventHandlerInvoker(IServiceScopeFactory serviceScopeFactory, 
                                      ILogger<DefaultEventHandlerInvoker> logger, 
                                      ISubscriptionProfileManager subscriptionProfileManager)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _subscriptionProfileManager = subscriptionProfileManager;
    }

    public async Task InvokeAsync(object @event, Headers headers, CancellationToken cancellationToken = default)
    {
        if (@event is null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        var eventType = @event.GetType();

        if (_subscriptionProfileManager.HasSubscriptionsForEvent(eventType).Equals(false))
        {
            _logger.LogTrace("There are no subscriptions for this event.");
            return;
        }

        var executors = _subscriptionProfileManager.GetEventHandlerExecutorForEvent(eventType);
        
        var tasks = new List<Task>();
        
        await using var serviceScope = this._serviceScopeFactory.CreateAsyncScope();

        var serviceProvider = serviceScope.ServiceProvider;
        
        foreach (var executor in executors)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
           
            var task = ExecuteHandlerWithExceptionHandling(serviceProvider,executor, @event, headers, cancellationToken);
            tasks.Add(task);
        }
        
        await Task.Yield();
        await Task.WhenAll(tasks);

        _logger.LogTrace($"Processed event {eventType.Name}.");
    }
    
    private async Task ExecuteHandlerWithExceptionHandling(IServiceProvider serviceProvider,
                                                           IEventHandlerExecutor eventHandlerExecutor, 
                                                           object @event, Headers headers, CancellationToken cancellationToken)
    {
        var handlerInstance = serviceProvider.GetRequiredService(eventHandlerExecutor.HandlerType);
        if (handlerInstance is null)
        {
            _logger.LogWarning($"There are no handlers for the following event: {eventHandlerExecutor.EventType.Name}");
            return;
        }
        
        var handlerDelegate = CachedHandlers.GetOrAdd
        (
            (eventHandlerExecutor.EventType, eventHandlerExecutor.HandlerType), 
            key => eventHandlerExecutor.CreateHandlerDelegate()
        );

        try
        {
            await handlerDelegate(handlerInstance, @event, headers, cancellationToken);
        }
        catch (Exception exception)
        {
            var exceptionContext = new ExceptionContext(@event, headers, exception);
            var pipeline = serviceProvider.GetRequiredService<IExceptionHandlerPipeline>();
            pipeline.Execute(exceptionContext);
        }
    }
}

using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleEventBus.Event;
using SimpleEventBus.ExceptionHandlers;
using SimpleEventBus.Profile;

namespace SimpleEventBus.Subscriber;

/// <summary>
///     The default implementation of the event handler invoker.
///     Responsible for executing registered event handlers with exception handling support.
/// </summary>
/// <seealso cref="IEventHandlerInvoker" />
internal class DefaultEventHandlerInvoker : IEventHandlerInvoker
{
    /// <summary>
    ///     A cache of handler delegates to improve performance by avoiding repetitive delegate generation.
    /// </summary>
    private static readonly ConcurrentDictionary<(Type EventType, Type HandlerType), Func<object, object, Headers, CancellationToken, Task>> CachedHandlers = new();

    /// <summary>
    ///     Logger instance.
    /// </summary>
    private readonly ILogger<DefaultEventHandlerInvoker> _logger;

    /// <summary>
    ///     The application-wide service provider for resolving dependencies.
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Manages all event subscriptions and provides handler executors.
    /// </summary>
    private readonly ISubscriptionProfileManager _subscriptionProfileManager;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultEventHandlerInvoker" /> class.
    ///     Pre-caches all handler delegates at construction time.
    /// </summary>
    public DefaultEventHandlerInvoker(IServiceProvider serviceProvider,
                                      ILogger<DefaultEventHandlerInvoker> logger,
                                      ISubscriptionProfileManager subscriptionProfileManager)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _subscriptionProfileManager = subscriptionProfileManager;

        InitializeHandlerCache();
    }

    /// <summary>
    ///     Eagerly creates and caches all handler delegates for fast lookup during invocation.
    /// </summary>
    private void InitializeHandlerCache()
    {
        foreach (var (eventType, executors) in _subscriptionProfileManager.GetAllSubscriptions())
        {
            foreach (var executor in executors)
            {
                var key = (executor.EventType, executor.HandlerType);
                CachedHandlers.TryAdd(key, executor.CreateHandlerDelegate());
            }
        }
    }

    /// <summary>
    ///     Invokes all registered handlers for a given event instance.
    /// </summary>
    /// <param name="event">The event instance.</param>
    /// <param name="headers">Associated headers metadata.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public async Task InvokeAsync(object @event, Headers headers, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(headers);

        var eventType = @event.GetType();
        
        if (_subscriptionProfileManager.HasSubscriptionsForEvent(eventType).Equals(false))
        {
            _logger.LogTrace("There are no subscriptions for this event.");
            return;
        }

        var executors = _subscriptionProfileManager.GetEventHandlerExecutorsForEvent(eventType);
        
        await Parallel.ForEachAsync(executors, cancellationToken, async (executor, token) =>
        {
            try
            {
                var handler = _serviceProvider.GetService(executor.HandlerType);
                if (handler is null)
                {
                    _logger.LogWarning("Handler not found for {EventType}", executor.EventType.Name);
                    return;
                }
                
                var key = (executor.EventType, executor.HandlerType);
                if (CachedHandlers.TryGetValue(key, out var handlerDelegate).Equals(false))
                {
                    _logger.LogWarning("Cached handler not found for {EventType}", executor.EventType.Name);
                    return;
                }
                
                await handlerDelegate!(handler, @event, headers, token);
            }
            catch (Exception exception)
            {
                var exceptionContext = new ExceptionContext(@event, headers, exception);
                var exceptionHandlerInvoker = _serviceProvider.GetRequiredService<IExceptionHandlerInvoker>();
                await exceptionHandlerInvoker.ExecuteAsync(exceptionContext, cancellationToken);
            }
        });

        _logger.LogTrace("Processed event {EventType}", eventType.Name);
    }
}

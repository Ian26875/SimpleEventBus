using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FluentEventBus.Errors;
using FluentEventBus.Event;
using FluentEventBus.ExceptionHandlers;
using FluentEventBus.Profile;
using FluentEventBus.Subscriber.Executors;

namespace FluentEventBus.Subscriber;

/// <summary>
///     The default implementation of the event handler invoker.
///     Responsible for executing registered event handlers with exception handling support.
/// </summary>
/// <seealso cref="IEventHandlerInvoker" />
internal class DefaultEventHandlerInvoker : IEventHandlerInvoker
{
    /// <summary>
    ///     A cache of handler delegates to improve performance by avoiding repetitive delegate
    ///     generation. Instance state: the invoker is a singleton, so the cache still lives for
    ///     the whole process but never leaks across DI containers.
    /// </summary>
    private readonly ConcurrentDictionary<(Type EventType, Type HandlerType), Func<object, object, Headers, CancellationToken, Task>> CachedHandlers = new();

    /// <summary>
    ///     Logger instance.
    /// </summary>
    private readonly ILogger<DefaultEventHandlerInvoker> _logger;

    /// <summary>
    ///     
    /// </summary>
    private readonly IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    ///     Manages all event subscriptions and provides handler executors.
    /// </summary>
    private readonly ISubscriptionProfileManager _subscriptionProfileManager;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultEventHandlerInvoker" /> class.
    ///     Pre-caches all handler delegates at construction time.
    /// </summary>
    public DefaultEventHandlerInvoker(IServiceScopeFactory serviceScopeFactory,
                                      ILogger<DefaultEventHandlerInvoker> logger,
                                      ISubscriptionProfileManager subscriptionProfileManager)
    {
        _serviceScopeFactory = serviceScopeFactory;
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
            _logger.LogWarning("No subscription found for event type: {EventType}. Event full name: {EventTypeFullName}. " +
                               "Ensure it has been mapped and subscribed correctly.", eventType.Name, eventType.FullName);
            return;
        }


        var executors = _subscriptionProfileManager.GetEventHandlerExecutorsForEvent(eventType);

        if (executors.Count == 1)
        {
            await ExecuteHandlerAsync(executors[0], @event, headers, cancellationToken);
        }
        else
        {
            await Parallel.ForEachAsync(executors, cancellationToken, async (executor, token) =>
                await ExecuteHandlerAsync(executor, @event, headers, token));
        }

        _logger.LogTrace("Processed event {EventType}", eventType.Name);
    }

    /// <summary>
    ///     Executes a single handler inside its own service scope, so scoped dependencies
    ///     (e.g. DbContext) are never shared across concurrently running handlers.
    /// </summary>
    private async Task ExecuteHandlerAsync(IEventHandlerExecutor executor, object @event, Headers headers, CancellationToken cancellationToken)
    {
        await using var serviceScope = _serviceScopeFactory.CreateAsyncScope();
        var serviceProvider = serviceScope.ServiceProvider;

        try
        {
            var handler = serviceProvider.GetService(executor.HandlerType);
            if (handler is null)
            {
                throw new HandlerNotRegisteredException(executor.EventType, executor.HandlerType);
            }

            var key = (executor.EventType, executor.HandlerType);
            if (CachedHandlers.TryGetValue(key, out var handlerDelegate).Equals(false))
            {
                throw new HandlerNotRegisteredException(executor.EventType, executor.HandlerType);
            }

            await handlerDelegate!(handler, @event, headers, cancellationToken);
        }
        catch (Exception exception)
        {
            var exceptionContext = new ExceptionContext(@event, headers, exception);
            var exceptionHandlerInvoker = serviceProvider.GetRequiredService<IExceptionHandlerInvoker>();
            await exceptionHandlerInvoker.ExecuteAsync(exceptionContext, cancellationToken);

            // Unhandled failures must reach the transport so it can nack / retry / dead-letter.
            if (exceptionContext.Handled is false)
            {
                exceptionContext.ExceptionDispatch.Throw();
            }
        }
    }
}

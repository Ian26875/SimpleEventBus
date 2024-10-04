using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleEventBus.Event;
using SimpleEventBus.ExceptionHandlers;
using SimpleEventBus.Profile;
using SimpleEventBus.Subscriber;

namespace SimpleEventBus;

/// <summary>
///     The default event handler invoker class
/// </summary>
/// <seealso cref="IEventHandlerInvoker" />
internal class DefaultEventHandlerInvoker : IEventHandlerInvoker
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DefaultEventHandlerInvoker> _logger;
    private readonly ISubscriptionProfileManager _subscriptionProfileManager;

    // 缓存委托
    private static readonly ConcurrentDictionary<Type, Func<object, object, Headers, CancellationToken, Task>> _cachedHandlers 
        = new ConcurrentDictionary<Type, Func<object, object, Headers, CancellationToken, Task>>();

    public DefaultEventHandlerInvoker(IServiceProvider serviceProvider, ILogger<DefaultEventHandlerInvoker> logger, ISubscriptionProfileManager subscriptionProfileManager)
    {
        _serviceProvider = serviceProvider;
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

        if (!_subscriptionProfileManager.HasSubscriptionsForEvent(eventType))
        {
            _logger.LogTrace("There are no subscriptions for this event.");
            return;
        }

        var eventHandlerTypes = _subscriptionProfileManager.GetEventHandlersForEvent(eventType);

        var tasks = new List<Task>();

        foreach (var eventHandlerType in eventHandlerTypes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var handler = _serviceProvider.GetRequiredService(eventHandlerType);
            if (handler is null)
            {
                _logger.LogWarning($"There are no handlers for the following event: {eventType.Name}");
                continue;
            }

            var task = ExecuteHandlerWithExceptionHandling(eventType, handler, @event, headers, cancellationToken);
            tasks.Add(task);
        }

        await Task.Yield();
        await Task.WhenAll(tasks);

        _logger.LogTrace($"Processed event {eventType.Name}.");
    }

    /// <summary>
    /// 封装Handler的执行并且处理异常的逻辑
    /// </summary>
    private async Task ExecuteHandlerWithExceptionHandling(Type eventType, object handler, object @event, Headers headers, CancellationToken cancellationToken)
    {
        var handlerDelegate = _cachedHandlers.GetOrAdd(eventType, type => CreateHandlerDelegate(type));

        try
        {
            // 调用处理程序
            await handlerDelegate(handler, @event, headers, cancellationToken);
        }
        catch (Exception exception)
        {
            var exceptionContext = new ExceptionContext(@event, headers, exception);
            var pipeline = this._serviceProvider.GetRequiredService<IExceptionHandlerPipeline>();
            pipeline.Execute(exceptionContext);
        }
    }

    private static Func<object, object, Headers, CancellationToken, Task> CreateHandlerDelegate(Type eventType)
    {
        var eventHandlerInterfaceType = typeof(IEventHandler<>).MakeGenericType(eventType);
        var methodInfo = eventHandlerInterfaceType.GetMethod(nameof(IEventHandler<object>.HandleAsync));
        if (methodInfo is null)
        {
            throw new MissingMethodException();
        }
        
        var handlerParam = Expression.Parameter(typeof(object), "handler");
        var eventParam = Expression.Parameter(typeof(object), "event");
        var headersParam = Expression.Parameter(typeof(Headers), "headers");
        var cancellationTokenParam = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        var castHandler = Expression.Convert(handlerParam, eventHandlerInterfaceType);
        var castEvent = Expression.Convert(eventParam, eventType);

        var call = Expression.Call
        (
            castHandler,
            methodInfo,
            castEvent,
            headersParam,
            cancellationTokenParam
        );

        return Expression.Lambda<Func<object, object, Headers, CancellationToken, Task>>
        (
            call, 
            handlerParam, 
            eventParam, 
            headersParam,
            cancellationTokenParam
        ).Compile();
    }
}

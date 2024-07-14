namespace SimpleEventBus;

/// <summary>
///     The default event handler invoker class
/// </summary>
/// <seealso cref="IEventHandlerInvoker" />
internal class DefaultEventHandlerInvoker : IEventHandlerInvoker
{
    /// <summary>
    ///     The event bus option
    /// </summary>
    private readonly EventBusOption _eventBusOption;


    /// <summary>
    ///     The subscription manager
    /// </summary>
    private readonly ISubscriptionManager _subscriptionManager;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultEventHandlerInvoker" /> class
    /// </summary>
    /// <param name="eventBusOption">The event bus option</param>
    /// <param name="subscriptionManager">The subscription manager</param>
    internal DefaultEventHandlerInvoker(EventBusOption eventBusOption, ISubscriptionManager subscriptionManager)
    {
        _eventBusOption = eventBusOption;
        _subscriptionManager = subscriptionManager;
    }


    /// <summary>
    /// Invokes the event
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="event">The event</param>
    /// <param name="headers">The headers</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public async Task InvokeAsync<TEvent>(TEvent @event, Headers headers, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        var eventHandlers = _subscriptionManager.GetHandlersForEvent(@event);

        switch (_eventBusOption.HandlerStrategy)
        {
            case HandlerStrategy.ForEach:
                await ForEachInvokeEventHandler(@event, headers, eventHandlers);
                break;

            case HandlerStrategy.TaskWhenAll:
                await TaskWhenAllInvokeEventHandler(@event, headers, eventHandlers);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Fors the each invoke event handler using the specified event
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="@event">The event</param>
    /// <param name="headers">The headers</param>
    /// <param name="eventHandlers">The event handlers</param>
    private async Task ForEachInvokeEventHandler<TEvent>(TEvent @event, Headers headers,
        IEnumerable<IEventHandler<TEvent>> eventHandlers) where TEvent : class
    {
        foreach (var eventHandler in eventHandlers) await eventHandler.HandleAsync(@event, headers);
    }


    /// <summary>
    /// Tasks the when all invoke event handler using the specified event
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="@event">The event</param>
    /// <param name="headers">The headers</param>
    /// <param name="eventHandlers">The event handlers</param>
    private async Task TaskWhenAllInvokeEventHandler<TEvent>(TEvent @event, Headers headers,
        IEnumerable<IEventHandler<TEvent>> eventHandlers) where TEvent : class
    {
        await Task.WhenAll
        (
            eventHandlers.Select(e => e.HandleAsync(@event, headers))
        );
    }
}
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
    ///     The event handler resolver
    /// </summary>
    private readonly IEventHandlerResolver _eventHandlerResolver;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DefaultEventHandlerInvoker" /> class
    /// </summary>
    /// <param name="eventBusOption">The event bus option</param>
    /// <param name="eventHandlerResolver">The event handler resolver</param>
    public DefaultEventHandlerInvoker(EventBusOption eventBusOption, IEventHandlerResolver eventHandlerResolver)
    {
        _eventBusOption = eventBusOption;
        _eventHandlerResolver = eventHandlerResolver;
    }

    public async Task InvokeAsync<TEvent>(TEvent @event, Headers headers,
                                          CancellationToken cancellationToken = default(CancellationToken))
        where TEvent : class
    {
        var eventHandlers = _eventHandlerResolver.GetHandlersForEvent(@event);

        var eventContext = new EventContext<TEvent>(@event, headers);

        switch (_eventBusOption.HandlerStrategy)
        {
            case HandlerStrategy.ForEach:
                await ForEachInvokeEventHandler(eventContext, eventHandlers, cancellationToken);
                break;

            case HandlerStrategy.TaskWhenAll:
                await TaskWhenAllInvokeEventHandler(eventContext, eventHandlers, cancellationToken);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task ForEachInvokeEventHandler<TEvent>(EventContext<TEvent> eventContext,
                                                         IEnumerable<IEventHandler<TEvent>> eventHandlers,
                                                         CancellationToken cancellationToken) where TEvent : class
    {
        foreach (var eventHandler in eventHandlers)
        {
            await eventHandler.HandleAsync(eventContext, cancellationToken);
        }
    }

    private async Task TaskWhenAllInvokeEventHandler<TEvent>(EventContext<TEvent> eventContext,
                                                             IEnumerable<IEventHandler<TEvent>> eventHandlers,
                                                             CancellationToken cancellationToken) where TEvent : class
    {
        await Task.WhenAll
        (
            eventHandlers.Select(e => e.HandleAsync(eventContext, cancellationToken))
        );
    }

    public Task InvokeAsync<TEvent>(EventContext<TEvent> eventContext, CancellationToken cancellationToken = default) where TEvent : class
    {
        throw new NotImplementedException();
    }
}
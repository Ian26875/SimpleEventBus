using SimpleEventBus.Event;
using SimpleEventBus.Subscriber;

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
    
    /// <summary>
    /// Fors the each invoke event handler using the specified event context
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="eventContext">The event context</param>
    /// <param name="eventHandlers">The event handlers</param>
    /// <param name="cancellationToken">The cancellation token</param>
    private async Task ForEachInvokeEventHandler<TEvent>(EventContext<TEvent> eventContext,
                                                         IEnumerable<IEventHandler<TEvent>> eventHandlers,
                                                         CancellationToken cancellationToken) where TEvent : class
    {
        foreach (var eventHandler in eventHandlers)
        {
            await eventHandler.HandleAsync(eventContext, cancellationToken);
        }
    }

    /// <summary>
    /// Tasks the when all invoke event handler using the specified event context
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="eventContext">The event context</param>
    /// <param name="eventHandlers">The event handlers</param>
    /// <param name="cancellationToken">The cancellation token</param>
    private async Task TaskWhenAllInvokeEventHandler<TEvent>(EventContext<TEvent> eventContext,
                                                             IEnumerable<IEventHandler<TEvent>> eventHandlers,
                                                             CancellationToken cancellationToken) where TEvent : class
    {
        await Task.WhenAll
        (
            eventHandlers.Select(e => e.HandleAsync(eventContext, cancellationToken))
        );
    }

    /// <summary>
    /// Invokes the event context
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="eventContext">The event context</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public async Task InvokeAsync<TEvent>(EventContext<TEvent> eventContext, CancellationToken cancellationToken = default) where TEvent : class
    {
        var eventHandlers = _eventHandlerResolver.GetHandlersForEvent(eventContext.Event);
        
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
}
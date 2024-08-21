using SimpleEventBus.Event;

namespace SimpleEventBus.InMemory;

/// <summary>
///     The in memory event bus class
/// </summary>
/// <seealso cref="IEventPublisher" />
internal class InMemoryEventPublisher : AbstractEventPublisher
{
    /// <summary>
    ///     The background queue
    /// </summary>
    private readonly BackgroundQueue _backgroundQueue;

    /// <summary>
    ///     The event handler invoker
    /// </summary>
    private readonly IEventHandlerInvoker _eventHandlerInvoker;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InMemoryEventPublisher" /> class
    /// </summary>
    /// <param name="eventHandlerInvoker">The event handler invoker</param>
    /// <param name="backgroundQueue">The background queue</param>
    internal InMemoryEventPublisher(IEventHandlerInvoker eventHandlerInvoker, BackgroundQueue backgroundQueue)
    {
        _eventHandlerInvoker = eventHandlerInvoker;
        _backgroundQueue = backgroundQueue;
    }

    /// <summary>
    ///     Publishes the event using the specified event context
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="eventContext">The event context</param>
    /// <param name="cancellationToken">The cancellation token</param>
    protected override async Task PublishEventAsync<TEvent>(EventContext<TEvent> eventContext,
                                                            CancellationToken cancellationToken =
                                                                default(CancellationToken))
    {
        await _backgroundQueue.EnqueueAsync
        (
            async token => await _eventHandlerInvoker.InvokeAsync(eventContext, cancellationToken),
            cancellationToken
        );
    }
}
namespace SimpleEventBus.InMemory;

/// <summary>
///     The in memory event bus class
/// </summary>
/// <seealso cref="IEventBus" />
internal class InMemoryEventBus : IEventBus
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
    ///     Initializes a new instance of the <see cref="InMemoryEventBus" /> class
    /// </summary>
    /// <param name="eventHandlerInvoker">The event handler invoker</param>
    /// <param name="backgroundQueue">The background queue</param>
    internal InMemoryEventBus(IEventHandlerInvoker eventHandlerInvoker, BackgroundQueue backgroundQueue)
    {
        _eventHandlerInvoker = eventHandlerInvoker;
        _backgroundQueue = backgroundQueue;
    }


    /// <summary>
    ///     Publishes the event
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="event">The event</param>
    /// <param name="headers">The headers</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task PublishAsync<TEvent>(TEvent @event, 
                                           Headers? headers,
                                           CancellationToken cancellationToken = default) where TEvent : class
    {
        ArgumentNullException.ThrowIfNull(@event);

        headers ??= new Headers();

        await _backgroundQueue.EnqueueAsync
        (
            async token => await _eventHandlerInvoker.InvokeAsync(@event, headers, token),
            cancellationToken
        );
    }
}
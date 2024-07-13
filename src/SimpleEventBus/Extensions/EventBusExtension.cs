namespace SimpleEventBus;

/// <summary>
///     The event bus extension class
/// </summary>
public static class EventBusExtension
{
    /// <summary>
    ///     Publishes the event bus
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="eventBus">The event bus</param>
    /// <param name="event">The event</param>
    /// <param name="headers">The headers</param>
    public static void Publish<TEvent>(IEventBus eventBus, TEvent @event, Headers? headers = null) where TEvent : class
    {
        eventBus.PublishAsync(@event, headers).ConfigureAwait(false).GetAwaiter().GetResult();
    }
}
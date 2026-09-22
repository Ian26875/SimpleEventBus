using SimpleEventBus.Event;

namespace SimpleEventBus;

/// <summary>
///     The event bus extension class
/// </summary>
public static class EventPublisherExtension
{
    /// <summary>
    ///     Publishes the event bus
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="eventPublisher">The event bus</param>
    /// <param name="event">The event</param>
    /// <param name="headers">The headers</param>
    public static void Publish<TEvent>(IEventPublisher eventPublisher, TEvent @event, Headers? headers = null) where TEvent : class
    {
        eventPublisher.PublishAsync(@event, headers).ConfigureAwait(false).GetAwaiter().GetResult();
    }
}
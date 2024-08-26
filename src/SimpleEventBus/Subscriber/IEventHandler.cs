using SimpleEventBus.Event;

namespace SimpleEventBus.Subscriber;

/// <summary>
///     The event handler interface
/// </summary>
public interface IEventHandler<in TEvent> where TEvent : class
{
    /// <summary>
    /// Handles the event
    /// </summary>
    /// <param name="event">The event</param>
    /// <param name="headers">The headers</param>
    /// <param name="cancellationToken">The cancellation token</param>
    Task HandleAsync(TEvent @event,Headers headers, CancellationToken cancellationToken);
}

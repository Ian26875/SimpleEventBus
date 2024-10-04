using SimpleEventBus.Event;

namespace SimpleEventBus.Subscriber;

/// <summary>
///     The event handler invoker interface
/// </summary>
public interface IEventHandlerInvoker
{
    /// <summary>
    /// Invokes the event
    /// </summary>
    /// <param name="event">The event</param>
    /// <param name="headers">The headers</param>
    /// <param name="cancellationToken">The cancellation token</param>
    Task InvokeAsync(object @event,Headers headers, CancellationToken cancellationToken = default(CancellationToken));
}
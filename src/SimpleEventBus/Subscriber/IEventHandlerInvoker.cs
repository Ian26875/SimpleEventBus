using SimpleEventBus.Event;

namespace SimpleEventBus.Subscriber;

/// <summary>
///     The event handler invoker interface
/// </summary>
public interface IEventHandlerInvoker
{
    /// <summary>
    /// Invokes the event context
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="eventContext">The event context</param>
    /// <param name="cancellationToken">The cancellation token</param>
    Task InvokeAsync<TEvent>(EventContext<TEvent> eventContext, 
                             CancellationToken cancellationToken = default(CancellationToken)) where TEvent : class;
}
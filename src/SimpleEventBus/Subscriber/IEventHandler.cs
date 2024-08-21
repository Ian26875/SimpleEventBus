using SimpleEventBus.Event;

namespace SimpleEventBus.Subscriber;

/// <summary>
///     The event handler interface
/// </summary>
public interface IEventHandler<TEvent> where TEvent : class
{
    /// <summary>
    ///     Handles the event context
    /// </summary>
    /// <param name="eventContext">The event context</param>
    /// <param name="cancellationToken">The cancellation token</param>
    Task HandleAsync(EventContext<TEvent> eventContext, CancellationToken cancellationToken);
}

public interface IEventHandler
{
    /// <summary>
    ///     Handles the event context
    /// </summary>
    /// <param name="eventContext">The event context</param>
    /// <param name="cancellationToken">The cancellation token</param>
    Task HandleAsync<TEvent>(EventContext<TEvent> eventContext, CancellationToken cancellationToken)
        where TEvent : class;
}
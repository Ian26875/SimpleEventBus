namespace SimpleEventBus;

/// <summary>
///     The event handler invoker interface
/// </summary>
public interface IEventHandlerInvoker
{
    /// <summary>
    ///     Invokes the event
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="event">The event</param>
    /// <param name="headers">The headers</param>
    /// <param name="cancellationToken">The cancellation token</param>
    Task InvokeAsync<TEvent>(TEvent @event, Headers headers, CancellationToken cancellationToken = default) where TEvent : class;
}
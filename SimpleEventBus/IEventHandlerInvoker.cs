namespace SimpleEventBus;

/// <summary>
/// The event handler invoker interface
/// </summary>
public interface IEventHandlerInvoker
{
    /// <summary>
    /// Invokes the event
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="@event">The event</param>
    Task InvokeAsync<TEvent>(TEvent @event);
}
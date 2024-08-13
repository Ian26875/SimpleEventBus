namespace SimpleEventBus;

/// <summary>
/// The event bus interface
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes the event
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="@event">The event</param>
    /// <param name="headers">The headers</param>
    /// <param name="cancellationToken">The cancellation token</param>
    Task PublishAsync<TEvent>(TEvent @event, Headers? headers = null, CancellationToken cancellationToken = default) where TEvent : class;
}
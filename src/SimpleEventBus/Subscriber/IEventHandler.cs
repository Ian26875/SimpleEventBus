using SimpleEventBus.Event;

namespace SimpleEventBus.Subscriber;

/// <summary>
///     The event handler interface
/// </summary>
public interface IEventHandler<in TEvent> where TEvent : class
{
    Task HandleAsync(TEvent @event,Headers headers, CancellationToken cancellationToken);
}

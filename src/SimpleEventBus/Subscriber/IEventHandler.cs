using SimpleEventBus.Event;

namespace SimpleEventBus;

/// <summary>
/// The event handler interface
/// </summary>
public interface IEventHandler<TEvent> where TEvent : class
{
    Task HandleAsync(EventContext<TEvent> eventContext,CancellationToken cancellationToken);
}


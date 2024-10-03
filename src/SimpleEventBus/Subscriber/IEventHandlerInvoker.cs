using SimpleEventBus.Event;

namespace SimpleEventBus.Subscriber;

/// <summary>
///     The event handler invoker interface
/// </summary>
public interface IEventHandlerInvoker
{

    Task InvokeAsync<T>(T @event, Headers headers, CancellationToken cancellationToken = default(CancellationToken)) where T : class;
}
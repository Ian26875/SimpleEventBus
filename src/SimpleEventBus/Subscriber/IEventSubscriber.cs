using SimpleEventBus.Event;

namespace SimpleEventBus.Subscriber;

/// <summary>
/// The event subscriber interface
/// </summary>
public interface IEventSubscriber
{
    /// <summary>
    /// Subscribes the event names
    /// </summary>
    /// <param name="eventNames">The event names</param>
    /// <param name="cancellationToken">The cancellation token</param>
    Task SubscribeAsync(List<string> eventNames,CancellationToken cancellationToken = default(CancellationToken));

    /// <summary>
    /// consumer received
    /// </summary>
    Func<EventData, Task> ConsumerReceived { set; get; }

    /// <summary>
    /// Receives the cancellation token
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    Task ReceiveAsync(CancellationToken cancellationToken = default(CancellationToken));
}
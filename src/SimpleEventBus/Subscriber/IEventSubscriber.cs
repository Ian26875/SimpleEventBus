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
    Task SubscribeAsync(List<string> eventNames);

    /// <summary>
    /// Sets the value of the consumer received
    /// </summary>
    Func<EventData, Task> ConsumerReceived { set; get; }

}
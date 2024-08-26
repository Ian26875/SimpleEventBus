namespace SimpleEventBus.Subscriber;

/// <summary>
///     The event subscriber interface
/// </summary>
public interface IEventSubscriber
{
    /// <summary>
    ///     Subscribes the event type
    /// </summary>
    /// <param name="eventType">The event type</param>
    /// <param name="eventHandler">The event handler</param>
    Task SubscribeAsync(Type eventType, Type eventHandler);
}
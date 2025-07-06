using SimpleEventBus.Subscriber.Executors;

namespace SimpleEventBus.Profile;

/// <summary>
/// The subscription profile manager interface
/// </summary>
public interface ISubscriptionProfileManager
{

    /// <summary>
    /// Initializes this instance
    /// </summary>
    internal void Initialize();
    
    /// <summary>
    /// Has the subscriptions for event using the specified event type
    /// </summary>
    /// <param name="eventType">The event type</param>
    /// <returns>The bool</returns>
    public bool HasSubscriptionsForEvent(Type eventType);

    /// <summary>
    /// Gets the all event types
    /// </summary>
    /// <returns>A list of type</returns>
    public List<Type> GetAllEventTypes();
    
    /// <summary>
    /// Gets the event handler executor for event using the specified event type
    /// </summary>
    /// <param name="eventType">The event type</param>
    /// <returns>A list of i event handler executor</returns>
    public List<IEventHandlerExecutor> GetEventHandlerExecutorsForEvent(Type eventType);

    /// <summary>
    /// Gets the error handlers for event using the specified event type
    /// </summary>
    /// <param name="eventType">The event type</param>
    /// <returns>A list of type</returns>
    public List<Type> GetErrorHandlersForEvent(Type eventType);

}
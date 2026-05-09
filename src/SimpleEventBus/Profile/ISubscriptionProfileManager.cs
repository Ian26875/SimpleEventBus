using SimpleEventBus.Subscriber.Executors;

namespace SimpleEventBus.Profile;
/// <summary>
/// Represents a manager for subscription profiles used to handle event-to-handler mappings
/// and associated error-handling logic.
/// </summary>
public interface ISubscriptionProfileManager
{
<<<<<<< HEAD
    public bool HasSubscriptionsForEvent(Type eventType);

    public List<Type> GetAllEventTypes();
    
    public List<IEventHandlerExecutor> GetEventHandlerExecutorForEvent(Type eventType);

    public List<Type> GetErrorHandlersForEvent(Type eventType);
}
=======
    /// <summary>
    /// Initializes the manager by loading all subscription profiles and their handler mappings.
    /// </summary>
    internal void Initialize();

    /// <summary>
    /// Determines whether there are any registered subscriptions for the specified event type.
    /// </summary>
    /// <param name="eventType">The event type to check.</param>
    /// <returns><c>true</c> if the event has at least one registered handler; otherwise, <c>false</c>.</returns>
    bool HasSubscriptionsForEvent(Type eventType);

    /// <summary>
    /// Gets a list of all event types that have been registered in the system.
    /// </summary>
    /// <returns>A list of registered event types.</returns>
    IReadOnlyList<Type> GetAllEventTypes();

    /// <summary>
    /// Retrieves all handler executors associated with the specified event type.
    /// </summary>
    /// <param name="eventType">The type of the event.</param>
    /// <returns>A list of <see cref="IEventHandlerExecutor"/> instances that will handle the event.</returns>
    IReadOnlyList<IEventHandlerExecutor> GetEventHandlerExecutorsForEvent(Type eventType);

    /// <summary>
    /// Retrieves all error handler types registered to handle exceptions for the specified event type.
    /// </summary>
    /// <param name="eventType">The type of the event.</param>
    /// <returns>A list of types implementing <see cref="IEventExceptionHandler"/>.</returns>
    IReadOnlyList<Type> GetErrorHandlersForEvent(Type eventType);

    /// <summary>
    /// Gets all event subscriptions and their corresponding handler executors.
    /// </summary>
    /// <returns>A dictionary of event type to its list of executors.</returns>
    Dictionary<Type, List<IEventHandlerExecutor>> GetAllSubscriptions();
}
>>>>>>> feature/BuildEventHandlerExecutor

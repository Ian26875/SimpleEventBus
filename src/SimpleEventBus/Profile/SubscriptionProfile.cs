using SimpleEventBus.Subscriber.Executors;

namespace SimpleEventBus.Profile;

/// <summary>
///     The base class for subscription profiles.
/// </summary>
public abstract class SubscriptionProfile
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SubscriptionProfile" /> class
    /// </summary>
    protected SubscriptionProfile()
    {
        EventHandlers = new Dictionary<Type, List<Type>>();
        ErrorHandlers = new Dictionary<Type, List<Type>>();
        EventHandlerExecutors = new Dictionary<Type, List<IEventHandlerExecutor>>();
    }

    /// <summary>
    ///     Maps event types to their list of handler types.
    /// </summary>
    public Dictionary<Type, List<Type>> EventHandlers { get; }

    /// <summary>
    /// Gets the value of the event handler executors
    /// </summary>
    public Dictionary<Type, List<IEventHandlerExecutor>> EventHandlerExecutors { get; }

    /// <summary>
    ///     Gets the value of the error handlers
    /// </summary>
    public Dictionary<Type, List<Type>> ErrorHandlers { get; }

    /// <summary>
    /// Adds the subscription using the specified event type
    /// </summary>
    /// <param name="eventType">The event type</param>
    /// <param name="eventHandlerExecutor">The event handler executor</param>
    /// <exception cref="ArgumentException">Handler type '{eventHandlerExecutor}' is already registered for event type '{eventType.FullName}'.</exception>
    public void AddSubscription(Type eventType, IEventHandlerExecutor eventHandlerExecutor)
    {
        if (EventHandlerExecutors.TryGetValue(eventType, out var handlersList).Equals(false))
        {
            handlersList = new List<IEventHandlerExecutor>();
            EventHandlerExecutors[eventType] = handlersList;
        }

        if (handlersList.Contains(eventHandlerExecutor))
        {
            throw new ArgumentException(
                $"Handler type '{eventHandlerExecutor}' is already registered for event type '{eventType.FullName}'.");
            
        }

        handlersList.Add(eventHandlerExecutor);
    }

    /// <summary>
    ///     Creates the subscription using the specified event type
    /// </summary>
    /// <param name="eventType">The event type</param>
    /// <param name="eventHandlerType">The event handler type</param>
    /// <exception cref="ArgumentException">
    ///     Handler type '{eventHandlerType.FullName}' is already registered for event type
    ///     '{eventType.FullName}'.
    /// </exception>
    public void AddSubscription(Type eventType, Type eventHandlerType)
    {
        if (EventHandlers.TryGetValue(eventType, out var handlersList).Equals(false))
        {
            handlersList = new List<Type>();
            EventHandlers[eventType] = handlersList;
        }

        if (handlersList.Contains(eventHandlerType))
            throw new ArgumentException(
                $"Handler type '{eventHandlerType.FullName}' is already registered for event type '{eventType.FullName}'.");

        handlersList.Add(eventHandlerType);
    }

    /// <summary>
    ///     Creates the error handler using the specified event type
    /// </summary>
    /// <param name="eventType">The event type</param>
    /// <param name="errorHandlerType">The error handler type</param>
    /// <exception cref="ArgumentException">
    ///     Handler type '{errorHandlerType.FullName}' is already registered for event type
    ///     '{eventType.FullName}'.
    /// </exception>
    public void AddErrorFilter(Type eventType, Type errorHandlerType)
    {
        if (ErrorHandlers.TryGetValue(eventType, out var handlersList).Equals(false))
        {
            handlersList = new List<Type>();
            ErrorHandlers[eventType] = handlersList;
        }

        if (handlersList.Contains(errorHandlerType))
            throw new ArgumentException(
                $"Handler type '{errorHandlerType.FullName}' is already registered for event type '{eventType.FullName}'.");

        handlersList.Add(errorHandlerType);
    }
}
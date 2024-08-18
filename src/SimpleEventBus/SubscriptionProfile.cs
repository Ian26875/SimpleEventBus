namespace SimpleEventBus;

/// <summary>
/// The base class for subscription profiles.
/// </summary>
public abstract class SubscriptionProfile
{
    /// <summary>
    /// Maps event types to their list of handler types.
    /// </summary>
    internal Dictionary<Type, List<Type>> EventHandlers { get; }

    /// <summary>
    /// Gets the value of the error handlers
    /// </summary>
    internal Dictionary<Type, List<Type>> ErrorHandlers { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriptionProfile"/> class
    /// </summary>
    protected SubscriptionProfile()
    {
        this.EventHandlers = new Dictionary<Type, List<Type>>();
        this.ErrorHandlers = new Dictionary<Type, List<Type>>();
    }

    /// <summary>
    /// Creates the subscription using the specified event type
    /// </summary>
    /// <param name="eventType">The event type</param>
    /// <param name="eventHandlerType">The event handler type</param>
    /// <exception cref="ArgumentException">Handler type '{eventHandlerType.FullName}' is already registered for event type '{eventType.FullName}'.</exception>
    internal void CreateSubscription(Type eventType,Type eventHandlerType)
    {
        if (EventHandlers.TryGetValue(eventType, out var handlersList).Equals(false))
        {
            handlersList = new List<Type>();
            EventHandlers[eventType] = handlersList;
        }

        if (handlersList.Contains(eventHandlerType))
        {
            throw new ArgumentException($"Handler type '{eventHandlerType.FullName}' is already registered for event type '{eventType.FullName}'.");
        }

        handlersList.Add(eventHandlerType);
    }

    /// <summary>
    /// Creates the error handler using the specified event type
    /// </summary>
    /// <param name="eventType">The event type</param>
    /// <param name="errorHandlerType">The error handler type</param>
    /// <exception cref="ArgumentException">Handler type '{errorHandlerType.FullName}' is already registered for event type '{eventType.FullName}'.</exception>
    internal void CreateErrorHandler(Type eventType, Type errorHandlerType)
    {
        if (ErrorHandlers.TryGetValue(eventType, out var handlersList).Equals(false))
        {
            handlersList = new List<Type>();
            ErrorHandlers[eventType] = handlersList;
        }

        if (handlersList.Contains(errorHandlerType))
        {
            throw new ArgumentException($"Handler type '{errorHandlerType.FullName}' is already registered for event type '{eventType.FullName}'.");
        }

        handlersList.Add(errorHandlerType);
    }
}

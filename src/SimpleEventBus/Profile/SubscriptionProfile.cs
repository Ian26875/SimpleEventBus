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
        ErrorHandlers = new Dictionary<Type, List<Type>>();
        EventHandlerExecutors = new Dictionary<Type, List<IEventHandlerExecutor>>();
    }
    
    /// <summary>
    /// Gets the value of the event handler executors
    /// </summary>
    internal Dictionary<Type, List<IEventHandlerExecutor>> EventHandlerExecutors { get; }

    /// <summary>
    ///     Gets the value of the error handlers
    /// </summary>
    internal Dictionary<Type, List<Type>> ErrorHandlers { get; }

    /// <summary>
    /// Adds the subscription using the specified event type
    /// </summary>
    /// <param name="eventType">The event type</param>
    /// <param name="eventHandlerExecutor">The event handler executor</param>
    /// <exception cref="ArgumentException">Handler type '{eventHandlerExecutor}' is already registered for event type '{eventType.FullName}'.</exception>
    internal void AddSubscription(Type eventType, IEventHandlerExecutor eventHandlerExecutor)
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
    ///     Creates the error handler using the specified event type
    /// </summary>
    /// <param name="eventType">The event type</param>
    /// <param name="errorHandlerType">The error handler type</param>
    /// <exception cref="ArgumentException">
    ///     Handler type '{errorHandlerType.FullName}' is already registered for event type
    ///     '{eventType.FullName}'.
    /// </exception>
    internal void AddErrorFilter(Type eventType, Type errorHandlerType)
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
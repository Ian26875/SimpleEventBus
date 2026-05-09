using SimpleEventBus.Subscriber.Executors;

namespace SimpleEventBus;

/// <summary>
///     The base class for subscription profiles.
/// </summary>
public abstract class SubscriptionProfile
{
    private readonly Dictionary<Type, List<IEventHandlerExecutor>> _eventHandlerExecutors = new();
    private readonly Dictionary<Type, List<Type>> _errorHandlers = new();
    
    
    /// <summary>
    ///     Initializes a new instance of the <see cref="SubscriptionProfile" /> class.
    /// </summary>
    protected SubscriptionProfile()
    {
        
    }

    /// <summary>
    ///     Gets the registered event handler executors.
    /// </summary>
<<<<<<< HEAD
    public Dictionary<Type, List<Type>> EventHandlers { get; }
=======
    public IReadOnlyDictionary<Type, IReadOnlyList<IEventHandlerExecutor>> EventHandlerExecutors =>
        _eventHandlerExecutors.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyList<IEventHandlerExecutor>)kvp.Value.AsReadOnly()
        );
>>>>>>> feature/BuildEventHandlerExecutor

    /// <summary>
    ///     Gets the registered error handler types.
    /// </summary>
<<<<<<< HEAD
    public Dictionary<Type, List<IEventHandlerExecutor>> EventHandlerExecutors { get; }

=======
    public IReadOnlyDictionary<Type, IReadOnlyList<Type>> ErrorHandlers =>
        _errorHandlers.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyList<Type>)kvp.Value.AsReadOnly()
        );
    
>>>>>>> feature/BuildEventHandlerExecutor
    /// <summary>
    ///     Adds a subscription for the specified event type.
    /// </summary>
<<<<<<< HEAD
    public Dictionary<Type, List<Type>> ErrorHandlers { get; }

    /// <summary>
    /// Adds the subscription using the specified event type
    /// </summary>
    /// <param name="eventType">The event type</param>
    /// <param name="eventHandlerExecutor">The event handler executor</param>
    /// <exception cref="ArgumentException">Handler type '{eventHandlerExecutor}' is already registered for event type '{eventType.FullName}'.</exception>
    public void AddSubscription(Type eventType, IEventHandlerExecutor eventHandlerExecutor)
=======
    /// <param name="eventType">The event type.</param>
    /// <param name="eventHandlerExecutor">The handler executor instance.</param>
    /// <exception cref="ArgumentException">Thrown if the handler has already been registered for the event type.</exception>
    internal void AddSubscription(Type eventType, IEventHandlerExecutor eventHandlerExecutor)
>>>>>>> feature/BuildEventHandlerExecutor
    {
        AddToDictionaryList(_eventHandlerExecutors, eventType, eventHandlerExecutor);
    }

    /// <summary>
    ///     Adds an error filter for the specified event type.
    /// </summary>
<<<<<<< HEAD
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
=======
    /// <param name="eventType">The event type.</param>
    /// <param name="errorHandlerType">The error handler type.</param>
    /// <exception cref="ArgumentException">Thrown if the error handler has already been registered for the event type.</exception>
    internal void AddErrorFilter(Type eventType, Type errorHandlerType)
>>>>>>> feature/BuildEventHandlerExecutor
    {
        AddToDictionaryList(_errorHandlers, eventType, errorHandlerType);
    }

    /// <summary>
    ///     Helper method to add an item to a dictionary of lists.
    /// </summary>
    /// <typeparam name="T">The list item type.</typeparam>
    /// <param name="dictionary">The dictionary to update.</param>
    /// <param name="key">The dictionary key.</param>
    /// <param name="value">The value to add.</param>
    /// <exception cref="ArgumentException">Thrown if the value already exists in the list for the given key.</exception>
    private static void AddToDictionaryList<T>(Dictionary<Type, List<T>> dictionary, Type key, T value)
    {
        if (!dictionary.TryGetValue(key, out var list))
        {
            list = new List<T>();
            dictionary[key] = list;
        }

        if (list.Contains(value))
        {
            throw new ArgumentException($"Handler of type '{value}' is already registered for event type '{key.FullName}'.");
        }

        list.Add(value);
    }
}

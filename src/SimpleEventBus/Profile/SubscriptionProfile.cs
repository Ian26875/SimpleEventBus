using SimpleEventBus.Subscriber.Executors;

namespace SimpleEventBus.Profile;

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
    public IReadOnlyDictionary<Type, IReadOnlyList<IEventHandlerExecutor>> EventHandlerExecutors =>
        _eventHandlerExecutors.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyList<IEventHandlerExecutor>)kvp.Value.AsReadOnly()
        );

    /// <summary>
    ///     Gets the registered error handler types.
    /// </summary>
    public IReadOnlyDictionary<Type, IReadOnlyList<Type>> ErrorHandlers =>
        _errorHandlers.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyList<Type>)kvp.Value.AsReadOnly()
        );
    
    /// <summary>
    ///     Adds a subscription for the specified event type.
    /// </summary>
    /// <param name="eventType">The event type.</param>
    /// <param name="eventHandlerExecutor">The handler executor instance.</param>
    /// <exception cref="ArgumentException">Thrown if the handler has already been registered for the event type.</exception>
    internal void AddSubscription(Type eventType, IEventHandlerExecutor eventHandlerExecutor)
    {
        AddToDictionaryList(_eventHandlerExecutors, eventType, eventHandlerExecutor);
    }

    /// <summary>
    ///     Adds an error filter for the specified event type.
    /// </summary>
    /// <param name="eventType">The event type.</param>
    /// <param name="errorHandlerType">The error handler type.</param>
    /// <exception cref="ArgumentException">Thrown if the error handler has already been registered for the event type.</exception>
    internal void AddErrorFilter(Type eventType, Type errorHandlerType)
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

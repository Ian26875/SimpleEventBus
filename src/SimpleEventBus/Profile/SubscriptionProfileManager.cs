using SimpleEventBus.Schema;
using SimpleEventBus.Subscriber.Executors;

namespace SimpleEventBus.Profile;

/// <summary>
/// Aggregates multiple subscription profiles into a single cohesive unit.
/// </summary>
public class SubscriptionProfileManager : ISubscriptionProfileManager
{
    /// <summary>
    /// The profiles
    /// </summary>
    private readonly IEnumerable<SubscriptionProfile> _profiles;

    /// <summary>
    /// The event mapper
    /// </summary>
    private readonly IEventMapper _eventMapper;
    
    /// <summary>
    /// Gets or sets the value of the error handlers
    /// </summary>
    private Dictionary<Type, List<Type>> ErrorHandlers { get; set; } = new Dictionary<Type, List<Type>>();

    /// <summary>
    /// The event handler executor
    /// </summary>
    private Dictionary<Type, List<IEventHandlerExecutor>> EventHandlerExecutors =
        new Dictionary<Type, List<IEventHandlerExecutor>>();
    
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriptionProfileManager"/> class
    /// </summary>
    /// <param name="profiles">The profiles</param>
    /// <param name="eventMapper">The event mapper</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public SubscriptionProfileManager(IEnumerable<SubscriptionProfile> profiles, 
                                      IEventMapper eventMapper)
    {
        this._profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
        this._eventMapper = eventMapper ?? throw new ArgumentNullException(nameof(eventMapper));
    }

    
    
    /// <summary>
    /// Aggregates subscription and error handler information from multiple profiles.
    /// </summary>
    public void Initialize()
    {
        foreach (var profile in _profiles)
        {
           
            foreach (var pair in profile.EventHandlerExecutors)
            {
                foreach (var handler in pair.Value)
                {
                    TryAddExecutors(EventHandlerExecutors, pair.Key, handler);
                }
            }
            
            foreach (var pair in profile.ErrorHandlers)
            {
                foreach (var handler in pair.Value)
                {
                    TryAddHandler(ErrorHandlers, pair.Key, handler);
                }
            }
            
            foreach (var @event in EventHandlerExecutors.Keys)
            {
                _eventMapper.Register(@event);
            }
        }
    }

    /// <summary>
    /// Attempts to add a handler to the specified dictionary.
    /// </summary>
    /// <param name="handlersDictionary">The dictionary of handlers</param>
    /// <param name="eventType">The event type</param>
    /// <param name="handlerType">The handler type</param>
    private void TryAddHandler(Dictionary<Type, List<Type>> handlersDictionary, Type eventType, Type handlerType)
    {
        if (!handlersDictionary.TryGetValue(eventType, out var handlersList))
        {
            handlersList = new List<Type>();
            handlersDictionary[eventType] = handlersList;
        }

        if (!handlersList.Contains(handlerType))
        {
            handlersList.Add(handlerType);
        }
    }

    /// <summary>
    /// Tries the add executors using the specified handlers dictionary
    /// </summary>
    /// <param name="handlersDictionary">The handlers dictionary</param>
    /// <param name="eventType">The event type</param>
    /// <param name="handlerType">The handler type</param>
    private void TryAddExecutors(Dictionary<Type, List<IEventHandlerExecutor>> handlersDictionary, Type eventType, IEventHandlerExecutor handlerType)
    {
        if (!handlersDictionary.TryGetValue(eventType, out var handlersList))
        {
            handlersList = new List<IEventHandlerExecutor>();
            handlersDictionary[eventType] = handlersList;
        }

        if (!handlersList.Contains(handlerType))
        {
            handlersList.Add(handlerType);
        }
    }

    /// <summary>
    /// Gets the all error handlers
    /// </summary>
    /// <returns>A dictionary of type and list type</returns>
    public Dictionary<Type, List<Type>> GetAllErrorHandlers()
    {
        return this.ErrorHandlers;
    }

    /// <summary>
    /// Hases the subscriptions for event using the specified event type
    /// </summary>
    /// <param name="eventType">The event type</param>
    /// <returns>The bool</returns>
    public bool HasSubscriptionsForEvent(Type eventType)
    {
        return this.EventHandlerExecutors.ContainsKey(eventType)|| this.EventHandlerExecutors.ContainsKey(eventType);
    }

    public List<Type> GetAllEventTypes()
    {
        return this.EventHandlerExecutors.Keys.ToList();
    }
    
    /// <summary>
    /// Gets the event handler executor for event using the specified event type
    /// </summary>
    /// <param name="eventType">The event type</param>
    /// <returns>A list of i event handler executor</returns>
    public List<IEventHandlerExecutor> GetEventHandlerExecutorsForEvent(Type eventType)
    {
        return this.EventHandlerExecutors[eventType];
    }

    /// <summary>
    /// Gets the error handlers for event using the specified event type
    /// </summary>
    /// <param name="eventType">The event type</param>
    /// <returns>A list of type</returns>
    public List<Type> GetErrorHandlersForEvent(Type eventType)
    {
        return this.ErrorHandlers[eventType];
    }
    
}

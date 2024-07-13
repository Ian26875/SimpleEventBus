namespace SimpleEventBus;

/// <summary>
/// The subscription profile class
/// </summary>
public abstract class SubscriptionProfile
{
    /// <summary>
    /// Gets or sets the value of the event types
    /// </summary>
    private List<Type> EventTypes { get; set; } = new List<Type>();

    /// <summary>
    /// Gets or sets the value of the handlers
    /// </summary>
    private Dictionary<Type, List<SubscriptionDescriptor>> Handlers { get; set; } = new();
    
    /// <summary>
    /// Creates the subscription using the specified event type
    /// </summary>
    /// <param name="eventType">The event type</param>
    /// <param name="handlerType">The handler type</param>
    /// <exception cref="ArgumentException">Event Handler Type '{handlerType.Namespace}' already registered for '{eventType}'</exception>
    public void CreateSubscription(Type eventType,Type handlerType) 
    {
        if (HasSubscriptionForEvent(eventType).Equals(false))
        {
            this.Handlers.Add(eventType,new List<SubscriptionDescriptor>());
        }

        if (this.Handlers[eventType].Any(s => s.EventHandlerType == handlerType))
        {
            throw new ArgumentException(
                $"Event Handler Type '{handlerType.Namespace}' already registered for '{eventType}'");
        }
        
        this.Handlers[eventType].Add(new SubscriptionDescriptor(eventType,handlerType));
    }

    /// <summary>
    /// Has the subscription for event using the specified event type
    /// </summary>
    /// <param name="eventType">The event type</param>
    /// <returns>The bool</returns>
    private bool HasSubscriptionForEvent(Type eventType)
    {
        return this.Handlers.ContainsKey(eventType);
    }

    public void CreateSubscription<TEvent, TEvenHandler>() where TEvent : class
        where TEvenHandler : IEventHandler<TEvent>
    {
        this.CreateSubscription(typeof(TEvent),typeof(TEvenHandler));
    }
}
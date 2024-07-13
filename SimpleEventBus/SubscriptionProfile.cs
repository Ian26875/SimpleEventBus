namespace SimpleEventBus;

public abstract class SubscriptionProfile
{
    private List<Type> EventTypes { get; set; } = new List<Type>();

    private Dictionary<Type, List<SubscriptionDescriptor>> Handlers { get; set; } =
        new Dictionary<Type, List<SubscriptionDescriptor>>();

    public void CreateSubscription<TEvent,TEventHandler>() where TEvent : class where TEventHandler : IEventHandler<TEvent>
    {
        this.CreateSubscription(typeof(TEvent),typeof(TEventHandler));
    }

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

    private bool HasSubscriptionForEvent(Type eventType)
    {
        return this.Handlers.ContainsKey(eventType);
    }
}
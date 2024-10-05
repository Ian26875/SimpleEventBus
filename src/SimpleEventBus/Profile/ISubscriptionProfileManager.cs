using SimpleEventBus.Subscriber.Executors;

namespace SimpleEventBus.Profile;

public interface ISubscriptionProfileManager
{
    public Dictionary<Type, List<Type>> GetAllEventHandlers();

    public Dictionary<Type, List<Type>> GetAllErrorHandlers();

    public bool HasSubscriptionsForEvent(Type eventType);

    public List<Type> GetAllEventTypes();

    public List<Type> GetEventHandlersForEvent(Type eventType);
    
    public List<IEventHandlerExecutor> GetEventHandlerExecutorForEvent(Type eventType);

    public List<Type> GetErrorHandlersForEvent(Type eventType);

}
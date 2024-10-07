using SimpleEventBus.Subscriber.Executors;

namespace SimpleEventBus.Profile;

public interface ISubscriptionProfileManager
{
    public bool HasSubscriptionsForEvent(Type eventType);

    public List<Type> GetAllEventTypes();
    
    public List<IEventHandlerExecutor> GetEventHandlerExecutorForEvent(Type eventType);

    public List<Type> GetErrorHandlersForEvent(Type eventType);
}
namespace SimpleEventBus;

internal interface ISubscriptionManager
{
    
    void LoadFromProfile<TProfile>(TProfile profile) where TProfile : SubscriptionProfile;

    IEnumerable<IEventHandler<TEvent>> GetHandlersForEvent<TEvent>(TEvent @event) where TEvent : class;
}
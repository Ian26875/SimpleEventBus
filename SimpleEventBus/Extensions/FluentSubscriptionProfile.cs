namespace SimpleEventBus;

public static class FluentSubscriptionProfile
{
    public static FluentSubscriptionLoadSpec<TEvent> When<TEvent>(this SubscriptionProfile subscriptionProfile) where TEvent : class
    {
        return new FluentSubscriptionLoadSpec<TEvent>(subscriptionProfile, typeof(TEvent));
    }
}

public class FluentSubscriptionLoadSpec<TEvent> where TEvent : class
{
    internal FluentSubscriptionLoadSpec(SubscriptionProfile subscriptionProfile, Type eventType)
    {
        CurrentProfile = subscriptionProfile;
        EventType = eventType;
    }

    public SubscriptionProfile CurrentProfile { get; }
    
    public Type EventType { get; }

    public FluentSubscriptionLoadSpec<TEvent> Do<THandler>() where THandler : IEventHandler<TEvent>
    {
        CurrentProfile.CreateSubscription<TEvent, THandler>();

        return this;
    }
}
namespace SimpleEventBus;

/// <summary>
/// The fluent subscription load spec class
/// </summary>
public class FluentSubscriptionBuilder<TEvent> : IFluentSubscriptionBuilder<TEvent> where TEvent : class
{
    internal FluentSubscriptionBuilder(SubscriptionProfile subscriptionProfile)
    {
        this.Profile = subscriptionProfile;
    }

    /// <summary>
    /// Gets the value of the current profile
    /// </summary>
    public SubscriptionProfile Profile { get; }
    
    
    public IFluentSubscriptionBuilder<TEvent> Do<TEventHandler>() where TEventHandler : IEventHandler<TEvent>
    {
        Profile.CreateSubscription(typeof(TEvent),typeof(TEventHandler));
        return this;
    }

    public IFluentSubscriptionBuilder<TEvent> IfExceptionDo<TErrorHandler>() where TErrorHandler : IErrorHandler
    {
        Profile.CreateErrorHandler(typeof(TEvent),typeof(TErrorHandler));
        return this;
    }
}
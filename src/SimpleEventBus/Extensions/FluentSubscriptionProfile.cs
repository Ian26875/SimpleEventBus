namespace SimpleEventBus;

/// <summary>
/// The fluent subscription profile class
/// </summary>
public static class FluentSubscriptionProfile
{
    /// <summary>
    /// Whens the subscription profile
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="subscriptionProfile">The subscription profile</param>
    /// <returns>A fluent subscription load spec of t event</returns>
    public static FluentSubscriptionLoadSpec<TEvent> When<TEvent>(this SubscriptionProfile subscriptionProfile) where TEvent : class
    {
        return new FluentSubscriptionLoadSpec<TEvent>(subscriptionProfile, typeof(TEvent));
    }
}

/// <summary>
/// The fluent subscription load spec class
/// </summary>
public class FluentSubscriptionLoadSpec<TEvent> where TEvent : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FluentSubscriptionLoadSpec{TEvent}"/> class
    /// </summary>
    /// <param name="subscriptionProfile">The subscription profile</param>
    /// <param name="eventType">The event type</param>
    internal FluentSubscriptionLoadSpec(SubscriptionProfile subscriptionProfile, Type eventType)
    {
        CurrentProfile = subscriptionProfile;
        EventType = eventType;
    }

    /// <summary>
    /// Gets the value of the current profile
    /// </summary>
    public SubscriptionProfile CurrentProfile { get; }
    
    /// <summary>
    /// Gets the value of the event type
    /// </summary>
    public Type EventType { get; }

    /// <summary>
    /// Does this instance
    /// </summary>
    /// <typeparam name="THandler">The handler</typeparam>
    /// <returns>A fluent subscription load spec of t event</returns>
    public FluentSubscriptionLoadSpec<TEvent> Do<THandler>() where THandler : IEventHandler<TEvent>
    {
        CurrentProfile.CreateSubscription<TEvent, THandler>();

        return this;
    }
}
using SimpleEventBus.Profile;

namespace SimpleEventBus;

/// <summary>
/// The fluent subscription profile class
/// </summary>
public static class FluentSubscriptionProfile
{
    /// <summary>
    /// When the subscription profile
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="subscriptionProfile">The subscription profile</param>
    /// <returns>A fluent subscription load spec of t event</returns>
    public static IFluentSubscriptionBuilder<TEvent> WhenOccurs<TEvent>(this SubscriptionProfile subscriptionProfile) where TEvent : class
    {
        return new FluentSubscriptionBuilder<TEvent>(subscriptionProfile);
    }
}
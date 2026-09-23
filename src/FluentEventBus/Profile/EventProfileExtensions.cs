using FluentEventBus.Profile;

namespace FluentEventBus;

/// <summary>
/// The fluent subscription profile class
/// </summary>
public static class EventProfileExtensions
{
    /// <summary>
    /// Whens the subscription profile
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="eventProfile">The subscription profile</param>
    /// <returns>A fluent subscription load spec of t event</returns>
    public static IFluentSubscriptionBuilder<TEvent> On<TEvent>(this EventProfile eventProfile) where TEvent : class
    {
        return new FluentSubscriptionBuilder<TEvent>(eventProfile);
    }

    /// <summary>
    /// Declares that this service publishes the specified event: it is registered in the
    /// name registry, appears as a send operation in generated documentation, and the
    /// returned builder mounts header enrichment policies applied at publish time.
    /// </summary>
    /// <typeparam name="TEvent">The published event type</typeparam>
    /// <param name="eventProfile">The event profile</param>
    /// <returns>A builder for correlation/message-id/header policies and doc examples</returns>
    public static IEventPublicationBuilder<TEvent> Publishes<TEvent>(this EventProfile eventProfile) where TEvent : class
    {
        return new EventPublicationBuilder<TEvent>(eventProfile.GetOrAddPublication(typeof(TEvent)));
    }
}
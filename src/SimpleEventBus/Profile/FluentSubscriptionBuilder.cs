using SimpleEventBus.Exceptions;
using SimpleEventBus.Subscriber;

namespace SimpleEventBus.Profile;

/// <summary>
/// The fluent subscription load spec class
/// </summary>
public class FluentSubscriptionBuilder<TEvent> : IFluentSubscriptionBuilder<TEvent> where TEvent : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FluentSubscriptionBuilder{TEvent}"/> class
    /// </summary>
    /// <param name="subscriptionProfile">The subscription profile</param>
    internal FluentSubscriptionBuilder(SubscriptionProfile subscriptionProfile)
    {
        this.Profile = subscriptionProfile;
    }

    /// <summary>
    /// Gets the value of the current profile
    /// </summary>
    public SubscriptionProfile Profile { get; }
    
    
    /// <summary>
    /// Does this instance
    /// </summary>
    /// <typeparam name="TEventHandler">The event handler</typeparam>
    /// <returns>A fluent subscription builder of t event</returns>
    public IFluentSubscriptionBuilder<TEvent> ToDo<TEventHandler>() where TEventHandler : IEventHandler<TEvent>
    {
        Profile.AddSubscription(typeof(TEvent),typeof(TEventHandler));
        return this;
    }

    /// <summary>
    /// Ifs the exception do
    /// </summary>
    /// <typeparam name="TErrorHandler">The error handler</typeparam>
    /// <returns>A fluent subscription builder of t event</returns>
    public IFluentSubscriptionBuilder<TEvent> CatchExceptionToDo<TErrorHandler>() where TErrorHandler : IHandlerExceptionHandler
    {
        Profile.AddErrorFilter(typeof(TEvent),typeof(TErrorHandler));
        return this;
    }
}
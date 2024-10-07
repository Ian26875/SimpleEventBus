using System.Linq.Expressions;
using SimpleEventBus.Event;
using SimpleEventBus.ExceptionHandlers;
using SimpleEventBus.Subscriber;
using SimpleEventBus.Subscriber.Executors;

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
    public FluentSubscriptionBuilder(SubscriptionProfile subscriptionProfile)
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
        Profile.AddSubscription(typeof(TEvent),new InterfaceEventHandlerExecutor<TEvent,TEventHandler>());
        return this;
    }

    /// <summary>
    /// Returns the do using the specified expression
    /// </summary>
    /// <typeparam name="THandler">The handler</typeparam>
    /// <param name="expression">The expression</param>
    /// <returns>A fluent subscription builder of t event</returns>
    public IFluentSubscriptionBuilder<TEvent> ToDo<THandler>(Expression<Func<THandler, Func<TEvent, Headers, CancellationToken, Task>>> expression) where THandler : class
    {
        Profile.AddSubscription(typeof(TEvent),new ExpressionEventHandlerExecutor<TEvent,THandler>(expression));
        return this;
    }

    /// <summary>
    /// Ifs the exception do
    /// </summary>
    /// <typeparam name="TErrorHandler">The error handler</typeparam>
    /// <returns>A fluent subscription builder of t event</returns>
    public IFluentSubscriptionBuilder<TEvent> CatchExceptionToDo<TErrorHandler>() where TErrorHandler : IEventExceptionHandler
    {
        Profile.AddErrorFilter(typeof(TEvent),typeof(TErrorHandler));
        return this;
    }
}
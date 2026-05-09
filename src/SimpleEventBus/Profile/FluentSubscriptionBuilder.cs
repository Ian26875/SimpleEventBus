using System.Linq.Expressions;
using SimpleEventBus.Event;
using SimpleEventBus.ExceptionHandlers;
using SimpleEventBus.Subscriber;
using SimpleEventBus.Subscriber.Executors;

namespace SimpleEventBus.Profile;

/// <summary>
/// Represents a fluent builder for configuring event subscriptions for a specific event type.
/// </summary>
/// <typeparam name="TEvent">The type of the event to be handled.</typeparam>
public class FluentSubscriptionBuilder<TEvent> : IFluentSubscriptionBuilder<TEvent> where TEvent : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FluentSubscriptionBuilder{TEvent}"/> class
    /// using the provided <see cref="SubscriptionProfile"/>.
    /// </summary>
<<<<<<< HEAD
    /// <param name="subscriptionProfile">The subscription profile</param>
    public FluentSubscriptionBuilder(SubscriptionProfile subscriptionProfile)
=======
    /// <param name="subscriptionProfile">The subscription profile that holds subscription configurations.</param>
    internal FluentSubscriptionBuilder(SubscriptionProfile subscriptionProfile)
>>>>>>> feature/BuildEventHandlerExecutor
    {
        this.Profile = subscriptionProfile;
    }

    /// <summary>
    /// Gets the current subscription profile used for this builder.
    /// </summary>
    public SubscriptionProfile Profile { get; }

    /// <summary>
    /// Registers a strongly-typed event handler for the current event type <typeparamref name="TEvent"/>.
    /// </summary>
    /// <typeparam name="TEventHandler">The type that implements <see cref="IEventHandler{TEvent}"/>.</typeparam>
    /// <returns>The fluent subscription builder instance for chaining.</returns>
    public IFluentSubscriptionBuilder<TEvent> ToDo<TEventHandler>() where TEventHandler : IEventHandler<TEvent>
    {
<<<<<<< HEAD
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
=======
        Profile.AddSubscription(typeof(TEvent), new InterfaceEventHandlerExecutor<TEvent, TEventHandler>());
>>>>>>> feature/BuildEventHandlerExecutor
        return this;
    }

    /// <summary>
    /// Registers a delegate-based event handler with full access to <see cref="Headers"/> and <see cref="CancellationToken"/>.
    /// </summary>
    /// <typeparam name="THandler">The type containing the delegate method.</typeparam>
    /// <param name="expression">An expression that points to the handler method.</param>
    /// <returns>The fluent subscription builder instance for chaining.</returns>
    public IFluentSubscriptionBuilder<TEvent> ToDo<THandler>(
        Expression<Func<THandler, Func<TEvent, Headers, CancellationToken, Task>>> expression) where THandler : class
    {
        ArgumentNullException.ThrowIfNull(expression);
        Profile.AddSubscription(typeof(TEvent), new LambdaEventHandlerExecutor<TEvent, THandler>(expression));
        return this;
    }

    /// <summary>
    /// Registers a simplified delegate-based event handler that does not require headers.
    /// </summary>
    /// <typeparam name="THandler">The type containing the delegate method.</typeparam>
    /// <param name="expression">An expression that points to the handler method.</param>
    /// <returns>The fluent subscription builder instance for chaining.</returns>
    public IFluentSubscriptionBuilder<TEvent> ToDo<THandler>(
        Expression<Func<THandler, Func<TEvent, CancellationToken, Task>>> expression) where THandler : class
    {
        ArgumentNullException.ThrowIfNull(expression);
        Profile.AddSubscription(typeof(TEvent), new LambdaEventHandlerExecutor<TEvent, THandler>(expression));
        return this;
    }

    /// <summary>
    /// Registers an exception handler that will be invoked if an error occurs during the event handling.
    /// </summary>
    /// <typeparam name="TErrorHandler">The type that implements <see cref="IEventExceptionHandler"/>.</typeparam>
    /// <returns>The fluent subscription builder instance for chaining.</returns>
    public IFluentSubscriptionBuilder<TEvent> CatchExceptionToDo<TErrorHandler>() where TErrorHandler : IEventExceptionHandler
    {
        Profile.AddErrorFilter(typeof(TEvent), typeof(TErrorHandler));
        return this;
    }
}

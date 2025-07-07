using System.Linq.Expressions;
using SimpleEventBus.Event;
using SimpleEventBus.ExceptionHandlers;
using SimpleEventBus.Subscriber;

namespace SimpleEventBus.Profile;
/// <summary>
/// Provides a fluent interface for building event subscriptions.
/// </summary>
public interface IFluentSubscriptionBuilder<TEvent> where TEvent : class
{
    /// <summary>
    /// Gets the current subscription profile configuration.
    /// </summary>
    SubscriptionProfile Profile { get; }

    /// <summary>
    /// Specifies the event handler type to handle the event.
    /// </summary>
    /// <typeparam name="TEventHandler">The type that handles the event.</typeparam>
    /// <returns>The fluent builder for chaining.</returns>
    IFluentSubscriptionBuilder<TEvent> ToDo<TEventHandler>() where TEventHandler : IEventHandler<TEvent>;

    /// <summary>
    /// Specifies a custom method on the handler to execute when the event is received.
    /// The method must have the signature: (TEvent, Headers, CancellationToken) => Task.
    /// </summary>
    /// <typeparam name="THandler">The type of the handler class.</typeparam>
    /// <param name="expression">An expression selecting the handler method.</param>
    /// <returns>The fluent builder for chaining.</returns>
    IFluentSubscriptionBuilder<TEvent> ToDo<THandler>(Expression<Func<THandler, Func<TEvent, Headers, CancellationToken, Task>>> expression) where THandler : class;

    /// <summary>
    /// Specifies a custom method on the handler to execute when the event is received.
    /// The method must have the signature: (TEvent, CancellationToken) => Task.
    /// </summary>
    /// <typeparam name="THandler">The type of the handler class.</typeparam>
    /// <param name="expression">An expression selecting the handler method.</param>
    /// <returns>The fluent builder for chaining.</returns>
    IFluentSubscriptionBuilder<TEvent> ToDo<THandler>(Expression<Func<THandler, Func<TEvent, CancellationToken, Task>>> expression) where THandler : class;

    /// <summary>
    /// Specifies the handler to use when an exception occurs during event processing.
    /// </summary>
    /// <typeparam name="TErrorHandler">The type that handles exceptions.</typeparam>
    /// <returns>The fluent builder for chaining.</returns>
    IFluentSubscriptionBuilder<TEvent> CatchExceptionToDo<TErrorHandler>() where TErrorHandler : IEventExceptionHandler;
}

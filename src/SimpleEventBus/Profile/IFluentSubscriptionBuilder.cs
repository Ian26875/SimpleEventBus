using System.Linq.Expressions;
using SimpleEventBus.Event;
using SimpleEventBus.ExceptionHandlers;
using SimpleEventBus.Subscriber;

namespace SimpleEventBus.Profile;

/// <summary>
/// The fluent subscription builder interface
/// </summary>
public interface IFluentSubscriptionBuilder<TEvent> where TEvent : class
{
    /// <summary>
    /// Gets the value of the current profile
    /// </summary>
    SubscriptionProfile Profile { get; }
    
    /// <summary>
    /// Does this instance
    /// </summary>
    /// <typeparam name="TEventHandler">The event handler</typeparam>
    /// <returns>A fluent subscription builder of t event</returns>
    IFluentSubscriptionBuilder<TEvent> ToDo<TEventHandler>() where TEventHandler : IEventHandler<TEvent>;

    /// <summary>
    /// Returns the do using the specified expression
    /// </summary>
    /// <typeparam name="THandler">The handler</typeparam>
    /// <param name="expression">The expression</param>
    /// <returns>A fluent subscription builder of t event</returns>
    IFluentSubscriptionBuilder<TEvent> ToDo<THandler>(Expression<Func<THandler, Func<TEvent, Headers, CancellationToken, Task>>> expression) where THandler : class;
    
    /// <summary>
    /// Ifs the exception do
    /// </summary>
    /// <typeparam name="TErrorHandler">The error handler</typeparam>
    /// <returns>A fluent subscription builder of t event</returns>
    IFluentSubscriptionBuilder<TEvent> CatchExceptionToDo<TErrorHandler>() where TErrorHandler : IEventExceptionHandler;
}
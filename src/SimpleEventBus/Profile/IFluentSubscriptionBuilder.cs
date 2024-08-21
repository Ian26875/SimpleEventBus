using SimpleEventBus.Exceptions;
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
    /// Ifs the exception do
    /// </summary>
    /// <typeparam name="TErrorHandler">The error handler</typeparam>
    /// <returns>A fluent subscription builder of t event</returns>
    IFluentSubscriptionBuilder<TEvent> CatchExceptionToDo<TErrorHandler>() where TErrorHandler : IErrorHandler;
}
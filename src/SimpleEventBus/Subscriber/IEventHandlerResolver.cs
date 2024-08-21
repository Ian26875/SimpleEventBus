using SimpleEventBus.Subscriber;

namespace SimpleEventBus;

/// <summary>
/// The event handler resolver interface
/// </summary>
public interface IEventHandlerResolver
{
    /// <summary>
    /// Gets the handlers for event using the specified event
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="event">The event</param>
    /// <returns>An enumerable of i event handler t event</returns>
    IEnumerable<IEventHandler<TEvent>> GetHandlersForEvent<TEvent>(TEvent @event) where TEvent : class;
}
namespace SimpleEventBus.Schema;

/// <summary>
/// The schema registry interface
/// </summary>
public interface ISchemaRegistry
{
    /// <summary>
    /// Registers the event type
    /// </summary>
    /// <param name="eventType">The event type</param>
    /// <exception cref="ArgumentException">Schema with name {eventType} and version {eventAttribute.Version} is already registered.</exception>
    /// <exception cref="ArgumentException">The event type {eventType.Name} does not have an EventAttribute.</exception>
    void Register(Type eventType);

    /// <summary>
    /// Gets this instance
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <returns>The string</returns>
    string Get<TEvent>() where TEvent : class;
}
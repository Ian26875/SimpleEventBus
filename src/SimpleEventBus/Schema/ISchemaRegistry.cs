namespace SimpleEventBus.Schema;

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
    /// Gets the name
    /// </summary>
    /// <param name="name">The name</param>
    /// <param name="version">The version</param>
    /// <exception cref="KeyNotFoundException">Schema with name {name} and version {version} not found.</exception>
    /// <returns>The type</returns>
    Type Get(string name, int version);
}
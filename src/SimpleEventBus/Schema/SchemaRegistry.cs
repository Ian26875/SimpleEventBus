using System.Collections.Concurrent;

namespace SimpleEventBus.Schema;

/// <summary>
/// The schema registry class
/// </summary>
public class SchemaRegistry : ISchemaRegistry
{
    /// <summary>
    /// The type
    /// </summary>
    private readonly ConcurrentDictionary<string, Type> _schemas = new ConcurrentDictionary<string, Type>();

    private static readonly Lazy<SchemaRegistry> _instance = new Lazy<SchemaRegistry>(() => new SchemaRegistry());
    
    private SchemaRegistry() { }

    public static SchemaRegistry Instance => _instance.Value;
    
    /// <summary>
    /// Registers the event type
    /// </summary>
    /// <param name="eventType">The event type</param>
    /// <exception cref="ArgumentException">Schema with name {eventType} and version {eventAttribute.Version} is already registered.</exception>
    /// <exception cref="ArgumentException">The event type {eventType.Name} does not have an EventAttribute.</exception>
    public void Register(Type eventType)
    {
        var eventAttribute = eventType.GetAttribute<EventVersionAttribute>();
        if (eventAttribute is null)
        {
            throw new ArgumentException($"The event type {eventType.Name} does not have an EventAttribute.");
        }

        var key = $"{eventType.Name}_v{eventAttribute.Version}";
        if (_schemas.TryAdd(key, eventType).Equals(false))
        {
            throw new ArgumentException($"Schema with name {eventType} and version {eventAttribute.Version} is already registered.");
        }
    }
    
    /// <summary>
    /// Gets the name
    /// </summary>
    /// <param name="name">The name</param>
    /// <param name="version">The version</param>
    /// <exception cref="KeyNotFoundException">Schema with name {name} and version {version} not found.</exception>
    /// <returns>The type</returns>
    public Type Get(string name, int version)
    {
        var key = $"{name}_v{version}";
        if (_schemas.TryGetValue(key, out var eventType))
        {
            return eventType;
        }
        throw new KeyNotFoundException($"Schema with name {name} and version {version} not found.");
    }
}
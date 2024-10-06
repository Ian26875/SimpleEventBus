using System.Collections.Concurrent;
using System.Reflection;

namespace SimpleEventBus.Schema;

/// <summary>
/// The schema registry class
/// </summary>
/// <seealso cref="ISchemaRegistry"/>
public class SchemaRegistry : ISchemaRegistry
{
    /// <summary>
    /// The schema registry
    /// </summary>
    private static readonly Lazy<SchemaRegistry> _instance = new Lazy<SchemaRegistry>(() => new SchemaRegistry());
    
    /// <summary>
    /// The type
    /// </summary>
    private readonly ConcurrentDictionary<Type, string> _schemas = new ConcurrentDictionary<Type, string>();
    
    /// <summary>
    /// The type
    /// </summary>
    private readonly ConcurrentDictionary<string, Type> _typesByName = new ConcurrentDictionary<string, Type>();

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaRegistry"/> class
    /// </summary>
    private SchemaRegistry()
    {
    }

    /// <summary>
    /// Gets the value of the instance
    /// </summary>
    public static SchemaRegistry Instance => _instance.Value;

    /// <summary>
    /// Registers the event type
    /// </summary>
    /// <param name="eventType">The event type</param>
    /// <exception cref="ArgumentException">Failed to register type mapping for {value}.</exception>
    /// <exception cref="ArgumentException">Schema with name {eventType} and version {eventAttribute.Version} is already registered.</exception>
    public void Register(Type eventType)
    {
        string eventName;
        string eventVersion;
        var eventAttribute = eventType.GetCustomAttribute<EventAttribute>();
        if (eventAttribute is null)
        {
            eventName = eventType.Name;
            eventVersion = "1";
        }
        else
        {
            eventName = string.IsNullOrWhiteSpace(eventAttribute.Name) ? eventType.Name : eventAttribute.Name;
            eventVersion = string.IsNullOrWhiteSpace(eventAttribute.Version) ? "1" : eventAttribute.Version;
        }

        var value = $"{eventName}_v{eventVersion}";

        if (_schemas.TryAdd(eventType, value).Equals(false))
        {
            throw new ArgumentException(
                $"Schema with name {eventType} and version {eventAttribute.Version} is already registered.");
        }

        if (!_typesByName.TryAdd(value, eventType))
        {
            throw new ArgumentException($"Failed to register type mapping for {value}.");
        }
    }

    /// <summary>
    /// Gets the event name using the specified type
    /// </summary>
    /// <param name="type">The type</param>
    /// <exception cref="KeyNotFoundException">No schema registered for type {type.Name}.</exception>
    /// <returns>The string</returns>
    public string GetEventName(Type type)
    {
        if (_schemas.TryGetValue(type, out var eventName))
        {
            return eventName;
        }
        
        throw new KeyNotFoundException($"No schema registered for type {type.Name}.");
    }

    /// <summary>
    /// Gets the event type using the specified event name
    /// </summary>
    /// <param name="eventName">The event name</param>
    /// <exception cref="KeyNotFoundException">No type registered for event name {eventName}.</exception>
    /// <returns>The type</returns>
    public Type GetEventType(string eventName)
    {
        if (_typesByName.TryGetValue(eventName, out var type))
        {
            return type;
        }

        throw new KeyNotFoundException($"No type registered for event name {eventName}.");
    }
}
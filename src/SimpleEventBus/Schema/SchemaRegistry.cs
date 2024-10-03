using System.Collections.Concurrent;
using System.Reflection;

namespace SimpleEventBus.Schema;

public class SchemaRegistry : ISchemaRegistry
{
    private readonly ConcurrentDictionary<Type, string> _schemas = new ConcurrentDictionary<Type, string>();
    private readonly ConcurrentDictionary<string, Type> _typesByName = new ConcurrentDictionary<string, Type>();

    private static readonly Lazy<SchemaRegistry> _instance = new Lazy<SchemaRegistry>(() => new SchemaRegistry());

    private SchemaRegistry() { }

    public static SchemaRegistry Instance => _instance.Value;
    
    public void Register(Type eventType)
    {
        var eventAttribute = eventType.GetCustomAttribute<EventAttribute>();
        if (eventAttribute == null)
        {
            throw new ArgumentException($"The event type {eventType.Name} does not have an EventAttribute.");
        }

        var eventName = string.IsNullOrWhiteSpace(eventAttribute.Name) ? eventType.Name : eventAttribute.Name;
        var value = $"{eventName}_v{eventAttribute.Version}";

        if (!_schemas.TryAdd(eventType, value))
        {
            throw new ArgumentException($"Schema with name {eventType} and version {eventAttribute.Version} is already registered.");
        }

        if (!_typesByName.TryAdd(value, eventType))
        {
            throw new ArgumentException($"Failed to register type mapping for {value}.");
        }
    }

    public string GetEventName(Type type)
    {
        if (_schemas.TryGetValue(type, out var eventName))
        {
            return eventName;
        }
        throw new KeyNotFoundException($"No schema registered for type {type.Name}.");
    }

    public Type GetEventType(string eventName)
    {
        if (_typesByName.TryGetValue(eventName, out var type))
        {
            return type;
        }
        throw new KeyNotFoundException($"No type registered for event name {eventName}.");
    }
}

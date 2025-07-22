using System.Collections.Concurrent;
using System.Reflection;
using SimpleEventBus.Schema;

namespace SimpleEventBus.Mapper;
/// <summary>
/// The schema registry class
/// </summary>
public class EventMapper : IEventMapper
{
    private static readonly Lazy<EventMapper> _instance = new(() => new EventMapper());

    private readonly ConcurrentDictionary<Type, string> _schemas = new();
    private readonly ConcurrentDictionary<string, Type> _typesByName = new();

    private EventMapper() { }

    /// <summary>
    /// Singleton instance
    /// </summary>
    public static EventMapper Instance => _instance.Value;

    /// <summary>
    /// Registers an event type and its versioned schema name.
    /// </summary>
    public void Register(Type eventType)
    {
        if (eventType == null) throw new ArgumentNullException(nameof(eventType));

        var schemaName = GetEventKey(eventType);

        if (!_schemas.TryAdd(eventType, schemaName))
            throw new ArgumentException($"Schema already registered for {eventType.FullName}.");

        if (!_typesByName.TryAdd(schemaName, eventType))
            throw new ArgumentException($"Schema name '{schemaName}' is already mapped to another type.");
    }

    /// <summary>
    /// Gets the versioned event name for the given type.
    /// </summary>
    public string GetEventName(Type type)
    {
        return _schemas.TryGetValue(type, out var name) ? name : GetEventKey(type);
    }

    /// <summary>
    /// Gets the event type mapped to the given name.
    /// </summary>
    public Type GetEventType(string eventName)
    {
        if (_typesByName.TryGetValue(eventName, out var type))
            return type;

        throw new KeyNotFoundException($"No type registered for event name '{eventName}'.");
    }

    /// <summary>
    /// Builds a unique key for an event type including version.
    /// </summary>
    private static string GetEventKey(Type type)
    {
        var attr = type.GetCustomAttribute<EventAttribute>();
        var name = string.IsNullOrWhiteSpace(attr?.Name) ? type.Name : attr.Name;
        var version = string.IsNullOrWhiteSpace(attr?.Version) ? "1" : attr.Version;
        return $"{name}_v{version}";
    }
}

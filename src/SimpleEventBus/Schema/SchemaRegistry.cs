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
    private readonly ConcurrentDictionary<Type, string> _schemas = new ConcurrentDictionary<Type, string>();
    
    
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

        var value = $"{eventType.Name}_v{eventAttribute.Version}";
        if (_schemas.TryAdd(eventType,value ).Equals(false))
        {
            throw new ArgumentException($"Schema with name {eventType} and version {eventAttribute.Version} is already registered.");
        }
    }

    public string Get<TEvent>() where TEvent : class
    {
        var eventType = typeof(TEvent);
        return this._schemas.TryGetValue(eventType, out var value) ? value : eventType.Name;
    }
}
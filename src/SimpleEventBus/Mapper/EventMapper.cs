using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Linq;
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
    private readonly object _sync = new();

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

        lock (_sync)
        {
            if (_schemas.TryGetValue(eventType, out var existingSchema))
            {
                if (!string.Equals(existingSchema, schemaName, StringComparison.Ordinal))
                {
                    throw new ArgumentException($"Schema already registered for {eventType.FullName}.");
                }

                return;
            }

            if (_typesByName.TryGetValue(schemaName, out var existingType))
            {
                if (existingType != eventType)
                {
                    throw new ArgumentException($"Schema name '{schemaName}' is already mapped to another type.");
                }

                _schemas.TryAdd(eventType, schemaName);
                return;
            }

            _schemas.TryAdd(eventType, schemaName);
            _typesByName.TryAdd(schemaName, eventType);
        }
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
        var version = string.IsNullOrWhiteSpace(attr?.Version) ? "1" : attr.Version;
        var baseName = !string.IsNullOrWhiteSpace(attr?.Name) && attr.Name.Contains('.')
            ? attr.Name
            : BuildConventionalName(type);

        return $"{baseName}.v{version}";
    }

    private static string BuildConventionalName(Type type)
    {
        var domain = type.Namespace?.Split('.').LastOrDefault();
        if (string.IsNullOrWhiteSpace(domain))
        {
            domain = "app";
        }
        domain = domain.ToLowerInvariant();

        var tokens = SplitPascal(type.Name);
        if (tokens.Count == 0)
        {
            return $"{domain}.event.unknown";
        }

        if (tokens.Count == 1)
        {
            return $"{domain}.{tokens[0].ToLowerInvariant()}.event";
        }

        var action = tokens[^1].ToLowerInvariant();
        var entity = string.Join(".", tokens.Take(tokens.Count - 1)).ToLowerInvariant();

        return $"{domain}.{entity}.{action}";
    }

    private static List<string> SplitPascal(string name)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(name))
        {
            return result;
        }

        var start = 0;
        for (var i = 1; i < name.Length; i++)
        {
            var prev = name[i - 1];
            var curr = name[i];
            var next = i + 1 < name.Length ? name[i + 1] : '\0';

            var boundary = false;

            if (char.IsDigit(prev) && !char.IsDigit(curr))
            {
                boundary = true;
            }
            else if (!char.IsDigit(prev) && char.IsDigit(curr))
            {
                boundary = true;
            }
            else if (char.IsUpper(curr))
            {
                if (char.IsLower(prev))
                {
                    boundary = true;
                }
                else if (char.IsUpper(prev) && next != '\0' && char.IsLower(next))
                {
                    boundary = true;
                }
            }

            if (boundary)
            {
                result.Add(name.Substring(start, i - start));
                start = i;
            }
        }

        result.Add(name.Substring(start));

        return result;
    }
}

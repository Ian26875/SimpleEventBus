﻿namespace SimpleEventBus.Profile;

/// <summary>
/// Aggregates multiple subscription profiles into a single cohesive unit.
/// </summary>
public class SubscriptionProfileManager : ISubscriptionProfileManager
{
    private readonly IEnumerable<SubscriptionProfile> _profiles;

    private Dictionary<Type, List<Type>> EventHandlers { get; set; }

    
    private Dictionary<Type, List<Type>> ErrorHandlers { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriptionProfileManager"/> class
    /// </summary>
    /// <param name="profiles">Collection of subscription profiles to aggregate</param>
    public SubscriptionProfileManager(IEnumerable<SubscriptionProfile> profiles)
    {
        _profiles = profiles ?? throw new ArgumentNullException(nameof(profiles));
        AggregateProfiles();
    }

    /// <summary>
    /// Aggregates subscription and error handler information from multiple profiles.
    /// </summary>
    private void AggregateProfiles()
    {
        foreach (var profile in _profiles)
        {
            foreach (var pair in profile.EventHandlers)
            {
                foreach (var handler in pair.Value)
                {
                    TryAddHandler(EventHandlers, pair.Key, handler);
                }
            }
            foreach (var pair in profile.ErrorHandlers)
            {
                foreach (var handler in pair.Value)
                {
                    TryAddHandler(ErrorHandlers, pair.Key, handler);
                }
            }
        }
    }

    /// <summary>
    /// Attempts to add a handler to the specified dictionary.
    /// </summary>
    /// <param name="handlersDictionary">The dictionary of handlers</param>
    /// <param name="eventType">The event type</param>
    /// <param name="handlerType">The handler type</param>
    private void TryAddHandler(Dictionary<Type, List<Type>> handlersDictionary, Type eventType, Type handlerType)
    {
        if (!handlersDictionary.TryGetValue(eventType, out var handlersList))
        {
            handlersList = new List<Type>();
            handlersDictionary[eventType] = handlersList;
        }

        if (!handlersList.Contains(handlerType))
        {
            handlersList.Add(handlerType);
        }
    }

    public Dictionary<Type, List<Type>> GetAllEventHandlers()
    {
        return this.EventHandlers;
    }

    public Dictionary<Type, List<Type>> GetAllErrorHandlers()
    {
        return this.ErrorHandlers;
    }

    public bool HasSubscriptionsForEvent(Type eventType)
    {
        return this.EventHandlers.ContainsKey(eventType);
    }

    public List<Type> GetAllEventTypes()
    {
        return this.EventHandlers.Keys.ToList();
    }
}
namespace SimpleEventBus;

/// <summary>
/// The base class for subscription profiles.
/// </summary>
public abstract class SubscriptionProfile
{
    /// <summary>
    /// Maps event types to their list of handler types.
    /// </summary>
    internal Dictionary<Type, List<Type>> Handlers { get; } = new();

    /// <summary>
    /// Registers a subscription for a specific event and handler type.
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <typeparam name="THandler">The handler type.</typeparam>
    /// <exception cref="ArgumentException">Thrown if the handler is already registered for the event.</exception>
    public void CreateSubscription<TEvent, THandler>() where TEvent : class where THandler : IEventHandler<TEvent>
    {
        var eventType = typeof(TEvent);
        var handlerType = typeof(THandler);

        if (Handlers.TryGetValue(eventType, out var handlersList).Equals(false))
        {
            handlersList = new List<Type>();
            Handlers[eventType] = handlersList;
        }

        if (handlersList.Contains(handlerType))
        {
            throw new ArgumentException($"Handler type '{handlerType.FullName}' is already registered for event type '{eventType.FullName}'.");
        }

        handlersList.Add(handlerType);
    }
}

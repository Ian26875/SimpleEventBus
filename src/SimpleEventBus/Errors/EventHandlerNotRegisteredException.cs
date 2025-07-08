namespace SimpleEventBus.Errors;

/// <summary>
/// Represents an exception thrown when a handler is not registered for a specific event type.
/// </summary>
public class HandlerNotRegisteredException : Exception
{
    public HandlerNotRegisteredException(Type eventType, Type handlerType)
        : base($"Handler of type '{handlerType.FullName}' is not registered for event type '{eventType.FullName}'. " +
               "Ensure you have registered the handler in your service container.")
    {
        EventType = eventType;
        HandlerType = handlerType;
    }

    public Type EventType { get; }
    public Type HandlerType { get; }
}
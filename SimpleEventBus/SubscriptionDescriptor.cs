using System.Diagnostics;

namespace SimpleEventBus;

[DebuggerDisplay("EventType={EventType}，EventHandlerType={EventHandlerType}")]
public class SubscriptionDescriptor
{
    public Type EventType { get; }

    public Type EventHandlerType { get; }
    

    public SubscriptionDescriptor(Type eventType, Type eventHandlerType)
    {
        EventType = eventType;
        EventHandlerType = eventHandlerType;
    }
}
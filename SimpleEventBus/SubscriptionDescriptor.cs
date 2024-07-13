using System.Diagnostics;

namespace SimpleEventBus;

[DebuggerDisplay("EventType={EventType}，EventHandlerType={EventHandlerType}")]
public record SubscriptionDescriptor(Type EventType, Type EventHandlerType);
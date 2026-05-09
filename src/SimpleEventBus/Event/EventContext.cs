namespace SimpleEventBus.Event;

/// <summary>
/// Represents the context of an event being transmitted or processed,
/// including the raw event data, optional metadata headers, and the event name.
/// </summary>
/// <param name="Data">The raw serialized event payload.</param>
/// <param name="Headers">Optional metadata or routing information associated with the event.</param>
/// <param name="EventName">The logical name of the event, typically used for routing or deserialization.</param>
public record EventContext(ReadOnlyMemory<byte> Data, Headers Headers, string EventName);

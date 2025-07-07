namespace SimpleEventBus.Event;

public record EventContext(ReadOnlyMemory<byte> Data, Headers Headers, string EventName);
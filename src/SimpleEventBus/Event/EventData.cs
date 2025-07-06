namespace SimpleEventBus.Event;

public record EventData(ReadOnlyMemory<byte> Data, Headers Headers, string EventName);
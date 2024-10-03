namespace SimpleEventBus.Event;

public class EventData
{
    public ReadOnlyMemory<byte> Data { get; }
    public Headers Headers { get; }
    public string EventName { get; }

    public EventData(ReadOnlyMemory<byte> data, Headers headers, string eventName)
    {
        Data = data;
        Headers = headers;
        EventName = eventName;
    }
}
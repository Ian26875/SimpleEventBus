namespace SimpleEventBus.Event;

/// <summary>
/// The event data class
/// </summary>
public class EventData
{
    /// <summary>
    /// Gets the value of the data
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; }
    
    /// <summary>
    /// Gets the value of the headers
    /// </summary>
    public Headers Headers { get; }
    
    /// <summary>
    /// Gets the value of the event name
    /// </summary>
    public string EventName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventData"/> class
    /// </summary>
    /// <param name="data">The data</param>
    /// <param name="headers">The headers</param>
    /// <param name="eventName">The event name</param>
    public EventData(ReadOnlyMemory<byte> data, Headers headers, string eventName)
    {
        Data = data;
        Headers = headers;
        EventName = eventName;
    }
}
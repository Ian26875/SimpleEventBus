namespace SimpleEventBus;

/// <summary>
/// The event context class
/// </summary>
public class EventContext<TEvent> where TEvent : class
{
    /// <summary>
    /// Sets or gets the value of the event
    /// </summary>
    public TEvent Event { protected set; get; }

    /// <summary>
    /// Sets or gets the value of the event type
    /// </summary>
    public Type EventType { protected set; get; }

    /// <summary>
    /// Sets or gets the value of the event version
    /// </summary>
    public string EventVersion { protected set; get; }

    /// <summary>
    /// Sets or gets the value of the headers
    /// </summary>
    public Headers Headers { protected set; get; }

    protected EventContext(TEvent @event, Type eventType, string eventVersion, Headers headers)
    {
        Event = @event;
        EventType = eventType;
        EventVersion = eventVersion;
        Headers = headers;
    }

    /// <summary>
    /// Creates the event
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="@event">The event</param>
    /// <param name="headers">The headers</param>
    /// <returns>An event context of t event</returns>
    public static EventContext<TEvent> Create<TEvent>(TEvent @event, Headers headers) where TEvent : class
    {
        var eventType = typeof(TEvent);
        var version = eventType.GetEventVersion();

        var eventVersion = string.IsNullOrWhiteSpace(version) ? "1.0" : version;

        return new EventContext<TEvent>(@event,eventType,eventVersion,headers);
    }
}
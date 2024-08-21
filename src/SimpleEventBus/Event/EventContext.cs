namespace SimpleEventBus.Event;

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
    /// Sets or gets the value of the headers
    /// </summary>
    public Headers Headers { protected set; get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="EventContext{TEvent}"/> class
    /// </summary>
    /// <param name="event">The event</param>
    /// <param name="headers">The headers</param>
    internal EventContext(TEvent @event, Headers headers)
    {
        Event = @event;
        EventType = typeof(TEvent);
        Headers = headers;
    }
}
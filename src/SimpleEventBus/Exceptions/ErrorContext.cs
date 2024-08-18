using System.Runtime.ExceptionServices;

namespace SimpleEventBus;

/// <summary>
/// The exception context class
/// </summary>
/// <seealso cref="EventContext{object}"/>
public class ErrorContext : EventContext<object>
{
    /// <summary>
    /// Gets or sets the value of the exception
    /// </summary>
    public Exception Exception { get; private set; }

    /// <summary>
    /// Gets or sets the value of the exception dispatch
    /// </summary>
    public ExceptionDispatchInfo ExceptionDispatch { get; private set;}
    
    /// <summary>
    /// Creates the exception
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="exception">The exception</param>
    /// <param name="event">The event</param>
    /// <param name="headers">The headers</param>
    /// <returns>The exception context</returns>
    public static ErrorContext Create<TEvent>(Exception exception,TEvent @event, Headers headers) where TEvent : class
    {
        var eventType = typeof(TEvent);
        var version = eventType.GetEventVersion();

        var eventVersion = string.IsNullOrWhiteSpace(version) ? "1.0" : version;

        return new ErrorContext(@event,headers)
        {
            Exception = exception,
            ExceptionDispatch = ExceptionDispatchInfo.Capture(exception)
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorContext"/> class
    /// </summary>
    /// <param name="@event">The event</param>
    /// <param name="eventType">The event type</param>
    /// <param name="eventVersion">The event version</param>
    /// <param name="headers">The headers</param>
    protected ErrorContext(object @event, Headers headers) 
        : base(@event, headers)
    {
    }
}
using System.Runtime.ExceptionServices;
using SimpleEventBus.Event;

namespace SimpleEventBus.Exceptions;

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
    /// Initializes a new instance of the <see cref="ErrorContext"/> class
    /// </summary>
    /// <param name="event">The event</param>
    /// <param name="headers">The headers</param>
    /// <param name="exception">The exception</param>
    internal ErrorContext(object @event, Headers headers,Exception exception) : base(@event, headers)
    {
        Exception = exception;
        ExceptionDispatch = ExceptionDispatchInfo.Capture(exception);
    }
}
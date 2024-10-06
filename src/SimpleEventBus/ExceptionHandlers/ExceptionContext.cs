using System.Runtime.ExceptionServices;
using System.Text;
using SimpleEventBus.Event;

namespace SimpleEventBus.ExceptionHandlers;

/// <summary>
/// The exception context class
/// </summary>
/// <seealso cref="EventContext{object}"/>
public class ExceptionContext 
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
    /// Gets or sets the value of the event
    /// </summary>
    public object Event { get; private set; }

    /// <summary>
    /// Gets or sets the value of the headers
    /// </summary>
    public Headers Headers { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionContext"/> class
    /// </summary>
    /// <param name="event">The event</param>
    /// <param name="headers">The headers</param>
    /// <param name="exception">The exception</param>
    internal ExceptionContext(object @event, Headers headers,Exception exception)
    {
        Event = @event;
        Headers = headers;
        Exception = exception;
        ExceptionDispatch = ExceptionDispatchInfo.Capture(exception);
    }
}
using System.Runtime.ExceptionServices;
using SimpleEventBus.Event;

namespace SimpleEventBus.ExceptionHandlers;

/// <summary>
/// Represents the context of an exception that occurred during event handling.
/// Includes the original event, its metadata headers, and captured exception details.
/// </summary>
public class ExceptionContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionContext"/> class.
    /// </summary>
    /// <param name="event">The original event object that caused the exception.</param>
    /// <param name="headers">The metadata headers associated with the event.</param>
    /// <param name="exception">The exception that was thrown during event processing.</param>
    internal ExceptionContext(object @event, Headers headers, Exception exception)
    {
        Event = @event;
        Headers = headers;
        Exception = exception;
        ExceptionDispatch = ExceptionDispatchInfo.Capture(exception);
    }

    /// <summary>
    /// Gets the exception that was thrown.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Gets the captured exception dispatch information, which preserves the stack trace.
    /// </summary>
    public ExceptionDispatchInfo ExceptionDispatch { get; }

    /// <summary>
    /// Gets the original event instance that triggered the exception.
    /// </summary>
    public object Event { get; }

    /// <summary>
    /// Gets the headers associated with the event.
    /// </summary>
    public Headers Headers { get; }
}

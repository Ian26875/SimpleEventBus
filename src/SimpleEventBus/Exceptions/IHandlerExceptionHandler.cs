namespace SimpleEventBus.Exceptions;

/// <summary>
/// The error handler interface
/// </summary>
public interface IHandlerExceptionHandler
{
    /// <summary>
    /// Ons the exception using the specified exception context
    /// </summary>
    /// <param name="exceptionContext">The exception context</param>
    void OnException(ExceptionContext exceptionContext);
}
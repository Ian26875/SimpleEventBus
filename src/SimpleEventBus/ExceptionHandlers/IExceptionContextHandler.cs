namespace SimpleEventBus.ExceptionHandlers;

/// <summary>
/// The exception context handler interface
/// </summary>
public interface IExceptionContextHandler
{
    /// <summary>
    /// Invokes the exception context
    /// </summary>
    /// <param name="exceptionContext">The exception context</param>
    Task InvokeAsync(ExceptionContext exceptionContext);
}
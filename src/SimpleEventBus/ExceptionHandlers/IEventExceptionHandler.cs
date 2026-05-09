namespace SimpleEventBus.ExceptionHandlers;

/// <summary>
///     The error handler interface
/// </summary>
public interface IEventExceptionHandler
{
    /// <summary>
    ///     Ons the exception using the specified exception context
    /// </summary>
    /// <param name="exceptionContext">The exception context</param>
    /// <param name="cancellationToken">The cancellation token</param>
    Task OnExceptionAsync(ExceptionContext exceptionContext, CancellationToken cancellationToken);
}
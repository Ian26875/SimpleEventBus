namespace SimpleEventBus;

/// <summary>
/// The error handler interface
/// </summary>
public interface IErrorHandler
{
    /// <summary>
    /// Ons the exception using the specified exception context
    /// </summary>
    /// <param name="errorContext">The exception context</param>
    void OnException(ErrorContext errorContext);
}
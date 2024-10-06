namespace SimpleEventBus.ExceptionHandlers;

/// <summary>
/// The exception handler pipeline builder interface
/// </summary>
public interface IExceptionHandlerPipelineBuilder
{
    /// <summary>
    /// Uses the exception handler
    /// </summary>
    /// <param name="exceptionHandler">The exception handler</param>
    /// <returns>The exception handler pipeline builder</returns>
    IExceptionHandlerPipelineBuilder Use(Func<ExceptionContextHandlerDelegate, ExceptionContextHandlerDelegate> exceptionHandler);

    /// <summary>
    /// Uses this instance
    /// </summary>
    /// <typeparam name="TExceptionContextHandler">The exception context handler</typeparam>
    /// <returns>The exception handler pipeline builder</returns>
    IExceptionHandlerPipelineBuilder Use<TExceptionContextHandler>()
        where TExceptionContextHandler : IExceptionContextHandler;
    
    /// <summary>
    /// Builds this instance
    /// </summary>
    /// <returns>The exception context handler delegate</returns>
    ExceptionContextHandlerDelegate Build();
}
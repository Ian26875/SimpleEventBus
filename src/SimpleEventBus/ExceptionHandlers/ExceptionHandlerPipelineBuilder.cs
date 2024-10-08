using Microsoft.Extensions.DependencyInjection;

namespace SimpleEventBus.ExceptionHandlers;

/// <summary>
/// The exception handler pipeline builder class
/// </summary>
/// <seealso cref="IExceptionHandlerPipelineBuilder"/>
public class ExceptionHandlerPipelineBuilder : IExceptionHandlerPipelineBuilder
{
    /// <summary>
    /// The components
    /// </summary>
    private readonly List<Func<ExceptionContextHandlerDelegate, ExceptionContextHandlerDelegate>> _components = new();
    
    /// <summary>
    /// Uses the exception handler
    /// </summary>
    /// <param name="exceptionHandler">The exception handler</param>
    /// <returns>The exception handler pipeline builder</returns>
    public IExceptionHandlerPipelineBuilder Use(Func<ExceptionContextHandlerDelegate, ExceptionContextHandlerDelegate> exceptionHandler)
    {
        _components.Add(exceptionHandler);
        return this;
    }
    
    /// <summary>
    /// Builds this instance
    /// </summary>
    /// <returns>The exception context handler delegate</returns>
    public ExceptionContextHandlerDelegate Build()
    {
        ExceptionContextHandlerDelegate pipeline = context => Task.CompletedTask;

        return _components.AsEnumerable()
                          .Reverse()
                          .Aggregate
                          (
                              pipeline, 
                              (current, component) => component(current)
                          );
    }
}

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
    /// The service provider
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlerPipelineBuilder"/> class
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    public ExceptionHandlerPipelineBuilder(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

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
    /// Uses this instance
    /// </summary>
    /// <typeparam name="TExceptionContextHandler">The exception context handler</typeparam>
    /// <returns>The exception handler pipeline builder</returns>
    public IExceptionHandlerPipelineBuilder Use<TExceptionContextHandler>() where TExceptionContextHandler : IExceptionContextHandler
    {
        _components.Add(next => async context =>
        {
            var handler = (TExceptionContextHandler)_serviceProvider.GetRequiredService(typeof(TExceptionContextHandler));
            
            await handler.InvokeAsync(context);
            
            await next(context);
        });

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

using Microsoft.Extensions.DependencyInjection;

namespace SimpleEventBus.ExceptionHandlers;

public class ExceptionHandlerPipelineBuilder : IExceptionHandlerPipelineBuilder
{
    private readonly List<Func<ExceptionContextHandlerDelegate, ExceptionContextHandlerDelegate>> _components = new();

    private readonly IServiceProvider _serviceProvider;

    public ExceptionHandlerPipelineBuilder(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IExceptionHandlerPipelineBuilder Use(Func<ExceptionContextHandlerDelegate, ExceptionContextHandlerDelegate> exceptionHandler)
    {
        _components.Add(exceptionHandler);
        return this;
    }

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

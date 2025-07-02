namespace SimpleEventBus.ExceptionHandlers;

public interface IExceptionHandlerPipeline
{
    Task ExecuteAsync(ExceptionContext context, CancellationToken cancellationToken);
}
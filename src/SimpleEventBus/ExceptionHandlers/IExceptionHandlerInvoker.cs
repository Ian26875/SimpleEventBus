namespace SimpleEventBus.ExceptionHandlers;

public interface IExceptionHandlerInvoker
{
    Task ExecuteAsync(ExceptionContext context, CancellationToken cancellationToken);
}
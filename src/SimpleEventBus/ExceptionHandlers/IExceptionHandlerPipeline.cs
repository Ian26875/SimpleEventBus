namespace SimpleEventBus.ExceptionHandlers;

public interface IExceptionHandlerPipeline
{
    void Execute(ExceptionContext context);
}
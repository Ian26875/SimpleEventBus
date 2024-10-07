using SimpleEventBus.ExceptionHandlers;

namespace SimpleEventBus.Tests;

public class TestErrorHandler : IEventExceptionHandler
{
    public ExceptionContext ExceptionContext { get; private set; }


    public void OnException(ExceptionContext exceptionContext)
    {
        ExceptionContext = exceptionContext;
    }
}
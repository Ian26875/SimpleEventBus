namespace SimpleEventBus.Tests;

public class ExpressionTestEventHandler
{
    public TestEvent TestEvent { get; private set; }


    public IDictionary<string, object> Headers { get; private set; }

    public Task HandleAsync(TestEvent @event, IDictionary<string,object> headers, CancellationToken cancellationToken)
    {
        TestEvent = @event;
        Headers = headers;
        return Task.CompletedTask;
    }
}
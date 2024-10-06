namespace SimpleEventBus.Tests;

public class TestHandler
{
    public TestEvent HandledEvent { get; set; }
    
    public IDictionary<string, object> HandledHeaders { get; set; }

    public Task Handle(TestEvent @event, IDictionary<string,object> headers, CancellationToken cancellationToken)
    {
        HandledEvent = @event;
        HandledHeaders = headers;
        return Task.CompletedTask;
    }
}
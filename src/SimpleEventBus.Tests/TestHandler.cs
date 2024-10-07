using SimpleEventBus.Event;
using SimpleEventBus.Subscriber;

namespace SimpleEventBus.Tests;

public class TestHandler : IEventHandler<TestEvent>
{
    public TestEvent HandledEvent { get; set; }
    
    public IDictionary<string, object> HandledHeaders { get; set; }
    
    public Task HandleAsync(TestEvent @event, Headers headers, CancellationToken cancellationToken)
    {
        HandledEvent = @event;
        HandledHeaders = headers;
        return Task.CompletedTask;
    }
}
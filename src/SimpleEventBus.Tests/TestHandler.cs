using SimpleEventBus.Event;
using SimpleEventBus.Subscriber;

namespace SimpleEventBus.Tests;

public class TestHandler : IEventHandler<TestEvent>
{
    public TestEvent? HandledEvent { get; set; }
    
<<<<<<< HEAD
    public IDictionary<string, object> HandledHeaders { get; set; }
    
    public Task HandleAsync(TestEvent @event, Headers headers, CancellationToken cancellationToken)
=======
    public IDictionary<string, object>? HandledHeaders { get; set; }

    public Task Handle(TestEvent @event, IDictionary<string,object> headers, CancellationToken cancellationToken)
>>>>>>> feature/BuildEventHandlerExecutor
    {
        HandledEvent = @event;
        HandledHeaders = headers;
        return Task.CompletedTask;
    }
}

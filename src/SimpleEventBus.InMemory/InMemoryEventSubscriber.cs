using Microsoft.Extensions.Logging;
using SimpleEventBus.Subscriber;

namespace SimpleEventBus.InMemory;

public class InMemoryEventSubscriber : AbstractEventSubscriber
{
    private ILogger<InMemoryEventSubscriber> _logger;
    
    public InMemoryEventSubscriber(ILogger<InMemoryEventSubscriber> logger)
    {
        _logger = logger;
    }
    
    protected override Task SubscribeEventsAsync(List<string> eventNames)
    {
        return Task.CompletedTask;
    }
}
using Microsoft.Extensions.Logging;
using SimpleEventBus.Subscriber;

namespace SimpleEventBus.InMemory;

internal class InMemoryEventSubscriber : AbstractEventSubscriber
{
    private readonly ILogger<InMemoryEventSubscriber> _logger;

    private BackgroundQueue _backgroundQueue;
    
    public InMemoryEventSubscriber(ILogger<InMemoryEventSubscriber> logger, 
                                   BackgroundQueue backgroundQueue)
    {
        _logger = logger;
        _backgroundQueue = backgroundQueue;
    }
    
    protected override Task SubscribeEventsAsync(List<string> eventNames)
    {
        return Task.CompletedTask;
    }
}
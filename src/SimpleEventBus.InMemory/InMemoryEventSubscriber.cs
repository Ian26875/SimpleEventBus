using Microsoft.Extensions.Logging;
using SimpleEventBus.Subscriber;

namespace SimpleEventBus.InMemory;

internal class InMemoryEventSubscriber : AbstractEventSubscriber
{
    private readonly ILogger<InMemoryEventSubscriber> _logger;
    
    public InMemoryEventSubscriber(ILogger<InMemoryEventSubscriber> logger)
    {
        _logger = logger;
    }

    protected override Task SubscribeEventsAsync(List<string> eventNames)
    {
        foreach (var eventName in eventNames)
        {
            _logger.LogInformation("Subscribed to event: {EventName}", eventName);
        }

        return Task.CompletedTask;
    }
}
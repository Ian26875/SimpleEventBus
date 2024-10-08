using Microsoft.Extensions.Logging;
using SimpleEventBus.Event;
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

    protected override Task SubscribeEventsAsync(List<string> eventNames, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public override async Task ReceiveAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        async Task Func(ReadOnlyMemory<byte> body, IDictionary<string, object> header, string route, CancellationToken ct)
        {
            var eventData = new EventData(body, (Headers)header, route);
            await ConsumerReceived(eventData);
        }

        await this._backgroundQueue.ReceiveAsync(Func, cancellationToken);
    }
}
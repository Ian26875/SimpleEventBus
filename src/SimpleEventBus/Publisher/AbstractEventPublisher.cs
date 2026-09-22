using SimpleEventBus.Event;
using SimpleEventBus.Metrics;
using SimpleEventBus.Schema;
using SimpleEventBus.Serialization;

namespace SimpleEventBus;

public abstract class AbstractEventPublisher : IEventBus
{
    protected readonly ISerializer _serializer;

    protected readonly IEventMapper EventMapper;
    
    protected AbstractEventPublisher(ISerializer serializer, IEventMapper eventMapper)
    {
        _serializer = serializer;
        EventMapper = eventMapper;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, Headers? headers = null,
                                     CancellationToken cancellationToken = default(CancellationToken)) where TEvent : class
    {
        ArgumentNullException.ThrowIfNull(@event);

        headers ??= new Headers();
        
        var serializedData = _serializer.Serialize(@event);

        var eventName = EventMapper.GetEventName(typeof(TEvent));
        
        var eventData = new EventContext
        (
            serializedData,
            headers,
            eventName
        );
        
        try
        {
            await this.PublishEventAsync(eventData, cancellationToken);
            EventBusMetrics.AddPublished(eventName);
        }
        catch
        {
            EventBusMetrics.AddFailure(eventName);
            throw;
        }
    }

    protected abstract Task PublishEventAsync(EventContext eventContext, CancellationToken cancellationToken = default(CancellationToken));

}
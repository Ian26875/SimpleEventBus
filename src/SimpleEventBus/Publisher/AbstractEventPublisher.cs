using SimpleEventBus.Event;
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

    public Task PublishAsync<TEvent>(TEvent @event, Headers? headers = null,
                                     CancellationToken cancellationToken = default(CancellationToken)) where TEvent : class
    {
        if (@event is null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        headers ??= new Headers();
        
        var serializedData = _serializer.Serialize(@event);
        
        var eventData = new EventData
        (
            serializedData,
            headers,
            EventMapper.GetEventName(typeof(TEvent))
        );
        
        return this.PublishEventAsync(eventData, cancellationToken);
    }

    protected abstract Task PublishEventAsync(EventData eventData, CancellationToken cancellationToken = default(CancellationToken));

}
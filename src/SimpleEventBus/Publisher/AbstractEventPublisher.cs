using SimpleEventBus.Event;
using SimpleEventBus.Schema;
using SimpleEventBus.Serialization;

namespace SimpleEventBus;

public abstract class AbstractEventPublisher : IEventBus
{
    protected readonly ISerializer _serializer;

    protected readonly ISchemaRegistry _schemaRegistry;
    
    protected AbstractEventPublisher(ISerializer serializer, ISchemaRegistry schemaRegistry)
    {
        _serializer = serializer;
        _schemaRegistry = schemaRegistry;
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
            _schemaRegistry.GetEventName(typeof(TEvent))
        );
        
        return this.PublishEventAsync(eventData, cancellationToken);
    }

    protected abstract Task PublishEventAsync(EventData eventData, CancellationToken cancellationToken = default(CancellationToken));

}
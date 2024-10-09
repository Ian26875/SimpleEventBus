using SimpleEventBus.Event;
using SimpleEventBus.Schema;
using SimpleEventBus.Serialization;

namespace SimpleEventBus;

/// <summary>
/// The abstract event publisher class
/// </summary>
/// <seealso cref="IEventBus"/>
public abstract class AbstractEventPublisher : IEventBus
{
    /// <summary>
    /// The serializer
    /// </summary>
    protected readonly ISerializer _serializer;

    /// <summary>
    /// The schema registry
    /// </summary>
    protected readonly ISchemaRegistry _schemaRegistry;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractEventPublisher"/> class
    /// </summary>
    /// <param name="serializer">The serializer</param>
    /// <param name="schemaRegistry">The schema registry</param>
    protected AbstractEventPublisher(ISerializer serializer, ISchemaRegistry schemaRegistry)
    {
        _serializer = serializer;
        _schemaRegistry = schemaRegistry;
    }

    /// <summary>
    /// Publishes the event
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="event">The event</param>
    /// <param name="headers">The headers</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <exception cref="ArgumentNullException"></exception>
    public Task PublishAsync<TEvent>(TEvent @event, 
                                     Headers? headers = null,
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

    /// <summary>
    /// Publishes the event using the specified event data
    /// </summary>
    /// <param name="eventData">The event data</param>
    /// <param name="cancellationToken">The cancellation token</param>
    protected abstract Task PublishEventAsync(EventData eventData, CancellationToken cancellationToken = default(CancellationToken));

}
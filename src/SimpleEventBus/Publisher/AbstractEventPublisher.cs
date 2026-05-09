using SimpleEventBus.Event;
using SimpleEventBus.Metrics;
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

<<<<<<< HEAD
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
=======
    protected readonly IEventMapper EventMapper;
    
    protected AbstractEventPublisher(ISerializer serializer, IEventMapper eventMapper)
>>>>>>> feature/BuildEventHandlerExecutor
    {
        _serializer = serializer;
        EventMapper = eventMapper;
    }

<<<<<<< HEAD
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
=======
    public async Task PublishAsync<TEvent>(TEvent @event, Headers? headers = null,
>>>>>>> feature/BuildEventHandlerExecutor
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

<<<<<<< HEAD
    /// <summary>
    /// Publishes the event using the specified event data
    /// </summary>
    /// <param name="eventData">The event data</param>
    /// <param name="cancellationToken">The cancellation token</param>
    protected abstract Task PublishEventAsync(EventData eventData, CancellationToken cancellationToken = default(CancellationToken));
=======
    protected abstract Task PublishEventAsync(EventContext eventContext, CancellationToken cancellationToken = default(CancellationToken));
>>>>>>> feature/BuildEventHandlerExecutor

}
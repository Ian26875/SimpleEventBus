using System.Text;
using SimpleEventBus.Event;
using SimpleEventBus.Schema;
using SimpleEventBus.Serialization;

namespace SimpleEventBus.InMemory;

/// <summary>
/// The in memory event publisher class
/// </summary>
/// <seealso cref="AbstractEventPublisher"/>
internal class InMemoryEventPublisher : AbstractEventPublisher
{
    /// <summary>
    /// The background queue
    /// </summary>
    private readonly BackgroundQueue _backgroundQueue;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryEventPublisher"/> class
    /// </summary>
    /// <param name="serializer">The serializer</param>
    /// <param name="schemaRegistry">The schema registry</param>
    /// <param name="backgroundQueue">The background queue</param>
    public InMemoryEventPublisher(ISerializer serializer, 
                                  ISchemaRegistry schemaRegistry, 
                                  BackgroundQueue backgroundQueue)
                                : base(serializer, schemaRegistry)
    {
        _backgroundQueue = backgroundQueue;
    }
    
    /// <summary>
    /// Publishes the event using the specified event data
    /// </summary>
    /// <param name="eventData">The event data</param>
    /// <param name="cancellationToken">The cancellation token</param>
    protected override async Task PublishEventAsync(EventData eventData, CancellationToken cancellationToken = default(CancellationToken))
    {
        await _backgroundQueue.SendAsync(eventData.Data, eventData.Headers,eventData.EventName, cancellationToken: cancellationToken);
    }

    
}
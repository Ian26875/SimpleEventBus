using SimpleEventBus.Event;
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
    /// <param name="backgroundQueue">The background queue</param>
    internal InMemoryEventPublisher(ISerializer serializer, 
                                    BackgroundQueue backgroundQueue) : base(serializer)
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
        await _backgroundQueue.EnqueueAsync(eventData, cancellationToken);
    }
}
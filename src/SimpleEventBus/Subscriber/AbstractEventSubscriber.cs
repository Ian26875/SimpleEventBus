using SimpleEventBus.Event;

namespace SimpleEventBus.Subscriber;

/// <summary>
/// The abstract event subscriber class
/// </summary>
/// <seealso cref="IEventSubscriber"/>
public abstract class AbstractEventSubscriber : IEventSubscriber
{
    
    protected abstract Task SubscribeEventsAsync(List<string> eventNames,CancellationToken cancellationToken);

    public Task SubscribeAsync(List<string> eventNames, CancellationToken cancellationToken = default(CancellationToken))
    {
        return SubscribeEventsAsync(eventNames,cancellationToken);
    }

    public Func<EventData, Task> ConsumerReceived { get; set; }
    public abstract Task ReceiveAsync(CancellationToken cancellationToken = default(CancellationToken));
}
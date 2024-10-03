using SimpleEventBus.Event;

namespace SimpleEventBus.Subscriber;

/// <summary>
/// The abstract event subscriber class
/// </summary>
/// <seealso cref="IEventSubscriber"/>
public abstract class AbstractEventSubscriber : IEventSubscriber
{
    
    protected abstract Task SubscribeEventsAsync(List<string> eventNames);
    
    public Task SubscribeAsync(List<string> eventNames)
    {
        return SubscribeEventsAsync(eventNames);
    }

    public Func<EventData, Task> ConsumerReceived { get; set; }
}
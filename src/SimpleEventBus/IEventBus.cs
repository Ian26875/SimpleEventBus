using SimpleEventBus.Subscriber;

namespace SimpleEventBus;

public interface IEventBus : IEventPublisher,IEventSubscriber,IDisposable
{
    
}
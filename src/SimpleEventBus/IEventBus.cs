using SimpleEventBus.Subscriber;

namespace SimpleEventBus;

public interface IEventBus : IEventPublisher, IDisposable
{
}
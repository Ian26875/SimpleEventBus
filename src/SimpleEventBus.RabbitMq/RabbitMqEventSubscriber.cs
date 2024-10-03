using SimpleEventBus.ExceptionHandlers;
using SimpleEventBus.Profile;
using SimpleEventBus.Subscriber;

namespace SimpleEventBus.RabbitMq;

public class RabbitMqEventSubscriber : AbstractEventSubscriber
{
    protected override Task SubscribeEventsAsync(List<string> eventNames)
    {
        throw new NotImplementedException();
    }
}
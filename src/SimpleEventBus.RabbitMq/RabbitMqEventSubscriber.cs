using SimpleEventBus.ExceptionHandlers;
using SimpleEventBus.Profile;
using SimpleEventBus.Subscriber;

namespace SimpleEventBus.RabbitMq;

public class RabbitMqEventSubscriber : AbstractEventSubscriber
{

    protected override Task SubscribeEventsAsync(List<string> eventNames, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override Task ReceiveAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        throw new NotImplementedException();
    }
}
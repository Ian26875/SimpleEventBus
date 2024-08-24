using SimpleEventBus.Event;

namespace SimpleEventBus.RabbitMq;

public class RabbitMqEventPublisher : AbstractEventPublisher
{
    private RabbitMqOption _rabbitMqOption;

    public RabbitMqEventPublisher(RabbitMqOption rabbitMqOption)
    {
        _rabbitMqOption = rabbitMqOption;
    }
    
    protected override Task PublishEventAsync<TEvent>(EventContext<TEvent> eventContext,
                                                      CancellationToken cancellationToken = default(CancellationToken))
    {
        throw new NotImplementedException();
    }
}
using SimpleEventBus.Profile;
using SimpleEventBus.Subscriber;

namespace WebApplication1;

public record OrderPlacedEvent(Guid OrderId,string Name);


public class SendEmail : IEventHandler<OrderPlacedEvent>
{
    private ILogger<SendEmail> _logger;

    public SendEmail(ILogger<SendEmail> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(OrderPlacedEvent @event, SimpleEventBus.Event.Headers headers, CancellationToken cancellationToken)
    {
        _logger.LogInformation("SendEmail");
		
        return Task.CompletedTask;
    }
}
public class OrderSubscriptionProfile : SubscriptionProfile 
{
    public OrderSubscriptionProfile()
    {
        this.WhenOccurs<OrderPlacedEvent>().ToDo<SendEmail>();
    }
}
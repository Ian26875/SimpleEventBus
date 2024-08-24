using EasyNetQ;
using EasyNetQ.Topology;
using SimpleEventBus.Event;

namespace SimpleEventBus.RabbitMq;

public class RabbitMqEventPublisher : AbstractEventPublisher, IDisposable
{
    private readonly RabbitMqOption _rabbitMqOption;
    
    private readonly RabbitMqBindingOption _rabbitMqBindingOption;

    private IAdvancedBus AdvancedBus { get; set; }

    public RabbitMqEventPublisher(RabbitMqOption rabbitMqOption, RabbitMqBindingOption rabbitMqBindingOption)
    {
        _rabbitMqOption = rabbitMqOption;
        _rabbitMqBindingOption = rabbitMqBindingOption;
        InitializeBus();
    }

    private void InitializeBus()
    {
        var bus = RabbitHutch.CreateBus($"{_rabbitMqOption.UserName}:{_rabbitMqOption.Password}@{_rabbitMqOption.Host}/");
        AdvancedBus = bus.Advanced;
    }

    protected override async Task PublishEventAsync<TEvent>(EventContext<TEvent> eventContext, CancellationToken cancellationToken = default)
    {
        if (eventContext == null)
        {
            throw new ArgumentNullException(nameof(eventContext));
        }
        
        string exchangeName = string.IsNullOrWhiteSpace(_rabbitMqOption.ExchangeName).Equals(false) 
            ? _rabbitMqBindingOption.ExchangeBindings[eventContext.EventType] 
            : _rabbitMqOption.ExchangeName;
        
        var exchange = await AdvancedBus.ExchangeDeclareAsync
        (
            exchangeName, 
            configure: configuration =>
            {
                configuration.WithType(ExchangeType.Direct);
            }, 
            cancellationToken
        );

        byte[] messageBody = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(eventContext.Event);
        
        await AdvancedBus.PublishAsync
        (
            exchange,
            eventContext.EventType.Name, 
            false,
            new MessageProperties
            {
                DeliveryMode = 2,
                Headers = eventContext.Headers,
            },
            messageBody,
            cancellationToken
        );
    }
    
    public void Dispose()
    {
        AdvancedBus.Dispose();
    }
}
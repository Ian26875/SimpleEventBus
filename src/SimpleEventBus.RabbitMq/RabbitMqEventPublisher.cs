using EasyNetQ;
using EasyNetQ.Topology;
using Microsoft.Extensions.Options;
using SimpleEventBus.Event;

namespace SimpleEventBus.RabbitMq;

/// <summary>
/// The rabbit mq event publisher class
/// </summary>
/// <seealso cref="AbstractEventPublisher"/>
/// <seealso cref="IDisposable"/>
public class RabbitMqEventPublisher : AbstractEventPublisher, IDisposable
{
    /// <summary>
    /// The rabbit mq option
    /// </summary>
    private readonly RabbitMqOption _rabbitMqOption;
    
    /// <summary>
    /// The rabbit mq binding option
    /// </summary>
    private readonly RabbitMqBindingOption _rabbitMqBindingOption;

    /// <summary>
    /// Gets or sets the value of the advanced bus
    /// </summary>
    private IAdvancedBus AdvancedBus { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMqEventPublisher"/> class
    /// </summary>
    /// <param name="rabbitMqOption">The rabbit mq option</param>
    /// <param name="rabbitMqBindingOption">The rabbit mq binding option</param>
    public RabbitMqEventPublisher(IOptions<RabbitMqOption> rabbitMqOption, IOptions<RabbitMqBindingOption> rabbitMqBindingOption)
    {
        _rabbitMqOption = rabbitMqOption.Value;
        _rabbitMqBindingOption = rabbitMqBindingOption.Value;
        InitializeBus();
    }

    /// <summary>
    /// Initializes the bus
    /// </summary>
    private void InitializeBus()
    {
        var bus = RabbitHutch.CreateBus($"{_rabbitMqOption.UserName}:{_rabbitMqOption.Password}@{_rabbitMqOption.Host}/");
        AdvancedBus = bus.Advanced;
    }

    /// <summary>
    /// Publishes the event using the specified event context
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="eventContext">The event context</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <exception cref="ArgumentNullException"></exception>
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
    
    /// <summary>
    /// Disposes this instance
    /// </summary>
    public override void Dispose()
    {
        AdvancedBus.Dispose();
    }
}
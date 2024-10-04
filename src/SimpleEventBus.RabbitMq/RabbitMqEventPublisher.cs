using EasyNetQ;
using EasyNetQ.Topology;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleEventBus.Event;
using SimpleEventBus.Schema;
using ISerializer = SimpleEventBus.Serialization.ISerializer;

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
    /// The logger
    /// </summary>
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    
    /// <summary>
    /// Gets or sets the value of the advanced bus
    /// </summary>
    private IAdvancedBus AdvancedBus { get; set; }
    
    
    public RabbitMqEventPublisher(ISerializer serializer, 
                                  ISchemaRegistry schemaRegistry) 
        : base(serializer, schemaRegistry)
    {
    }
    
    /// <summary>
    /// Initializes the bus
    /// </summary>
    private void InitializeBus()
    {
        var bus = RabbitHutch.CreateBus($"{_rabbitMqOption.UserName}:{_rabbitMqOption.Password}@{_rabbitMqOption.Host}/");
        AdvancedBus = bus.Advanced;
    }
    
    private async Task<Exchange> GetOrDeclareExchangeAsync(EventData eventData, CancellationToken cancellationToken)
    {
        var exchangeName = _rabbitMqBindingOption.ExchangeBindings.TryGetValue(eventData.EventName, out var bindingExchangeName)
                               ? bindingExchangeName 
                               : _rabbitMqBindingOption.GlobalExchange;

        var exchange = await AdvancedBus.ExchangeDeclareAsync
                       (
                           exchangeName, 
                           configure: configuration =>
                           {
                               configuration.WithType(ExchangeType.Topic);
                           }, 
                           cancellationToken
                       );
        return exchange;
    }
    

    protected override async Task PublishEventAsync(EventData eventData, CancellationToken cancellationToken = default(CancellationToken))
    {
        if (eventData is null)
        {
            throw new ArgumentNullException(nameof(eventData));
        }
        
        var exchange = await GetOrDeclareExchangeAsync(eventData, cancellationToken);
        
        var routeKey = eventData.EventName;
        
        _logger.LogTrace("Declaring RabbitMQ exchange to publish event ...");

        _logger.LogTrace("Publishing event to RabbitMQ...");
        
        await AdvancedBus.PublishAsync
        (
            exchange,
            routeKey, 
            false,
            new MessageProperties
            {
                DeliveryMode = DeliveryMode.Persistent,
                Headers = eventData.Headers,
            },
            eventData.Data,
            cancellationToken
        );
    }

    public void Dispose()
    {
        AdvancedBus.Dispose();
    }

    
}
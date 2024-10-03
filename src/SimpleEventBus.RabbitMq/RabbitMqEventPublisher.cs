using EasyNetQ;
using EasyNetQ.Topology;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleEventBus.Event;
using SimpleEventBus.Schema;

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
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMqEventPublisher"/> class
    /// </summary>
    /// <param name="rabbitMqOption">The rabbit mq option</param>
    /// <param name="rabbitMqBindingOption">The rabbit mq binding option</param>
    /// <param name="logger">The logger</param>
    public RabbitMqEventPublisher(IOptions<RabbitMqOption> rabbitMqOption,
                                  IOptions<RabbitMqBindingOption> rabbitMqBindingOption, 
                                  ILogger<RabbitMqEventPublisher> logger)
    {
        _logger = logger;
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
        if (eventContext is null)
        {
            throw new ArgumentNullException(nameof(eventContext));
        }
        
        var exchange = await GetOrDeclareExchangeAsync(eventContext, cancellationToken);
        
        var routeKey = SchemaRegistry.Instance.GetEventName(eventContext.EventType);
        
        byte[] messageBody = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(eventContext.Event);
        
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
                Headers = eventContext.Headers,
            },
            messageBody,
            cancellationToken
        );
    }

    /// <summary>
    /// Gets the or declare exchange using the specified event context
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="eventContext">The event context</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The exchange</returns>
    private async Task<Exchange> GetOrDeclareExchangeAsync<TEvent>(EventContext<TEvent> eventContext, CancellationToken cancellationToken) where TEvent : class
    {
        var exchangeName = _rabbitMqBindingOption.ExchangeBindings.TryGetValue(eventContext.EventType, out var bindingExchangeName)
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


    /// <summary>
    /// Disposes this instance
    /// </summary>
    public override void Dispose()
    {
        AdvancedBus.Dispose();
    }
}
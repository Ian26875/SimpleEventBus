using System;
using System.Threading;
using System.Threading.Tasks;
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
    private readonly RabbitMqConnectionOption _rabbitMqConnectionOption;
    
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
    private IAdvancedBus? _advancedBus;
    private IBus? _bus;
    private readonly object _initLock = new();
    
    
    public RabbitMqEventPublisher(ISerializer serializer, 
                                  IEventMapper eventMapper,
                                  IOptions<RabbitMqConnectionOption> rabbitMqOptions,
                                  IOptions<RabbitMqBindingOption> rabbitMqBindingOptions,
                                  ILogger<RabbitMqEventPublisher> logger) 
        : base(serializer, eventMapper)
    {
        _rabbitMqConnectionOption = rabbitMqOptions?.Value ?? throw new ArgumentNullException(nameof(rabbitMqOptions));
        _rabbitMqBindingOption = rabbitMqBindingOptions?.Value ?? throw new ArgumentNullException(nameof(rabbitMqBindingOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ValidateOptions(_rabbitMqConnectionOption);
    }
    
    /// <summary>
    /// Initializes the bus
    /// </summary>
    private void InitializeBus()
    {
        if (_advancedBus is not null)
        {
            return;
        }

        lock (_initLock)
        {
            if (_advancedBus is not null)
            {
                return;
            }

            var connectionString = $"amqp://{_rabbitMqConnectionOption.UserName}:{_rabbitMqConnectionOption.Password}@{_rabbitMqConnectionOption.Host}/";
            _bus = RabbitHutch.CreateBus(connectionString);
            _advancedBus = _bus.Advanced;
        }
    }

    private IAdvancedBus GetAdvancedBus()
    {
        InitializeBus();
        return _advancedBus!;
    }

    private static void ValidateOptions(RabbitMqConnectionOption connectionOption)
    {
        if (string.IsNullOrWhiteSpace(connectionOption.UserName))
        {
            throw new ArgumentException("RabbitMqOption.UserName is required.", nameof(connectionOption));
        }

        if (string.IsNullOrWhiteSpace(connectionOption.Password))
        {
            throw new ArgumentException("RabbitMqOption.Password is required.", nameof(connectionOption));
        }

        if (string.IsNullOrWhiteSpace(connectionOption.Host))
        {
            throw new ArgumentException("RabbitMqOption.Host is required.", nameof(connectionOption));
        }
    }
    
    private async Task<Exchange> GetOrDeclareExchangeAsync(EventContext eventContext, CancellationToken cancellationToken)
    {
        var exchangeName = _rabbitMqBindingOption.ExchangeBindings.TryGetValue(eventContext.EventName, out var bindingExchangeName)
                               ? bindingExchangeName 
                               : _rabbitMqBindingOption.GlobalExchange;

        if (string.IsNullOrWhiteSpace(exchangeName))
        {
            throw new InvalidOperationException($"Exchange is not configured for event '{eventContext.EventName}'.");
        }

        var exchange = await GetAdvancedBus().ExchangeDeclareAsync
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
    

    protected override async Task PublishEventAsync(EventContext eventContext, CancellationToken cancellationToken = default(CancellationToken))
    {
        if (eventContext is null)
        {
            throw new ArgumentNullException(nameof(eventContext));
        }
        
        var exchange = await GetOrDeclareExchangeAsync(eventContext, cancellationToken);
        
        var routeKey = eventContext.EventName;
        
        _logger.LogTrace("Declaring RabbitMQ exchange to publish event ...");

        _logger.LogTrace("Publishing event to RabbitMQ...");
        
        await GetAdvancedBus().PublishAsync
        (
            exchange,
            routeKey, 
            false,
            new MessageProperties
            {
                DeliveryMode = DeliveryMode.Persistent,
                Headers = eventContext.Headers,
            },
            eventContext.Data,
            cancellationToken
        );
    }

    public void Dispose()
    {
        _bus?.Dispose();
    }

    
}

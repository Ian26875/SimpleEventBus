using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FluentEventBus.Event;
using FluentEventBus.Schema;
using ISerializer = FluentEventBus.Serialization.ISerializer;

namespace FluentEventBus.RabbitMq;

/// <summary>
/// The rabbit mq event publisher class
/// </summary>
/// <seealso cref="AbstractEventPublisher"/>
public class RabbitMqEventPublisher : AbstractEventPublisher
{
    /// <summary>
    /// The rabbit mq binding option
    /// </summary>
    private readonly RabbitMqBindingOption _rabbitMqBindingOption;

    /// <summary>
    /// The shared connection provider
    /// </summary>
    private readonly RabbitMqConnectionProvider _connectionProvider;

    /// <summary>
    /// The logger
    /// </summary>
    private readonly ILogger<RabbitMqEventPublisher> _logger;

    /// <summary>
    /// Exchanges already declared on the broker, keyed by name. Exchanges are durable,
    /// so one declaration per process lifetime is enough; this avoids a broker
    /// round-trip on every publish.
    /// </summary>
    private readonly ConcurrentDictionary<string, Exchange> _declaredExchanges = new();

    public RabbitMqEventPublisher(ISerializer serializer,
                                  IEventMapper eventMapper,
                                  RabbitMqConnectionProvider connectionProvider,
                                  IOptions<RabbitMqBindingOption> rabbitMqBindingOptions,
                                  ILogger<RabbitMqEventPublisher> logger)
        : base(serializer, eventMapper)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        _rabbitMqBindingOption = rabbitMqBindingOptions?.Value ?? throw new ArgumentNullException(nameof(rabbitMqBindingOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

        if (_declaredExchanges.TryGetValue(exchangeName, out var cachedExchange))
        {
            return cachedExchange;
        }

        // A concurrent duplicate declaration is harmless: exchange declaration is idempotent.
        var exchange = await _connectionProvider.GetAdvancedBus().ExchangeDeclareAsync
                       (
                           exchangeName,
                           configure: configuration =>
                           {
                               configuration.WithType(ExchangeType.Topic);
                           },
                           cancellationToken
                       );
        _declaredExchanges.TryAdd(exchangeName, exchange);
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
        
        await _connectionProvider.GetAdvancedBus().PublishAsync
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
}

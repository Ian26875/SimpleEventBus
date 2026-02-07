using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleEventBus.Event;
using SimpleEventBus.Subscriber;

namespace SimpleEventBus.RabbitMq;

public class RabbitMqEventSubscriber : AbstractEventSubscriber, IDisposable
{
    private readonly RabbitMqOption _rabbitMqOption;
    private readonly RabbitMqBindingOption _rabbitMqBindingOption;
    private readonly ILogger<RabbitMqEventSubscriber> _logger;
    private readonly object _initLock = new();
    private IBus? _bus;
    private IAdvancedBus? _advancedBus;
    private readonly List<IDisposable> _consumers = new();

    public RabbitMqEventSubscriber(IOptions<RabbitMqOption> rabbitMqOptions,
                                   IOptions<RabbitMqBindingOption> rabbitMqBindingOptions,
                                   ILogger<RabbitMqEventSubscriber> logger)
    {
        _rabbitMqOption = rabbitMqOptions?.Value ?? throw new ArgumentNullException(nameof(rabbitMqOptions));
        _rabbitMqBindingOption = rabbitMqBindingOptions?.Value ?? throw new ArgumentNullException(nameof(rabbitMqBindingOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ValidateOptions(_rabbitMqOption);
    }

    protected override async Task SubscribeEventsAsync(List<string> eventNames)
    {
        ArgumentNullException.ThrowIfNull(eventNames);

        var advancedBus = GetAdvancedBus();

        foreach (var eventName in eventNames.Distinct())
        {
            var exchangeName = _rabbitMqBindingOption.ExchangeBindings.TryGetValue(eventName, out var bindingExchangeName)
                ? bindingExchangeName
                : _rabbitMqBindingOption.GlobalExchange;

            var queueName = _rabbitMqBindingOption.QueueBindings.TryGetValue(eventName, out var bindingQueueName)
                ? bindingQueueName
                : _rabbitMqBindingOption.GlobalQueue;

            if (string.IsNullOrWhiteSpace(exchangeName))
            {
                throw new InvalidOperationException($"Exchange is not configured for event '{eventName}'.");
            }

            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new InvalidOperationException($"Queue is not configured for event '{eventName}'.");
            }

            var exchange = await advancedBus.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic);
            var queue = await advancedBus.QueueDeclareAsync(queueName);
            await advancedBus.BindAsync(exchange, queue, eventName);

            var consumer = advancedBus.Consume(queue, async (body, properties, info) =>
            {
                var headers = new Headers();
                if (properties?.Headers is not null)
                {
                    foreach (var pair in properties.Headers)
                    {
                        headers[pair.Key] = pair.Value;
                    }
                }

                var context = new EventContext(body, headers, info.RoutingKey ?? eventName);

                if (ConsumerReceived is null)
                {
                    _logger.LogWarning("ConsumerReceived handler not set. Skipping event: {EventName}", context.EventName);
                    return;
                }

                await ConsumerReceived(context);
            });

            _consumers.Add(consumer);
            _logger.LogInformation("Subscribed to RabbitMQ event: {EventName}", eventName);
        }
    }

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

            var connectionString = $"{_rabbitMqOption.UserName}:{_rabbitMqOption.Password}@{_rabbitMqOption.Host}/";
            _bus = RabbitHutch.CreateBus(connectionString);
            _advancedBus = _bus.Advanced;
        }
    }

    private IAdvancedBus GetAdvancedBus()
    {
        InitializeBus();
        return _advancedBus!;
    }

    private static void ValidateOptions(RabbitMqOption option)
    {
        if (string.IsNullOrWhiteSpace(option.UserName))
        {
            throw new ArgumentException("RabbitMqOption.UserName is required.", nameof(option));
        }

        if (string.IsNullOrWhiteSpace(option.Password))
        {
            throw new ArgumentException("RabbitMqOption.Password is required.", nameof(option));
        }

        if (string.IsNullOrWhiteSpace(option.Host))
        {
            throw new ArgumentException("RabbitMqOption.Host is required.", nameof(option));
        }
    }

    public void Dispose()
    {
        foreach (var consumer in _consumers)
        {
            consumer.Dispose();
        }

        _bus?.Dispose();
    }
}

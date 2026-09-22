using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Consumer;
using EasyNetQ.Topology;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleEventBus.Event;
using SimpleEventBus.Subscriber;

namespace SimpleEventBus.RabbitMq;

public class RabbitMqEventSubscriber : AbstractEventSubscriber, IDisposable
{
    private readonly RabbitMqBindingOption _rabbitMqBindingOption;
    private readonly RabbitMqConnectionProvider _connectionProvider;
    private readonly ILogger<RabbitMqEventSubscriber> _logger;
    private readonly List<IDisposable> _consumers = new();

    public RabbitMqEventSubscriber(RabbitMqConnectionProvider connectionProvider,
                                   IOptions<RabbitMqBindingOption> rabbitMqBindingOptions,
                                   ILogger<RabbitMqEventSubscriber> logger)
    {
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        _rabbitMqBindingOption = rabbitMqBindingOptions?.Value ?? throw new ArgumentNullException(nameof(rabbitMqBindingOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task SubscribeEventsAsync(List<string> eventNames)
    {
        ArgumentNullException.ThrowIfNull(eventNames);

        var advancedBus = _connectionProvider.GetAdvancedBus();

        TryConfigurePrefetch(advancedBus);

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
                exchangeName = "eventbus.topic";
            }

            if (string.IsNullOrWhiteSpace(queueName))
            {
                queueName = $"{_rabbitMqBindingOption.ServiceName}.{_rabbitMqBindingOption.EnvironmentName}";
            }

            if (string.IsNullOrWhiteSpace(exchangeName))
            {
                throw new InvalidOperationException($"Exchange is not configured for event '{eventName}'.");
            }

            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new InvalidOperationException($"Queue is not configured for event '{eventName}'.");
            }

            var deadLetterExchangeName = _rabbitMqBindingOption.DeadLetterExchangeBindings.TryGetValue(eventName, out var bindingDeadLetterExchange)
                ? bindingDeadLetterExchange
                : _rabbitMqBindingOption.GlobalDeadLetterExchange;

            var deadLetterQueueName = _rabbitMqBindingOption.DeadLetterQueueBindings.TryGetValue(eventName, out var bindingDeadLetterQueue)
                ? bindingDeadLetterQueue
                : _rabbitMqBindingOption.GlobalDeadLetterQueue;

            var hasDeadLetter = !string.IsNullOrWhiteSpace(deadLetterExchangeName) &&
                                !string.IsNullOrWhiteSpace(deadLetterQueueName);

            var exchange = await advancedBus.ExchangeDeclareAsync(exchangeName, ExchangeType.Topic);

            Queue queue;
            if (hasDeadLetter)
            {
                var deadLetterExchange = await advancedBus.ExchangeDeclareAsync(deadLetterExchangeName, ExchangeType.Topic);
                var deadLetterQueue = await advancedBus.QueueDeclareAsync(deadLetterQueueName);
                // Dead-lettered messages keep their original routing key; "#" captures them all.
                await advancedBus.BindAsync(deadLetterExchange, deadLetterQueue, "#");

                queue = await advancedBus.QueueDeclareAsync(queueName,
                    configuration => configuration.WithArgument("x-dead-letter-exchange", deadLetterExchangeName));
            }
            else
            {
                queue = await advancedBus.QueueDeclareAsync(queueName);
            }

            await advancedBus.BindAsync(exchange, queue, eventName);

            var consumer = advancedBus.Consume(queue, async (body, properties, info) =>
            {
                var headers = new Headers();
                if (properties?.Headers is not null)
                {
                    foreach (var pair in properties.Headers)
                    {
                        // AMQP delivers string headers as byte[]; convert back so
                        // MessageId / CorrelationId round-trip as strings.
                        headers[pair.Key] = pair.Value is byte[] bytes
                            ? System.Text.Encoding.UTF8.GetString(bytes)
                            : pair.Value!;
                    }
                }

                var context = new EventContext(body, headers, info.RoutingKey ?? eventName);

                if (ConsumerReceived is null)
                {
                    _logger.LogWarning("ConsumerReceived handler not set. Skipping event: {EventName}", context.EventName);
                    return AckStrategies.NackWithRequeue;
                }

                try
                {
                    await ConsumerReceived(context);
                    return AckStrategies.Ack;
                }
                catch (Exception exception)
                {
                    return await HandleConsumeFailureAsync(advancedBus, queue, body, properties ?? new MessageProperties(), context, exception);
                }
            });

            _consumers.Add(consumer);
            _logger.LogInformation("Subscribed to RabbitMQ event: {EventName}", eventName);
        }
    }

    /// <summary>
    /// Header carrying the number of redeliveries already attempted for a failed message.
    /// </summary>
    internal const string RetryCountHeader = "x-retry-count";

    /// <summary>
    /// Decides what happens to a message whose handler threw: republish with an incremented
    /// retry counter while below <see cref="RabbitMqBindingOption.MaxRetryCount"/>, otherwise
    /// nack without requeue so the broker dead-letters it (or drops it when no DLX is configured).
    /// </summary>
    private async Task<AckStrategy> HandleConsumeFailureAsync(IAdvancedBus advancedBus,
                                                              Queue queue,
                                                              ReadOnlyMemory<byte> body,
                                                              MessageProperties properties,
                                                              EventContext context,
                                                              Exception exception)
    {
        var retryCount = GetRetryCount(properties);

        if (retryCount < _rabbitMqBindingOption.MaxRetryCount)
        {
            properties.Headers ??= new Dictionary<string, object?>();
            properties.Headers[RetryCountHeader] = retryCount + 1;

            // Republish to the same queue via the default exchange, then ack the original delivery.
            await advancedBus.PublishAsync(Exchange.Default, queue.Name, false, properties, body);

            _logger.LogWarning(exception,
                "Handler failed for event {EventName}. Retry {Retry}/{MaxRetry} scheduled.",
                context.EventName, retryCount + 1, _rabbitMqBindingOption.MaxRetryCount);

            return AckStrategies.Ack;
        }

        _logger.LogError(exception,
            "Handler failed for event {EventName} after {MaxRetry} retries. Message is nacked without requeue " +
            "(dead-lettered when a dead letter exchange is configured).",
            context.EventName, _rabbitMqBindingOption.MaxRetryCount);

        return AckStrategies.NackWithoutRequeue;
    }

    /// <summary>
    /// Reads the retry counter header, tolerating the numeric types AMQP clients may deliver.
    /// </summary>
    private static int GetRetryCount(MessageProperties properties)
    {
        if (properties?.Headers is null || !properties.Headers.TryGetValue(RetryCountHeader, out var value))
        {
            return 0;
        }

        return value switch
        {
            int intValue => intValue,
            long longValue => (int)longValue,
            byte[] bytes when int.TryParse(System.Text.Encoding.UTF8.GetString(bytes), out var parsed) => parsed,
            string text when int.TryParse(text, out var parsed) => parsed,
            _ => 0
        };
    }

    public void Dispose()
    {
        foreach (var consumer in _consumers)
        {
            consumer.Dispose();
        }
    }

    private void TryConfigurePrefetch(IAdvancedBus advancedBus)
    {
        if (_rabbitMqBindingOption.PrefetchCount == 0)
        {
            return;
        }

        var type = advancedBus.GetType();
        var method = type.GetMethod("Qos", new[] { typeof(uint), typeof(ushort), typeof(bool) })
                     ?? type.GetMethod("SetQos", new[] { typeof(uint), typeof(ushort), typeof(bool) });

        if (method is null)
        {
            return;
        }

        method.Invoke(advancedBus, new object[] { 0u, _rabbitMqBindingOption.PrefetchCount, false });
    }
}

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FluentEventBus.Event;
using FluentEventBus.Subscriber;

namespace FluentEventBus.InMemory;

internal class InMemoryEventSubscriber : BackgroundService, IEventSubscriber
{
    /// <summary>
    /// Header carrying the number of redeliveries already attempted for a failed event.
    /// </summary>
    internal const string RetryCountHeader = Headers.RetryCountKey;

    private readonly ILogger<InMemoryEventSubscriber> _logger;
    private readonly BackgroundQueue _backgroundQueue;
    private readonly BackgroundQueueOptions _options;
    private List<string> _subscribedEventNames = new();

    public InMemoryEventSubscriber(ILogger<InMemoryEventSubscriber> logger,
                                   BackgroundQueue backgroundQueue,
                                   BackgroundQueueOptions options)
    {
        _logger = logger;
        _backgroundQueue = backgroundQueue;
        _options = options;
    }

    /// <summary>
    /// The delegate to handle received events
    /// </summary>
    public Func<EventContext, Task>? ConsumerReceived { get; set; }

    /// <summary>
    /// Subscribe to the given list of event names
    /// </summary>
    public Task SubscribeAsync(List<string> eventNames)
    {
        _subscribedEventNames = eventNames;

        foreach (var eventName in eventNames)
        {
            _logger.LogInformation("Subscribed to event: {EventName}", eventName);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Background task execution loop
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("InMemoryEventSubscriber background processing started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var eventContext = await _backgroundQueue.DequeueAsync(stoppingToken);

                // Optional: Check if eventName is subscribed (guard clause)
                if (_subscribedEventNames.Contains(eventContext.EventName))
                {
                    if (ConsumerReceived != null)
                    {
                        try
                        {
                            await ConsumerReceived(eventContext);
                        }
                        catch (OperationCanceledException)
                        {
                            throw;
                        }
                        catch (Exception exception)
                        {
                            await HandleConsumeFailureAsync(eventContext, exception, stoppingToken);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("ConsumerReceived handler not set. Skipping event: {EventName}", eventContext.EventName);
                    }
                }
                else
                {
                    _logger.LogWarning("Received unregistered event: {EventName}", eventContext.EventName);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("InMemoryEventSubscriber cancellation requested.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing event.");
            }
        }

        _logger.LogInformation("InMemoryEventSubscriber background processing stopped.");
    }

    /// <summary>
    /// Re-enqueues a failed event with an incremented retry counter while below
    /// <see cref="BackgroundQueueOptions.MaxRetryCount"/>; beyond that the event is treated
    /// as a poison message: <see cref="BackgroundQueueOptions.OnPoisonMessage"/> is invoked
    /// (or an error is logged) and the event is dropped.
    /// </summary>
    private async Task HandleConsumeFailureAsync(EventContext eventContext, Exception exception, CancellationToken cancellationToken)
    {
        var retryCount = GetRetryCount(eventContext.Headers);

        if (retryCount < _options.MaxRetryCount)
        {
            eventContext.Headers[RetryCountHeader] = retryCount + 1;
            await _backgroundQueue.EnqueueAsync(eventContext, cancellationToken);

            _logger.LogWarning(exception,
                "Handler failed for event {EventName}. Retry {Retry}/{MaxRetry} scheduled.",
                eventContext.EventName, retryCount + 1, _options.MaxRetryCount);
            return;
        }

        _logger.LogError(exception,
            "Handler failed for event {EventName} after {MaxRetry} retries. Event is dropped as a poison message.",
            eventContext.EventName, _options.MaxRetryCount);

        if (_options.OnPoisonMessage is not null)
        {
            await _options.OnPoisonMessage(eventContext, exception);
        }
    }

    private static int GetRetryCount(IDictionary<string, object> headers)
    {
        if (!headers.TryGetValue(RetryCountHeader, out var value))
        {
            return 0;
        }

        return value switch
        {
            int intValue => intValue,
            long longValue => (int)longValue,
            string text when int.TryParse(text, out var parsed) => parsed,
            _ => 0
        };
    }
}

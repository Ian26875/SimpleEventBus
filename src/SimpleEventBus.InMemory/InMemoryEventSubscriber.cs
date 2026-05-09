<<<<<<< HEAD
﻿using Microsoft.Extensions.Logging;
=======
﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
>>>>>>> feature/BuildEventHandlerExecutor
using SimpleEventBus.Event;
using SimpleEventBus.Subscriber;

namespace SimpleEventBus.InMemory;

internal class InMemoryEventSubscriber : BackgroundService, IEventSubscriber
{
    private readonly ILogger<InMemoryEventSubscriber> _logger;
    private readonly BackgroundQueue _backgroundQueue;
    private List<string> _subscribedEventNames = new();

    public InMemoryEventSubscriber(ILogger<InMemoryEventSubscriber> logger, BackgroundQueue backgroundQueue)
    {
        _logger = logger;
        _backgroundQueue = backgroundQueue;
    }

<<<<<<< HEAD
    protected override Task SubscribeEventsAsync(List<string> eventNames, CancellationToken cancellationToken)
=======
    /// <summary>
    /// The delegate to handle received events
    /// </summary>
    public Func<EventContext, Task>? ConsumerReceived { get; set; }

    /// <summary>
    /// Subscribe to the given list of event names
    /// </summary>
    public Task SubscribeAsync(List<string> eventNames)
>>>>>>> feature/BuildEventHandlerExecutor
    {
        _subscribedEventNames = eventNames;

        foreach (var eventName in eventNames)
        {
            _logger.LogInformation("Subscribed to event: {EventName}", eventName);
        }

        return Task.CompletedTask;
    }

<<<<<<< HEAD
    public override async Task ReceiveAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        async Task Func(ReadOnlyMemory<byte> body, IDictionary<string, object> header, string route, CancellationToken ct)
        {
            var eventData = new EventData(body, (Headers)header, route);
            await ConsumerReceived(eventData);
        }

        await this._backgroundQueue.ReceiveAsync(Func, cancellationToken);
    }
}
=======
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
                        await ConsumerReceived(eventContext);
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
}
>>>>>>> feature/BuildEventHandlerExecutor

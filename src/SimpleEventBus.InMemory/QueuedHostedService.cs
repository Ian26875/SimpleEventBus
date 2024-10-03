using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleEventBus.Subscriber;

namespace SimpleEventBus.InMemory;

/// <summary>
/// The queued hosted service class
/// </summary>
/// <seealso cref="BackgroundService"/>
internal class QueuedHostedService : BackgroundService
{
    /// <summary>
    /// The background queue
    /// </summary>
    private readonly BackgroundQueue _backgroundQueue;
    
    /// <summary>
    /// The logger
    /// </summary>
    private readonly ILogger _logger;
    
    /// <summary>
    /// The event subscriber
    /// </summary>
    private readonly IEventSubscriber _eventSubscriber;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="QueuedHostedService"/> class
    /// </summary>
    /// <param name="backgroundQueue">The background queue</param>
    /// <param name="loggerFactory">The logger factory</param>
    /// <param name="eventSubscriber">The event subscriber</param>
    internal QueuedHostedService(BackgroundQueue backgroundQueue, 
                                 ILoggerFactory loggerFactory, IEventSubscriber eventSubscriber)
    {
        _backgroundQueue = backgroundQueue;
        _eventSubscriber = eventSubscriber;
        _logger = loggerFactory.CreateLogger<QueuedHostedService>();
    }

    /// <summary>
    /// Executes the stopping token
    /// </summary>
    /// <param name="stoppingToken">The stopping token</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Queued Hosted Service is starting.");

        while (stoppingToken.IsCancellationRequested.Equals(false))
        {
            try
            {
                var eventData = await _backgroundQueue.DequeueAsync(stoppingToken);
                
                _logger.LogInformation("Executing a task from the queue.");

                await _eventSubscriber.ConsumerReceived(eventData);
                
                _logger.LogInformation("Task executed successfully.");
                
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Operation canceled while executing tasks.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing task.");
            }
        }

        _logger.LogInformation("Queued Hosted Service is stopping.");
    }
}

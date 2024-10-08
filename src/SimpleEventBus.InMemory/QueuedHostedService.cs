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
    private readonly ILogger<QueuedHostedService> _logger;
    
    /// <summary>
    /// The event subscriber
    /// </summary>
    private readonly IEventSubscriber _eventSubscriber;

    public QueuedHostedService(BackgroundQueue backgroundQueue, ILogger<QueuedHostedService> logger, IEventSubscriber eventSubscriber)
    {
        _backgroundQueue = backgroundQueue;
        _logger = logger;
        _eventSubscriber = eventSubscriber;
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
                await this._eventSubscriber.ReceiveAsync(stoppingToken);
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
        
    }
}

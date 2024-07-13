using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SimpleEventBus.InMemory;

public class QueuedHostedService : BackgroundService
{
    private readonly BackgroundQueue _backgroundQueue;
    private readonly ILogger _logger;

    public QueuedHostedService(BackgroundQueue backgroundQueue, ILoggerFactory loggerFactory)
    {
        _backgroundQueue = backgroundQueue;
        _logger = loggerFactory.CreateLogger<QueuedHostedService>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Queued Hosted Service is starting.");

        while (stoppingToken.IsCancellationRequested.Equals(false))
        {
            try
            {
                var taskFunc = await _backgroundQueue.DequeueAsync(stoppingToken);
                
                _logger.LogInformation("Executing a task from the queue.");
                
                await taskFunc(stoppingToken);
                
                _logger.LogInformation("Task executed successfully.");
                
            }
            catch (OperationCanceledException)
            {
                // Ignore this exception because it's normal when stopping the service
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

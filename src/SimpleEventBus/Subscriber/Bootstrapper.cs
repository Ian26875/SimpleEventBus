using Microsoft.Extensions.Hosting;

namespace SimpleEventBus.Subscriber;

internal class Bootstrapper : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
}
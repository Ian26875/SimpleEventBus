using Microsoft.Extensions.Hosting;

namespace SimpleEventBus.Internal;

internal class Bootstrapper : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
}
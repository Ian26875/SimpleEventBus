using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using SimpleEventBus.Event;

namespace SimpleEventBus.InMemory;

internal class BackgroundQueue
{
    private readonly int _alertThreshold;
    private readonly BackgroundQueueOptions _backgroundQueueOptions;
    private readonly Channel<EventData> _channel;
    private readonly ILogger<BackgroundQueue> _logger;
    private int _pendingCount;

    public BackgroundQueue(BackgroundQueueOptions options,
        ILogger<BackgroundQueue> logger,
        BackgroundQueueOptions backgroundQueueOptions)
    {
        var opts = options;
        var channelOptions = new BoundedChannelOptions(opts.Capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };

        _channel = Channel.CreateBounded<EventData>(channelOptions);
        _alertThreshold = opts.AlertThreshold;
        _logger = logger;
        _backgroundQueueOptions = backgroundQueueOptions;
        BackgroundQueueMetrics.RegisterPendingCount(() => _pendingCount);
    }

    public int PendingCount => _pendingCount;

    public async ValueTask EnqueueAsync(EventData eventData, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _pendingCount);
        BackgroundQueueMetrics.EnqueueCounter.Add(1);
        if (_pendingCount >= _alertThreshold)
        {
            _logger.LogWarning("Queue size has reached alert threshold: {Count}", _pendingCount);
            await _backgroundQueueOptions.OnAlert?.Invoke()!;
        }

        await _channel.Writer.WriteAsync(eventData, cancellationToken);
    }

    public async ValueTask<EventData> DequeueAsync(CancellationToken cancellationToken = default)
    {
        var result = await _channel.Reader.ReadAsync(cancellationToken);
        Interlocked.Decrement(ref _pendingCount);
        BackgroundQueueMetrics.DequeueCounter.Add(1);
        return result;
    }
}
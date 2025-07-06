using System.Diagnostics.Metrics;

namespace SimpleEventBus.InMemory;

internal static class BackgroundQueueMetrics
{
    private static readonly Meter Meter = new(Telemetry.SimpleEventBus.BackgroundQueueMeter);

    private static Func<Measurement<int>>? _pendingCountCallback;
    
    public static Counter<long> EnqueueCounter { get; } =
        Meter.CreateCounter<long>("background_queue_enqueue_total", "events", "Total number of events enqueued");
    public static Counter<long> DequeueCounter { get; } =
        Meter.CreateCounter<long>("background_queue_dequeue_total", "events", "Total number of events dequeued");

    /// <summary>
    ///     Registers a callback to observe the current number of unprocessed events in the queue.
    /// </summary>
    /// <param name="pendingCountProvider">A function that returns the current pending count</param>
    public static void RegisterPendingCount(Func<int> pendingCountProvider)
    {
        if (_pendingCountCallback is null)
        {
            _pendingCountCallback = () =>
                new Measurement<int>(pendingCountProvider(), new KeyValuePair<string, object?>("queue", "default"));

            Meter.CreateObservableGauge
            (
                "background_queue_pending",
                _pendingCountCallback,
                "items",
                "Current number of unprocessed events in the background queue"
            );
        }
    }
}
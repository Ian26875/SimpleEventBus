using System.Threading.Channels;
using Microsoft.Extensions.Logging;
<<<<<<< HEAD
=======
using SimpleEventBus.Event;
>>>>>>> feature/BuildEventHandlerExecutor

namespace SimpleEventBus.InMemory;

/// <summary>
/// Represents a background queue for processing <see cref="EventContext"/> items asynchronously,
/// with capacity limit and alert threshold support.
/// </summary>
internal class BackgroundQueue
{
<<<<<<< HEAD
    public class MessageContent
    {
        public IDictionary<string, object> Headers { get; set; }

        public ReadOnlyMemory<byte> Body { get; set; }

        public string Route { get; set; }
    }
    
    private Channel<MessageContent> _channel;
    
    public BackgroundQueue(int capacity)
=======
    /// <summary>
    /// The threshold value that triggers an alert when the queue size exceeds this count.
    /// </summary>
    private readonly int _alertThreshold;

    /// <summary>
    /// The configuration options for the background queue, such as capacity and alert behavior.
    /// </summary>
    private readonly BackgroundQueueOptions _backgroundQueueOptions;

    /// <summary>
    /// The underlying channel used for enqueueing and dequeueing <see cref="EventContext"/> items.
    /// </summary>
    private readonly Channel<EventContext> _channel;

    /// <summary>
    /// The logger instance for diagnostic and trace-level logging.
    /// </summary>
    private readonly ILogger<BackgroundQueue> _logger;

    /// <summary>
    /// Tracks the current number of items pending in the queue.
    /// </summary>
    private int _pendingCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackgroundQueue"/> class.
    /// </summary>
    /// <param name="logger">The logger used for logging queue activities.</param>
    /// <param name="backgroundQueueOptions">The configuration options for the queue.</param>
    public BackgroundQueue(ILogger<BackgroundQueue> logger,
                           BackgroundQueueOptions backgroundQueueOptions)
>>>>>>> feature/BuildEventHandlerExecutor
    {
        var channelOptions = new BoundedChannelOptions(backgroundQueueOptions.Capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false, 
            SingleWriter = false,
        };
<<<<<<< HEAD
        _channel = Channel.CreateBounded<MessageContent>(options);
    }
    
    public async ValueTask SendAsync(ReadOnlyMemory<byte> body,
                                     IDictionary<string,object> headers,
                                     string route,
                                     CancellationToken cancellationToken = default(CancellationToken))
    {
        await this._channel.Writer.WriteAsync(new MessageContent
        {
            Body = body,
            Headers = headers,
            Route = route
        }, cancellationToken);
    }
    
    public async ValueTask ReceiveAsync(Func<ReadOnlyMemory<byte>, IDictionary<string, object>,string , CancellationToken, Task> consumerReceived,
                                        CancellationToken cancellationToken = default(CancellationToken))
    {
        if (consumerReceived is null)
        {
            throw new ArgumentNullException(nameof(consumerReceived));
        }

        while (await _channel.Reader.WaitToReadAsync(cancellationToken))
        {
            if (_channel.Reader.TryRead(out var message))
            {
                await consumerReceived.Invoke(message.Body,message.Headers,message.Route,cancellationToken);
            }
        }
=======

        _channel = Channel.CreateBounded<EventContext>(channelOptions);
        _alertThreshold = backgroundQueueOptions.AlertThreshold;
        _logger = logger;
        _backgroundQueueOptions = backgroundQueueOptions;
        BackgroundQueueMetrics.RegisterPendingCount(() => _pendingCount);
    }

    /// <summary>
    /// Gets the current number of items waiting in the queue.
    /// </summary>
    public int PendingCount => _pendingCount;

    /// <summary>
    /// Adds a new event context to the queue for processing.
    /// Triggers an alert if the queue size reaches or exceeds the configured threshold.
    /// </summary>
    /// <param name="eventContext">The event context to enqueue.</param>
    /// <param name="cancellationToken">Optional token to cancel the enqueue operation.</param>
    /// <returns>A task representing the asynchronous enqueue operation.</returns>
    public async ValueTask EnqueueAsync(EventContext eventContext, CancellationToken cancellationToken = default)
    {
        var newCount = Interlocked.Increment(ref _pendingCount);
        BackgroundQueueMetrics.EnqueueCounter.Add(1);

        _logger.LogTrace("Event enqueued. PendingCount = {Count}", newCount);

        // Trigger alert if threshold is reached
        if (newCount >= _alertThreshold)
        {
            _logger.LogWarning("Queue size has reached alert threshold: {Count}", newCount);
            if (_backgroundQueueOptions.OnAlert is not null)
            {
                _logger.LogInformation("Triggering OnAlert action...");
                await _backgroundQueueOptions.OnAlert.Invoke();
            }
        }

        await _channel.Writer.WriteAsync(eventContext, cancellationToken);
    }

    /// <summary>
    /// Dequeues the next event context from the queue for processing.
    /// </summary>
    /// <param name="cancellationToken">Optional token to cancel the dequeue operation.</param>
    /// <returns>A task representing the asynchronous dequeue operation, returning the next <see cref="EventContext"/>.</returns>
    public async ValueTask<EventContext> DequeueAsync(CancellationToken cancellationToken = default)
    {
        var result = await _channel.Reader.ReadAsync(cancellationToken);

        var newCount = Interlocked.Decrement(ref _pendingCount);
        BackgroundQueueMetrics.DequeueCounter.Add(1);

        _logger.LogTrace("Event dequeued. PendingCount = {Count}", newCount);

        return result;
>>>>>>> feature/BuildEventHandlerExecutor
    }
}

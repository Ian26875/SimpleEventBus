using System.Threading.Channels;

namespace SimpleEventBus.InMemory;

/// <summary>
/// The background queue class
/// </summary>
internal class BackgroundQueue
{
    /// <summary>
    /// The channels
    /// </summary>
    private readonly Channel<Func<CancellationToken, ValueTask>> _channel;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackgroundQueue"/> class
    /// </summary>
    public BackgroundQueue(int capacity)
    {
        BoundedChannelOptions options = new (capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _channel = Channel.CreateBounded<Func<CancellationToken, ValueTask>>(options);
    }
    
    /// <summary>
    /// Enqueues the event type
    /// </summary>
    /// <param name="taskFunc">The task func</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public async ValueTask EnqueueAsync(Func<CancellationToken, ValueTask> taskFunc, CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(taskFunc, cancellationToken);
    }
    
    /// <summary>
    /// Dequeues the event type
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing a func of cancellation token and value task</returns>
    public ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }

    /// <summary>
    /// Reads all enqueued tasks asynchronously from the channel.
    /// </summary>
    /// <returns>An asynchronous enumerable of tasks, allowing each to be processed as it's read.</returns>
    public IAsyncEnumerable<Func<CancellationToken, ValueTask>> ReadAllTasksAsync()
    {
        return _channel.Reader.ReadAllAsync();
    }
    
    /// <summary>
    /// Checks if the channel is available for writing.
    /// </summary>
    public bool CanWrite => _channel.Writer.TryComplete();

    /// <summary>
    /// Checks if the channel is available for reading.
    /// </summary>
    public bool CanRead => _channel.Reader.Completion.IsCompleted.Equals(false);

    /// <summary>
    /// Checks if the channel is full.
    /// </summary>
    public bool IsFull => _channel.Writer.TryWrite(default).Equals(false);

    /// <summary>
    /// Checks if the channel is empty.
    /// </summary>
    public bool IsEmpty => _channel.Reader.TryRead(out _);
}
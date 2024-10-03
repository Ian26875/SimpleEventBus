using System.Threading.Channels;
using SimpleEventBus.Event;

namespace SimpleEventBus.InMemory;

internal class BackgroundQueue
{
    private readonly Channel<EventData> _channel;

    public BackgroundQueue(int capacity)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _channel = Channel.CreateBounded<EventData>(options);
    }

    public async ValueTask EnqueueAsync(EventData eventData, CancellationToken cancellationToken = default)
    {
        await _channel.Writer.WriteAsync(eventData, cancellationToken);
    }

    public ValueTask<EventData> DequeueAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }
}

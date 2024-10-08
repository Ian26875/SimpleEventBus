using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace SimpleEventBus.InMemory;

internal class BackgroundQueue
{
    public class MessageContent
    {
        public IDictionary<string, object> Headers { get; set; }

        public ReadOnlyMemory<byte> Body { get; set; }

        public string Route { get; set; }
    }
    
    private Channel<MessageContent> _channel;
    
    public BackgroundQueue(int capacity)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false, 
            SingleWriter = false,
        };
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
    }
}

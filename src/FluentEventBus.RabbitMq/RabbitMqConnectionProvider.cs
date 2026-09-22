using EasyNetQ;
using Microsoft.Extensions.Options;

namespace FluentEventBus.RabbitMq;

/// <summary>
/// Owns the single EasyNetQ bus shared by all RabbitMQ components
/// (publisher and subscriber), so the application holds one broker connection.
/// </summary>
public sealed class RabbitMqConnectionProvider : IDisposable
{
    private readonly Lazy<IBus> _bus;

    public RabbitMqConnectionProvider(IOptions<RabbitMqConnectionOption> options)
    {
        var connectionOption = options?.Value ?? throw new ArgumentNullException(nameof(options));
        ValidateOptions(connectionOption);

        _bus = new Lazy<IBus>(() =>
        {
            var connectionString = $"amqp://{connectionOption.UserName}:{connectionOption.Password}@{connectionOption.Host}/";
            return RabbitHutch.CreateBus(connectionString);
        });
    }

    /// <summary>
    /// Gets the shared advanced bus. The underlying connection is created lazily on first use.
    /// </summary>
    public IAdvancedBus GetAdvancedBus()
    {
        return _bus.Value.Advanced;
    }

    private static void ValidateOptions(RabbitMqConnectionOption connectionOption)
    {
        if (string.IsNullOrWhiteSpace(connectionOption.UserName))
        {
            throw new ArgumentException("RabbitMqOption.UserName is required.", nameof(connectionOption));
        }

        if (string.IsNullOrWhiteSpace(connectionOption.Password))
        {
            throw new ArgumentException("RabbitMqOption.Password is required.", nameof(connectionOption));
        }

        if (string.IsNullOrWhiteSpace(connectionOption.Host))
        {
            throw new ArgumentException("RabbitMqOption.Host is required.", nameof(connectionOption));
        }
    }

    public void Dispose()
    {
        if (_bus.IsValueCreated)
        {
            _bus.Value.Dispose();
        }
    }
}

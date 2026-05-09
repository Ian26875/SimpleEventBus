using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimpleEventBus.Internal;

namespace SimpleEventBus.RabbitMq;

/// <summary>
/// Validates RabbitMQ connectivity during application startup (fail-fast).
/// </summary>
public sealed class RabbitMqConnectionInitializer : IInitializer
{
    private readonly RabbitMqConnectionOption _connectionOptions;
    private readonly ILogger<RabbitMqConnectionInitializer> _logger;

    public RabbitMqConnectionInitializer(IOptions<RabbitMqConnectionOption> options,
                                         ILogger<RabbitMqConnectionInitializer> logger)
    {
        _connectionOptions = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_connectionOptions.UserName) ||
            string.IsNullOrWhiteSpace(_connectionOptions.Password) ||
            string.IsNullOrWhiteSpace(_connectionOptions.Host))
        {
            throw new ArgumentException("RabbitMqOption is not fully configured.");
        }

        _logger.LogInformation("Validating RabbitMQ connection...");

        try
        {
            var connectionString = $"amqp://{_connectionOptions.UserName}:{_connectionOptions.Password}@{_connectionOptions.Host}/";
            using var bus = EasyNetQ.RabbitHutch.CreateBus(connectionString);
            _ = bus.Advanced;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ connection validation failed.");
            throw;
        }

        _logger.LogInformation("RabbitMQ connection validation succeeded.");
        return Task.CompletedTask;
    }
}

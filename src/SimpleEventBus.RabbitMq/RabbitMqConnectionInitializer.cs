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
    private readonly RabbitMqOption _options;
    private readonly ILogger<RabbitMqConnectionInitializer> _logger;

    public RabbitMqConnectionInitializer(IOptions<RabbitMqOption> options,
                                         ILogger<RabbitMqConnectionInitializer> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.UserName) ||
            string.IsNullOrWhiteSpace(_options.Password) ||
            string.IsNullOrWhiteSpace(_options.Host))
        {
            throw new ArgumentException("RabbitMqOption is not fully configured.");
        }

        _logger.LogInformation("Validating RabbitMQ connection...");

        try
        {
            var connectionString = $"{_options.UserName}:{_options.Password}@{_options.Host}/";
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

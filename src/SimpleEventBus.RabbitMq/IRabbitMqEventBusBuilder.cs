using Microsoft.Extensions.DependencyInjection;

namespace SimpleEventBus.RabbitMq;

/// <summary>
/// The rabbit mq event bus builder interface
/// </summary>
public interface IRabbitMqEventBusBuilder
{
    /// <summary>
    /// Gets the value of the services
    /// </summary>
    IServiceCollection Services { get; }
}

/// <summary>
/// The rabbit mq event bus builder class
/// </summary>
/// <seealso cref="IRabbitMqEventBusBuilder"/>
public class RabbitMqEventBusBuilder : IRabbitMqEventBusBuilder
{
    /// <summary>
    /// Gets the value of the services
    /// </summary>
    public IServiceCollection Services { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMqEventBusBuilder"/> class
    /// </summary>
    /// <param name="services">The services</param>
    public RabbitMqEventBusBuilder(IServiceCollection services)
    {
        this.Services = services;
    }
}
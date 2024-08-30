using Microsoft.Extensions.DependencyInjection;

namespace SimpleEventBus.RabbitMq;

/// <summary>
/// The rabbit mq event bus builder extension class
/// </summary>
public static class RabbitMqEventBusBuilderExtension
{
    /// <summary>
    /// Configures the binding using the specified rabbit mq event bus builder
    /// </summary>
    /// <param name="rabbitMqEventBusBuilder">The rabbit mq event bus builder</param>
    /// <param name="setupRabbitmqBinding">The setup rabbitmq binding</param>
    /// <returns>The rabbit mq event bus builder</returns>
    public static IRabbitMqEventBusBuilder ConfigureBinding(this IRabbitMqEventBusBuilder rabbitMqEventBusBuilder,
                                                            Action<RabbitMqBindingOption> setupRabbitmqBinding)
    {
        rabbitMqEventBusBuilder.Services.Configure<RabbitMqBindingOption>(setupRabbitmqBinding);
        
        return rabbitMqEventBusBuilder;
    }
}
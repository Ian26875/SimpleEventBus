using Microsoft.Extensions.DependencyInjection;

namespace SimpleEventBus.RabbitMq;

public static class RabbitMqEventBusBuilderExtension
{
    public static IRabbitMqEventBusBuilder BindOption(this IRabbitMqEventBusBuilder rabbitMqEventBusBuilder,
                                                      Action<RabbitMqBindingOption> setupRabbitmqBinding)
    {
        rabbitMqEventBusBuilder.Services.Configure<RabbitMqBindingOption>(setupRabbitmqBinding);
        
        return rabbitMqEventBusBuilder;
    }
}
using Microsoft.Extensions.DependencyInjection;
using SimpleEventBus.RabbitMq;

namespace SimpleEventBus.DependencyInjection;

public static class EventBusBuilderExtensions
{
    public static IEventBusBuilder UseRabbitMq(this IEventBusBuilder eventBusBuilder,
        Action<RabbitMqOption> setUpOption, 
        Action<RabbitMqBindingOption> setUpBindOption)
    {
        
        eventBusBuilder.Services.AddSingleton<IEventPublisher,RabbitMqEventPublisher>();
        
        eventBusBuilder.Services.Configure<RabbitMqOption>(setUpOption);
        
        eventBusBuilder.Services.Configure<RabbitMqBindingOption>(setUpBindOption);
        
        return eventBusBuilder;
    }
}
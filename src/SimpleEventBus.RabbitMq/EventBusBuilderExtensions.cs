using SimpleEventBus.RabbitMq;

namespace SimpleEventBus.DependencyInjection;

public static class EventBusBuilderExtensions
{
    public static IEventBusBuilder UseRabbitMq(this IEventBusBuilder eventBusBuilder,
        Action<RabbitMqOption> setUpOption, 
        Action<RabbitMqBindOption> setUpBindOption)
    {
        
        
        
        
        
        
        return eventBusBuilder;
    }
}
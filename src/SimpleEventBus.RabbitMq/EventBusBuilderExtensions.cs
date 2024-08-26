using Microsoft.Extensions.DependencyInjection;
using SimpleEventBus.RabbitMq;

namespace SimpleEventBus.DependencyInjection;

/// <summary>
/// The event bus builder extensions class
/// </summary>
public static class EventBusBuilderExtensions
{
    /// <summary>
    /// Uses the rabbit mq using the specified event bus builder
    /// </summary>
    /// <param name="eventBusBuilder">The event bus builder</param>
    /// <param name="setUpOption">The set up option</param>
    /// <param name="setUpBindOption">The set up bind option</param>
    /// <returns>The event bus builder</returns>
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
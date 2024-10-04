using Microsoft.Extensions.DependencyInjection;
using SimpleEventBus.RabbitMq;
using SimpleEventBus.Schema;

namespace SimpleEventBus.DependencyInjection;

/// <summary>
///     The event bus builder extensions class
/// </summary>
public static class EventBusBuilderExtensions
{
    /// <summary>
    ///     Uses the rabbit mq using the specified event bus builder
    /// </summary>
    /// <param name="eventBusBuilder">The event bus builder</param>
    /// <param name="setUpOption">The set up option</param>
    /// <param name="setUpBindOption">The set up bind option</param>
    /// <returns>The event bus builder</returns>
    public static IEventBusBuilder UseRabbitMq(this IEventBusBuilder eventBusBuilder,
                                               Action<RabbitMqOption> setUpOption,
                                               Action<RabbitMqBindingOption> setUpBindOption)
    {
        eventBusBuilder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

        eventBusBuilder.Services.Configure(setUpOption);
        
 
        eventBusBuilder.Services.PostConfigure<RabbitMqBindingOption>(option =>
        {
            var scopeFactory = eventBusBuilder.Services.BuildServiceProvider()
                                              .GetRequiredService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var schemaRegistry = scope.ServiceProvider.GetRequiredService<ISchemaRegistry>();
                option.SchemaRegistry = schemaRegistry;
            }
            
            setUpBindOption(option);
        });
        
        return eventBusBuilder;
    }

    /// <summary>
    ///     Uses the rabbit mq using the specified event bus builder
    /// </summary>
    /// <param name="eventBusBuilder">The event bus builder</param>
    /// <param name="setUpOption">The set up option</param>
    /// <returns>The rabbit mq event bus builder</returns>
    public static IRabbitMqEventBusBuilder UseRabbitMq(this IEventBusBuilder eventBusBuilder,
                                                       Action<RabbitMqOption> setUpOption)
    {
        return new RabbitMqEventBusBuilder(eventBusBuilder.Services);
    }
}
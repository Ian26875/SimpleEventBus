using Microsoft.Extensions.DependencyInjection;
using SimpleEventBus;
using SimpleEventBus.Internal;
using SimpleEventBus.RabbitMq;
using SimpleEventBus.Schema;
using SimpleEventBus.Subscriber;

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
    public static IEventBusBuilder UseRabbitMqTransport(this IEventBusBuilder eventBusBuilder,
                                               Action<RabbitMqConnectionOption> setUpOption,
                                               Action<RabbitMqBindingOption> setUpBindOption)
    {
        eventBusBuilder.Services.AddSingleton<RabbitMqEventPublisher>();
        eventBusBuilder.Services.AddSingleton<IEventPublisher>(sp => sp.GetRequiredService<RabbitMqEventPublisher>());
        eventBusBuilder.Services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<RabbitMqEventPublisher>());

        eventBusBuilder.Services.AddSingleton<RabbitMqEventSubscriber>();
        eventBusBuilder.Services.AddSingleton<IEventSubscriber>(sp => sp.GetRequiredService<RabbitMqEventSubscriber>());
        eventBusBuilder.Services.AddSingleton<IInitializer, RabbitMqConnectionInitializer>();

        eventBusBuilder.Services.Configure(setUpOption);

        eventBusBuilder.Services.AddOptions<RabbitMqBindingOption>()
            .Configure<IEventMapper>((option, mapper) =>
            {
                option.EventMapper = mapper;
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
                                                       Action<RabbitMqConnectionOption> setUpOption)
    {
        return new RabbitMqEventBusBuilder(eventBusBuilder.Services);
    }
}

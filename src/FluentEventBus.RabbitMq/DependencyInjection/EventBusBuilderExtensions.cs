using Microsoft.Extensions.DependencyInjection;
using FluentEventBus;
using FluentEventBus.RabbitMq;
using FluentEventBus.Naming;
using FluentEventBus.Subscriber;

namespace FluentEventBus.DependencyInjection;

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
        eventBusBuilder.Services.AddSingleton<RabbitMqConnectionProvider>();

        // Transport-neutral server description consumed by documentation tooling (e.g. AsyncAPI).
        eventBusBuilder.Services.AddSingleton<IEventBusServerDescriptor>(serviceProvider =>
        {
            var connectionOption = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<RabbitMqConnectionOption>>().Value;
            return new EventBusServerDescriptor(
                Name: "rabbitmq",
                Host: connectionOption.Host,
                Protocol: "amqp",
                ProtocolVersion: "0.9.1",
                Description: "RabbitMQ broker configured via RabbitMqConnectionOption.");
        });

        eventBusBuilder.Services.AddSingleton<RabbitMqEventPublisher>();
        eventBusBuilder.Services.AddSingleton<IEventPublisher>(sp => sp.GetRequiredService<RabbitMqEventPublisher>());
        eventBusBuilder.Services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<RabbitMqEventPublisher>());

        eventBusBuilder.Services.AddSingleton<RabbitMqEventSubscriber>();
        eventBusBuilder.Services.AddSingleton<IEventSubscriber>(sp => sp.GetRequiredService<RabbitMqEventSubscriber>());

        eventBusBuilder.Services.Configure(setUpOption);

        eventBusBuilder.Services.AddOptions<RabbitMqBindingOption>()
            .Configure<IEventNameRegistry>((option, mapper) =>
            {
                option.EventNameRegistry = mapper;
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

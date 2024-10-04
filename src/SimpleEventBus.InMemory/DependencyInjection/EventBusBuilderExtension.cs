using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleEventBus.InMemory;
using SimpleEventBus.Subscriber;

namespace SimpleEventBus.DependencyInjection;

/// <summary>
///     The event bus builder extension class
/// </summary>
public static class EventBusBuilderExtension
{
    /// <summary>
    /// Uses the in memory using the specified event bus builder
    /// </summary>
    /// <param name="eventBusBuilder">The event bus builder</param>
    /// <param name="capacity">The capacity</param>
    /// <returns>The event bus builder</returns>
    public static IEventBusBuilder UseInMemory(this IEventBusBuilder eventBusBuilder,int capacity = 10)
    {
        eventBusBuilder.Services.AddSingleton<BackgroundQueue>(new BackgroundQueue(capacity));
        eventBusBuilder.Services.TryAddSingleton<IEventBus, InMemoryEventPublisher>();
        eventBusBuilder.Services.TryAddSingleton<IEventPublisher, InMemoryEventPublisher>();
        eventBusBuilder.Services.TryAddSingleton<IEventSubscriber, InMemoryEventSubscriber>();
        eventBusBuilder.Services.AddHostedService<QueuedHostedService>();
        return eventBusBuilder;
    }
}
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
    ///     Uses the in memory using the specified event bus builder
    /// </summary>
    /// <param name="eventBusBuilder">The event bus builder</param>
    /// <param name="capacity">The capacity</param>
    /// <param name="alertThreshold">The alert threshold</param>
    /// <param name="onAlert">The on alert</param>
    /// <returns>The event bus builder</returns>
    public static IEventBusBuilder UseInMemoryTransport(this IEventBusBuilder eventBusBuilder,
        int capacity = 100,
        int alertThreshold = 80,
        Func<Task>? onAlert = default)
    {
        eventBusBuilder.Services.AddSingleton(new BackgroundQueueOptions
        {
            Capacity = capacity,
            AlertThreshold = alertThreshold,
            OnAlert = onAlert
        });
        eventBusBuilder.Services.AddSingleton<BackgroundQueue>();
        eventBusBuilder.Services.AddSingleton<InMemoryEventPublisher>(); // Register the implementation as a singleton
        eventBusBuilder.Services.AddSingleton<IEventBus, InMemoryEventPublisher>(provider =>
            provider.GetRequiredService<InMemoryEventPublisher>());
        eventBusBuilder.Services.AddSingleton<IEventPublisher, InMemoryEventPublisher>(provider =>
            provider.GetRequiredService<InMemoryEventPublisher>());
        eventBusBuilder.Services.TryAddSingleton<IEventSubscriber, InMemoryEventSubscriber>();
        eventBusBuilder.Services.AddHostedService<QueuedHostedService>();
        return eventBusBuilder;
    }
}
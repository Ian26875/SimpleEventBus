using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleEventBus.InMemory;

namespace SimpleEventBus.DependencyInjection;

/// <summary>
/// The event bus builder extension class
/// </summary>
public static class EventBusBuilderExtension
{
    /// <summary>
    /// Uses the in memory using the specified event bus builder
    /// </summary>
    /// <param name="eventBusBuilder">The event bus builder</param>
    /// <returns>The event bus builder</returns>
    public static IEventBusBuilder UseInMemory(this IEventBusBuilder eventBusBuilder)
    {
        eventBusBuilder.Services.AddSingleton<BackgroundQueue>();
        eventBusBuilder.Services.TryAddSingleton<IEventBus,InMemoryEventBus>();
        eventBusBuilder.Services.AddHostedService<QueuedHostedService>();
        return eventBusBuilder;
    }
}
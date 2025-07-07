using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SimpleEventBus.Profile;
using SimpleEventBus.Subscriber;

namespace SimpleEventBus.DependencyInjection;

/// <summary>
/// Provides extension methods for configuring the Event Bus builder.
/// </summary>
public static class EventBusBuilderExtension
{
    /// <summary>
    /// Registers a subscription profile into the DI container.
    /// A subscription profile defines how event handlers are registered and executed.
    /// </summary>
    /// <typeparam name="TProfile">The type of the subscription profile to register.</typeparam>
    /// <param name="eventBusBuilder">The event bus builder instance.</param>
    /// <returns>The event bus builder instance for chaining.</returns>
    public static IEventBusBuilder WithProfile<TProfile>(this IEventBusBuilder eventBusBuilder)
        where TProfile : SubscriptionProfile
    {
        eventBusBuilder.Services.AddSingleton(typeof(SubscriptionProfile), typeof(TProfile));
        return eventBusBuilder;
    }

    /// <summary>
    /// Scans and registers all event handler implementations from the given assemblies.
    /// Automatically detects and registers types implementing IEventHandler&lt;TEvent&gt;.
    /// </summary>
    /// <param name="eventBusBuilder">The event bus builder instance.</param>
    /// <param name="assemblies">Assemblies to scan for event handlers.</param>
    /// <returns>The event bus builder instance for chaining.</returns>
    public static IEventBusBuilder ScanHandlersFrom(this IEventBusBuilder eventBusBuilder, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        
        var eventHandlerType = typeof(IEventHandler<>);

        var assemblyScanResults = from type in assemblies.SelectMany(x => x.GetExportedTypes().Distinct())
            where !type.IsAbstract && !type.IsGenericTypeDefinition
            let interfaces = type.GetInterfaces()
            let genericInterfaces = interfaces.Where(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == eventHandlerType)
            let matchingInterface = genericInterfaces.FirstOrDefault()
            where matchingInterface != null
            select (InterfaceType: matchingInterface, HandlerType: type);

        foreach (var scanResult in assemblyScanResults)
        {
            // Register handler interface (IEventHandler<T>) to concrete implementation
            eventBusBuilder.Services.Add(
                new ServiceDescriptor(scanResult.InterfaceType, scanResult.HandlerType, ServiceLifetime.Singleton));

            // Register the handler itself in case it's resolved directly (optional)
            eventBusBuilder.Services.Add(
                new ServiceDescriptor(scanResult.HandlerType, scanResult.HandlerType, ServiceLifetime.Singleton));
        }

        return eventBusBuilder;
    }

    /// <summary>
    /// Scans and registers all event handler implementations from the assembly of the specified type.
    /// This is a convenient overload of <see cref="ScanHandlersFrom(IEventBusBuilder, Assembly[])"/>.
    /// </summary>
    /// <typeparam name="T">A type contained in the target assembly.</typeparam>
    /// <param name="eventBusBuilder">The event bus builder instance.</param>
    /// <returns>The event bus builder instance for chaining.</returns>
    public static IEventBusBuilder ScanHandlersFrom<T>(this IEventBusBuilder eventBusBuilder)
    {
        return eventBusBuilder.ScanHandlersFrom(typeof(T).Assembly);
    }
}

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleEventBus.ExceptionHandlers;
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
        
        var eventHandlerInterface = typeof(IEventHandler<>);
        var exceptionHandlerInterface = typeof(IEventExceptionHandler);

        var allTypes = assemblies
            .SelectMany(x => x.GetExportedTypes().Distinct())
            .Where(type => type is {IsAbstract: false, IsGenericTypeDefinition: false})
            .ToList();

        foreach (var type in allTypes)
        {
            var interfaces = type.GetInterfaces();

            foreach (var interfaceType in interfaces.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == eventHandlerInterface))
            {
                eventBusBuilder.Services.TryAdd(new ServiceDescriptor(interfaceType, type, ServiceLifetime.Singleton));
                eventBusBuilder.Services.TryAdd(new ServiceDescriptor(type, type, ServiceLifetime.Singleton));
            }
            
            if (exceptionHandlerInterface.IsAssignableFrom(type))
            {
                eventBusBuilder.Services.TryAdd(new ServiceDescriptor(typeof(IEventExceptionHandler), type, ServiceLifetime.Singleton));
                eventBusBuilder.Services.TryAdd(new ServiceDescriptor(type, type, ServiceLifetime.Singleton));
            }
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

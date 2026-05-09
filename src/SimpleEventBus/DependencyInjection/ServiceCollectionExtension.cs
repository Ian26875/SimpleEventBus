using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleEventBus.ExceptionHandlers;
using SimpleEventBus.Internal;
using SimpleEventBus.Mapper;
using SimpleEventBus.Profile;
using SimpleEventBus.Schema;
using SimpleEventBus.Serialization;
using SimpleEventBus.Subscriber;

namespace SimpleEventBus.DependencyInjection;

/// <summary>
/// The service collection extension class
/// </summary>
public static class ServiceCollectionExtension
{

    /// <summary>
    /// Adds the event bus using the specified services
    /// </summary>
    /// <param name="services">The services</param>
    /// <param name="configureBuilder">The configure builder</param>
    /// <returns>The services</returns>
    public static IServiceCollection AddEventBus(this IServiceCollection services, 
                                                 Action<IEventBusBuilder> configureBuilder)
    {
        ArgumentNullException.ThrowIfNull(configureBuilder);
        
        // Subscriber
        services.TryAddSingleton<IEventHandlerInvoker, DefaultEventHandlerInvoker>();
        
        // Profile
        services.TryAddSingleton<ISubscriptionProfileManager,SubscriptionProfileManager>();
        
        // Internal
        services.AddSingleton<IInitializer, EventSubscribeInitializer>();
        
        // Exception
        services.AddSingleton<IExceptionHandlerInvoker, ExceptionHandlerInvoker>();
        
        // ApplicationBootstrapper
        services.AddSingleton<IApplicationBootstrapper, DefaultApplicationBootstrapper>();

        services.AddSingleton<ISerializer, JsonSerializer>();
        
        services.AddSingleton<IEventMapper>(EventMapper.Instance);
        
        // EventBus
       
        var eventBusBuilder = new EventBusBuilder(services);
        configureBuilder(eventBusBuilder);
            
        services.AddHostedService<DefaultApplicationBootstrapper>();
        
        return services;
    }

    /// <summary>
    /// Adds the event bus and registers a single subscription profile with optional handler scanning.
    /// </summary>
    /// <typeparam name="TProfile">The subscription profile type.</typeparam>
    /// <param name="services">The services.</param>
    /// <param name="assemblies">Assemblies to scan for handlers. If empty, no scanning is performed.</param>
    /// <returns>The services.</returns>
    public static IServiceCollection AddEventBusWithProfile<TProfile>(this IServiceCollection services,
                                                                      params Assembly[] assemblies)
        where TProfile : SubscriptionProfile
    {
        ArgumentNullException.ThrowIfNull(services);
        assemblies ??= Array.Empty<Assembly>();

        return services.AddEventBus(builder =>
        {
            builder.WithProfile<TProfile>();
            if (assemblies.Length > 0)
            {
                builder.ScanHandlersFrom(assemblies);
            }
        });
    }

    /// <summary>
    /// Adds the event bus, registers a single subscription profile, and scans handlers from the marker type's assembly.
    /// </summary>
    /// <typeparam name="TProfile">The subscription profile type.</typeparam>
    /// <typeparam name="TMarker">A marker type in the target assembly.</typeparam>
    /// <param name="services">The services.</param>
    /// <returns>The services.</returns>
    public static IServiceCollection AddEventBusWithProfile<TProfile, TMarker>(this IServiceCollection services)
        where TProfile : SubscriptionProfile
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddEventBusWithProfile<TProfile>(typeof(TMarker).Assembly);
    }
}

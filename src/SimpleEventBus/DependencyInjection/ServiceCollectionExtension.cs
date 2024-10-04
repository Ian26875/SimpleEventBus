using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleEventBus.ExceptionHandlers;
using SimpleEventBus.Internal;
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
        // Subscriber
        services.TryAddSingleton<IEventHandlerInvoker, DefaultEventHandlerInvoker>();
        // Profile
        services.TryAddSingleton<ISubscriptionProfileManager,SubscriptionProfileManager>();
        
        // Internal
        services.AddSingleton<IInitializer, EventSubscribeInitializer>();
        
        // Exception
        services.AddSingleton<IExceptionHandlerPipeline, ExceptionHandlerPipeline>();
        
        // ApplicationBootstrapper
        services.AddSingleton<IApplicationBootstrapper, DefaultApplicationBootstrapper>();
        
        services.AddSingleton<IServiceScopeFactory>(p => p.GetRequiredService<IServiceScopeFactory>());

        services.AddSingleton<ISerializer, JsonSerializer>();
        
        services.AddSingleton<ISchemaRegistry>(SchemaRegistry.Instance);
        
        var eventBusBuilder = new EventBusBuilder(services);
        configureBuilder(eventBusBuilder);
            
        services.AddHostedService<DefaultApplicationBootstrapper>();
        
        return services;
    }
}
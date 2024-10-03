using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleEventBus.ExceptionHandlers;
using SimpleEventBus.Internal;
using SimpleEventBus.Profile;
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
    /// <param name="configureOptions">The configure options</param>
    /// <returns>The services</returns>
    public static IServiceCollection AddEventBus(this IServiceCollection services, 
                                                 Action<IEventBusBuilder> configureBuilder, 
                                                 Action<EventBusOption>? configureOptions = default(Action<EventBusOption>?))
    {
        // Subscriber
        services.TryAddSingleton<IEventHandlerInvoker, DefaultEventHandlerInvoker>();
        services.TryAddSingleton<IEventHandlerResolver, DefaultEventHandlerResolver>();
        
        // Profile
        services.TryAddSingleton<ISubscriptionProfileManager,SubscriptionProfileManager>();
        
        // Internal
        services.AddSingleton<IInitializer, EventSubscribeInitializer>();
        
        // Exception
        services.AddSingleton<IEventExceptionHandlerResolver, DefaultEventExceptionHandlerResolver>();
        
        var options = new EventBusOption();
        configureOptions?.Invoke(options);
        
        services.AddSingleton(options);
        
        var eventBusBuilder = new EventBusBuilder(services);
        configureBuilder(eventBusBuilder);
        
       
        
        return services;
    }
}
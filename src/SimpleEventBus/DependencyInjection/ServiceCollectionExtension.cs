using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        services.TryAddSingleton<IEventHandlerInvoker, DefaultEventHandlerInvoker>();
        services.TryAddSingleton<IEventHandlerResolver, DefaultEventHandlerResolver>();
        
        var options = new EventBusOption();
        configureOptions?.Invoke(options);
        
        services.AddSingleton(options);
        
        var eventBusBuilder = new EventBusBuilder(services);
        configureBuilder(eventBusBuilder);
        
        services.AddSingleton(sp=>new SubscriptionProfileAggregator(sp.GetServices<SubscriptionProfile>()));
        
        return services;
    }
}
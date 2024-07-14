using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SimpleEventBus.DependencyInjection;

/// <summary>
/// The service collection extension class
/// </summary>
public static class ServiceCollectionExtension
{

    public static IServiceCollection AddEventBus(this IServiceCollection services, Action<IEventBusBuilder> configureBuilder, Action<EventBusOption> configureOptions = null)
    {
        services.TryAddSingleton<IEventHandlerInvoker, DefaultEventHandlerInvoker>();
        services.TryAddSingleton<ISubscriptionManager, SubscriptionManager>();
        
        var options = new EventBusOption();
        configureOptions?.Invoke(options);
        
        services.AddSingleton(options);
        
        var eventBusBuilder = new EventBusBuilder(services);
        configureBuilder(eventBusBuilder);

        return services;
    }
}
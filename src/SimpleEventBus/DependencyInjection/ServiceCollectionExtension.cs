using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace SimpleEventBus.DependencyInjection;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddEventBus(this IServiceCollection services, Action<IEventBusBuilder> setUp)
    {
        
        services.TryAddSingleton<IEventHandlerInvoker,DefaultEventHandlerInvoker>();
        
        
        var eventBusBuilder = new EventBusBuilder(services);
        setUp(eventBusBuilder);
        
        
        
        return services;
    }
}
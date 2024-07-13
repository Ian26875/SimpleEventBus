using Microsoft.Extensions.DependencyInjection;
using SimpleEventBus.DependencyInjection;

namespace SimpleEventBus.InMemory;

public static class EventBusBuilderExtension
{
    public static IEventBusBuilder UseInMemory(this IEventBusBuilder eventBusBuilder)
    {
        eventBusBuilder.Services.AddSingleton<BackgroundQueue>();
        
        return eventBusBuilder;
    }
}
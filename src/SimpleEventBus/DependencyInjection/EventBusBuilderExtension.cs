using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SimpleEventBus.Profile;
using SimpleEventBus.Subscriber;

namespace SimpleEventBus.DependencyInjection;

/// <summary>
/// The event bus builder extension class
/// </summary>
public static class EventBusBuilderExtension
{
    /// <summary>
    /// Adds the profile using the specified event bus builder
    /// </summary>
    /// <typeparam name="TProfile">The profile</typeparam>
    /// <param name="eventBusBuilder">The event bus builder</param>
    /// <returns>The event bus builder</returns>
    public static IEventBusBuilder AddProfile<TProfile>(this IEventBusBuilder eventBusBuilder) where TProfile : SubscriptionProfile
    {
        eventBusBuilder.Services.AddSingleton(typeof(SubscriptionProfile), typeof(TProfile));
        
        return eventBusBuilder;
    }
    
      /// <summary>
        /// Adds the handlers from assemblies using the specified event bus builder
        /// </summary>
        /// <param name="eventBusBuilder">The event bus builder</param>
        /// <param name="assemblies">The assemblies</param>
        /// <returns>The event bus builder</returns>
        public static IEventBusBuilder AddHandlersFromAssemblies(this IEventBusBuilder eventBusBuilder,
            params Assembly[] assemblies)
        {
            var eventHandlerType = typeof(IEventHandler<>);
            var assemblyScanResults = from type in assemblies.SelectMany(x=>x.GetExportedTypes().Distinct())
                                      where type.IsAbstract.Equals(false) && type.IsGenericTypeDefinition.Equals(false)
                                      let interfaces = type.GetInterfaces()
                                      let genericInterfaces = interfaces.Where(i =>
                                                                                   i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == eventHandlerType)
                                      let matchingInterface = genericInterfaces.FirstOrDefault()
                                      where matchingInterface != null
                                      select (InterfaceType: matchingInterface, HandlerType: type);

            foreach (var scanResult in assemblyScanResults)
            {
                eventBusBuilder.Services.Add
                (
                    new ServiceDescriptor
                    (
                        serviceType: scanResult.InterfaceType,
                        implementationType: scanResult.HandlerType,
                        lifetime: ServiceLifetime.Scoped
                    )
                );
                
                eventBusBuilder.Services.Add
                (
                    new ServiceDescriptor
                    (
                        serviceType: scanResult.HandlerType,
                        implementationType: scanResult.HandlerType,
                        lifetime: ServiceLifetime.Scoped
                    )
                );
            }
            return eventBusBuilder;
        }
}
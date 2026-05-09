using Microsoft.Extensions.DependencyInjection;

namespace SimpleEventBus.DependencyInjection;

/// <summary>
/// Defines the interface for building an Event Bus and accessing its registered services.
/// </summary>
public interface IEventBusBuilder
{
    /// <summary>
    /// Gets the <see cref="IServiceCollection"/> used to register services into the dependency injection container.
    /// </summary>
    IServiceCollection Services { get; }
}


/// <summary>
/// Provides a concrete implementation of <see cref="IEventBusBuilder"/> for configuring Event Bus-related services.
/// </summary>
public class EventBusBuilder : IEventBusBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventBusBuilder"/> class with the specified service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance to register services with.</param>
    public EventBusBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <summary>
    /// Gets the <see cref="IServiceCollection"/> used to register services into the dependency injection container.
    /// </summary>
    public IServiceCollection Services { get; }
}
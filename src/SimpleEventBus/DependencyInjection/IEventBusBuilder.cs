using Microsoft.Extensions.DependencyInjection;

namespace SimpleEventBus.DependencyInjection;

/// <summary>
/// The event bus builder interface
/// </summary>
public interface IEventBusBuilder
{
    /// <summary>
    /// Gets the value of the services
    /// </summary>
    IServiceCollection Services { get; }
}

/// <summary>
/// The event bus builder class
/// </summary>
/// <seealso cref="IEventBusBuilder"/>
public class EventBusBuilder : IEventBusBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventBusBuilder"/> class
    /// </summary>
    /// <param name="services">The services</param>
    public EventBusBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <summary>
    /// Gets the value of the services
    /// </summary>
    public IServiceCollection Services { get; }
    
}
using Microsoft.Extensions.DependencyInjection;

namespace SimpleEventBus.DependencyInjection;

public interface IEventBusBuilder
{
    IServiceCollection Services { get; }

    void AddProfile<TProfile>() where TProfile : SubscriptionProfile;
}

public class EventBusBuilder : IEventBusBuilder
{
    public EventBusBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IServiceCollection Services { get; }

    private readonly Dictionary<string, HashSet<Type>> registeredHandlers = new();
    public void AddProfile<TProfile>() where TProfile : SubscriptionProfile
    {
        Services.AddSingleton(typeof(SubscriptionProfile),typeof(TProfile));
    }
}
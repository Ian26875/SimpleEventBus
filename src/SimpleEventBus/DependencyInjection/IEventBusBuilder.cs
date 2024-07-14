using Microsoft.Extensions.DependencyInjection;

namespace SimpleEventBus.DependencyInjection;

public interface IEventBusBuilder
{
    IServiceCollection Services { get; }

    void AddProfile<TProfile>(TProfile profile) where TProfile : SubscriptionProfile;
}

public class EventBusBuilder : IEventBusBuilder
{
    public EventBusBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IServiceCollection Services { get; }


    public void AddProfile<TProfile>(TProfile profile) where TProfile : SubscriptionProfile
    {
        Services.AddSingleton<SubscriptionProfile>(profile);
    }
}
namespace SimpleEventBus;

public static class EventBusExtension
{
    public static void Publish<TEvent>(IEventBus eventBus,TEvent @event) where TEvent : class
    {
        eventBus.PublishAsync(@event).ConfigureAwait(false).GetAwaiter().GetResult();
    }
}
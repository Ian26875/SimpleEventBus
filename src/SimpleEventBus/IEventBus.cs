namespace SimpleEventBus;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, Headers? headers = null, CancellationToken cancellationToken = default) where TEvent : class;
}
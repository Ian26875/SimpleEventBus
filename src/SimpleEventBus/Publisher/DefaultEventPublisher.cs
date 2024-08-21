using SimpleEventBus.Event;

namespace SimpleEventBus;

public abstract class AbstractEventPublisher : IEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent @event, Headers? headers = null,
                                     CancellationToken cancellationToken = default(CancellationToken)) where TEvent : class
    {
        if (@event is null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        headers ??= new Headers();

        var eventContext = new EventContext<TEvent>(@event, headers);

        return this.PublishEventAsync(eventContext, cancellationToken);
    }

    protected abstract Task PublishEventAsync<TEvent>(EventContext<TEvent> eventContext,
                                             CancellationToken cancellationToken = default(CancellationToken)) where TEvent : class;
}
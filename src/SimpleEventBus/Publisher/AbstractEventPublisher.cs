using SimpleEventBus.Event;
using SimpleEventBus.Schema;

namespace SimpleEventBus;

public abstract class AbstractEventBus : IEventBus
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

    public abstract void Dispose();
    
    public Task SubscribeAsync(Type eventType, Type eventHandler)
    {
        SchemaRegistry.Instance.Register(eventType);
        
        
        return Task.CompletedTask;
    }
}
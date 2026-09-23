using FluentEventBus.Event;
using FluentEventBus.Metrics;
using FluentEventBus.Naming;
using FluentEventBus.Serialization;

namespace FluentEventBus;

public abstract class AbstractEventPublisher : IEventBus
{
    protected readonly ISerializer _serializer;

    protected readonly IEventNameRegistry EventNameRegistry;
    
    protected AbstractEventPublisher(ISerializer serializer, IEventNameRegistry eventMapper)
    {
        _serializer = serializer;
        EventNameRegistry = eventMapper;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, Headers? headers = null,
                                     CancellationToken cancellationToken = default(CancellationToken)) where TEvent : class
    {
        ArgumentNullException.ThrowIfNull(@event);

        headers ??= new Headers();

        // Standard envelope headers: consumers rely on MessageId for at-least-once
        // deduplication, so every published message must carry one.
        headers.MessageId ??= Guid.NewGuid().ToString();
        headers.OccurredAt ??= DateTimeOffset.UtcNow;

        var serializedData = _serializer.Serialize(@event);

        var eventName = EventNameRegistry.GetEventName(typeof(TEvent));

        using var activity = EventBusActivitySource.StartPublish(eventName, headers);

        var eventData = new EventContext
        (
            serializedData,
            headers,
            eventName
        );

        try
        {
            await this.PublishEventAsync(eventData, cancellationToken);
            EventBusMetrics.AddPublished(eventName);
        }
        catch (Exception exception)
        {
            EventBusMetrics.AddFailure(eventName);
            EventBusActivitySource.RecordFailure(activity, exception);
            throw;
        }
    }

    protected abstract Task PublishEventAsync(EventContext eventContext, CancellationToken cancellationToken = default(CancellationToken));

}
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleEventBus.Event;
using SimpleEventBus.Profile;
using SimpleEventBus.Schema;
using SimpleEventBus.Subscriber;

namespace SimpleEventBus.Internal;

/// <summary>
/// The event subscribe initializer class
/// </summary>
/// <seealso cref="IInitializer"/>
public class EventSubscribeInitializer : IInitializer
{
    /// <summary>
    /// The event subscriber
    /// </summary>
    private readonly IEventSubscriber _eventSubscriber;

    /// <summary>
    /// The subscription profile manager
    /// </summary>
    private readonly ISubscriptionProfileManager _subscriptionProfileManager;

    /// <summary>
    /// The logger
    /// </summary>
    private readonly ILogger<EventSubscribeInitializer> _logger;


    /// <summary>
    /// The service provider
    /// </summary>
    private readonly IServiceScopeFactory _serviceScopeFactory;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="EventSubscribeInitializer"/> class
    /// </summary>
    /// <param name="eventSubscriber">The event subscriber</param>
    /// <param name="subscriptionProfileManager">The subscription profile manager</param>
    /// <param name="logger">The logger</param>
    /// <param name="serviceScopeFactory">The service scope factory</param>
    public EventSubscribeInitializer(IEventSubscriber eventSubscriber, 
                                     ISubscriptionProfileManager subscriptionProfileManager, 
                                     ILogger<EventSubscribeInitializer> logger, 
                                     IServiceScopeFactory serviceScopeFactory)
    {
        _eventSubscriber = eventSubscriber;
        _subscriptionProfileManager = subscriptionProfileManager;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }
    
    /// <summary>
    /// Initializes the cancellation token
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var eventTypes = _subscriptionProfileManager.GetAllEventTypes();

        var eventNames = eventTypes.Select(eventType => SchemaRegistry.Instance.GetEventName(eventType)).ToList();
        
        await this._eventSubscriber.SubscribeAsync(eventNames);

        _eventSubscriber.ConsumerReceived = ConsumerReceived;
    }

    /// <summary>
    /// Consumers the received using the specified body
    /// </summary>
    /// <param name="body">The body</param>
    /// <param name="headers">The headers</param>
    /// <param name="eventName">The event name</param>
    private async Task ConsumerReceived(ReadOnlyMemory<byte> body, Headers headers,string eventName)
    {
        string messageContent = Encoding.UTF8.GetString(body.Span);
        
        await ProcessEventAsync(eventName, messageContent, headers);
    }
    
    /// <summary>
    /// Processes the event using the specified event name
    /// </summary>
    /// <param name="eventName">The event name</param>
    /// <param name="message">The message</param>
    /// <param name="headers">The headers</param>
    private async Task ProcessEventAsync(string eventName, string message, Headers headers)
    {
        _logger.LogTrace($"Processing RabbitMQ event: {eventName}...");

        var eventType = SchemaRegistry.Instance.GetEventType(eventName);
        
        if (_subscriptionProfileManager.HasSubscriptionsForEvent(eventType).Equals(obj: false))
        {
            _logger.LogTrace(message: "There are no subscriptions for this event.");
            return;
        }
        
        var @event = JsonSerializer.Deserialize(message, eventType, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        await using (var serviceScope = _serviceScopeFactory.CreateAsyncScope())
        {
            var eventHandlerInvoker = serviceScope.ServiceProvider.GetRequiredService<IEventHandlerInvoker>();
            
            await eventHandlerInvoker.InvokeAsync(@event, headers);
        }

        _logger.LogTrace($"Processed event {eventName}.");
    }
}
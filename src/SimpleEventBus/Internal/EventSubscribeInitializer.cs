using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleEventBus.Event;
using SimpleEventBus.Profile;
using SimpleEventBus.Schema;
using SimpleEventBus.Serialization;
using SimpleEventBus.Subscriber;

namespace SimpleEventBus.Internal;

/// <summary>
///     The event subscribe initializer class
/// </summary>
/// <seealso cref="IInitializer" />
public class EventSubscribeInitializer : IInitializer
{
    /// <summary>
    ///     The event subscriber
    /// </summary>
    private readonly IEventSubscriber _eventSubscriber;

    /// <summary>
    ///     The logger
    /// </summary>
    private readonly ILogger<EventSubscribeInitializer> _logger;

    /// <summary>
    ///     The schema registry
    /// </summary>
    private readonly ISchemaRegistry _schemaRegistry;


    /// <summary>
    ///     The service provider
    /// </summary>
    private readonly IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    ///     The subscription profile manager
    /// </summary>
    private readonly ISubscriptionProfileManager _subscriptionProfileManager;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EventSubscribeInitializer" /> class
    /// </summary>
    /// <param name="eventSubscriber">The event subscriber</param>
    /// <param name="subscriptionProfileManager">The subscription profile manager</param>
    /// <param name="logger">The logger</param>
    /// <param name="serviceScopeFactory">The service scope factory</param>
    /// <param name="schemaRegistry">The schema registry</param>
    public EventSubscribeInitializer(IEventSubscriber eventSubscriber,
                                     ISubscriptionProfileManager subscriptionProfileManager,
                                     ILogger<EventSubscribeInitializer> logger,
                                     IServiceScopeFactory serviceScopeFactory,
                                     ISchemaRegistry schemaRegistry)
    {
        _eventSubscriber = eventSubscriber;
        _subscriptionProfileManager = subscriptionProfileManager;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _schemaRegistry = schemaRegistry;
    }

    /// <summary>
    ///     Initializes the cancellation token
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var eventTypes = _subscriptionProfileManager.GetAllEventTypes();

        var eventNames = eventTypes.Select(eventType => _schemaRegistry.GetEventName(eventType)).ToList();

        await _eventSubscriber.SubscribeAsync(eventNames);

        _eventSubscriber.ConsumerReceived += ConsumerReceived;
    }

    /// <summary>
    ///     Consumers the received using the specified event data
    /// </summary>
    /// <param name="eventData">The event data</param>
    private async Task ConsumerReceived(EventData eventData)
    {
        var messageContent = Encoding.UTF8.GetString(eventData.Data.Span);

        await ProcessEventAsync(eventData.EventName, messageContent, eventData.Headers);
    }

    /// <summary>
    ///     Processes the event using the specified event name
    /// </summary>
    /// <param name="eventName">The event name</param>
    /// <param name="message">The message</param>
    /// <param name="headers">The headers</param>
    private async Task ProcessEventAsync(string eventName, string message, Headers headers)
    {
        _logger.LogTrace($"Processing RabbitMQ event: {eventName}...");
        
        await using (var serviceScope = _serviceScopeFactory.CreateAsyncScope())
        {
            var serviceProvider = serviceScope.ServiceProvider;
            
            var eventType = serviceProvider.GetRequiredService<ISchemaRegistry>().GetEventType(eventName);
            
            var serializer = serviceProvider.GetRequiredService<ISerializer>();

            var @event = serializer.Deserialize(message, eventType);

            var eventHandlerInvoker = serviceProvider.GetRequiredService<IEventHandlerInvoker>();

            await eventHandlerInvoker.InvokeAsync(@event, headers);
        }

        _logger.LogTrace($"Processed event {eventName}.");
    }
}
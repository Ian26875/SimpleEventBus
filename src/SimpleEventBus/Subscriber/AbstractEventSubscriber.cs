using SimpleEventBus.Event;
using SimpleEventBus.ExceptionHandlers;
using SimpleEventBus.Profile;

namespace SimpleEventBus.Subscriber;

/// <summary>
/// The abstract event subscriber class
/// </summary>
/// <seealso cref="IEventSubscriber"/>
public abstract class AbstractEventSubscriber : IEventSubscriber
{

    /// <summary>
    /// The event handler invoker
    /// </summary>
    private readonly IEventHandlerInvoker _eventHandlerInvoker;

    /// <summary>
    /// The event exception handler
    /// </summary>
    private readonly IEventExceptionHandler _eventExceptionHandler;

    /// <summary>
    /// The subscription profile manager
    /// </summary>
    private readonly ISubscriptionProfileManager _subscriptionProfileManager;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractEventSubscriber"/> class
    /// </summary>
    /// <param name="eventHandlerInvoker">The event handler invoker</param>
    /// <param name="eventExceptionHandler">The event exception handler</param>
    /// <param name="subscriptionProfileManager">The subscription profile manager</param>
    protected AbstractEventSubscriber(IEventHandlerInvoker eventHandlerInvoker, 
                                      IEventExceptionHandler eventExceptionHandler, 
                                      ISubscriptionProfileManager subscriptionProfileManager)
    {
        _eventHandlerInvoker = eventHandlerInvoker;
        _eventExceptionHandler = eventExceptionHandler;
        _subscriptionProfileManager = subscriptionProfileManager;
    }

    /// <summary>
    /// Registers the cancellation token
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The value task</returns>
    public ValueTask RegisterAsync(CancellationToken cancellationToken)
    {
        
        
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Gets or sets the value of the consumer received
    /// </summary>
    public Func<ReadOnlyMemory<byte>, IDictionary<string, string>, Task> ConsumerReceived { get; set; }
    
    /// <summary>
    /// Receives the message using the specified event
    /// </summary>
    /// <typeparam name="TEvent">The event</typeparam>
    /// <param name="event">The event</param>
    /// <param name="headers">The headers</param>
    public async Task ReceiveMessageAsync<TEvent>(TEvent @event, Headers headers) where TEvent : class
    {
        var eventContext = new EventContext<TEvent>(@event, headers);
        try
        {
            await this._eventHandlerInvoker.InvokeAsync(eventContext);
        }
        catch (Exception exception)
        {
            var exceptionContext = new ExceptionContext(@event, headers, exception);
            this._eventExceptionHandler.OnException(exceptionContext);
        }
    }
}
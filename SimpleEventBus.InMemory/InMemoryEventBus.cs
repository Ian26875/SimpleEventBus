namespace SimpleEventBus.InMemory;

public class InMemoryEventBus : IEventBus
{

    private readonly IEventHandlerInvoker _eventHandlerInvoker;

    public InMemoryEventBus(IEventHandlerInvoker eventHandlerInvoker)
    {
        _eventHandlerInvoker = eventHandlerInvoker;
    }
    
    public async Task PublishAsync<TEvent>(TEvent @event, Headers headers, CancellationToken cancellationToken = default) where TEvent : class
    {
        ArgumentNullException.ThrowIfNull(@event);
        
        await _eventHandlerInvoker.InvokeAsync(@event);
    }
}
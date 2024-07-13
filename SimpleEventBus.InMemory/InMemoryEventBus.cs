namespace SimpleEventBus.InMemory;

public class InMemoryEventBus : IEventBus
{
     
    
    
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
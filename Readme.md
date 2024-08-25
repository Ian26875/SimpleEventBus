# SimpleEventBus

Provider Simple event bus and fluent settings




## Publish


```csharp!


private IEventPublisher _eventPublisher;


public async Task<IActionResult> Index()
{
    var orderPlacedEvent = new OrderPlacedEvent
    {
        
    };
    
    await _eventPublisher.PublisherAsync(orderPlacedEvent);
}


```

## Subscripe


```csharp!
public class OrderPlacedEvent 
{
    
}

public class EmailService : IEventHandler<OrderPlacedEvent> 
{

    public Task HandleAsync(OrderPlacedEvent @event,Headers headers, CancellationToken cancellationToken)
    {
    
    
    }
}


public class SmsService : IEventHandler<OrderPlacedEvent> 
{

    public Task HandleAsync(OrderPlacedEvent @event,Headers headers, CancellationToken cancellationToken)
    {
    
    
    }
}


public class ExceptionLoggingHandler : IHandlerExceptionHandler
{
    private ILogger<LoggingService> _logger;

    public ExceptionLoggingHandler(ILogger<LoggingService> logger)
    {
        this._logger = logger;
    }

    public void OnException(ErrorContext errorContext)
    {
        
    }
}
```



`CustomProfile.cs`

```csharp!

public class CustomProfile : SubscriptionProfile
{
    public CustomProfile()
    {
        WhenOccurs<OrderPlacedEvent>().ToDo<EmailService>() 
                                      .ToDo<SmsService>()
                                      .CatchExceptionToDo<ExceptionLoggingHandler>();
    }
}


```

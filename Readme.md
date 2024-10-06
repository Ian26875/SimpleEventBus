# SimpleEventBus

Provider Simple event bus and fluent settings




## Publish


```csharp


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


```csharp
public class OrderPlacedEvent 
{
    
}
```


There are two ways to implement an EventHandler. The first is by implementing the IEventHandler interface, and the second is by implementing a delegate method.

### Implementing the `IEventHandler<TEvent>` interface

```csharp

public class EmailService : IEventHandler<OrderPlacedEvent> 
{

    public Task HandleAsync(OrderPlacedEvent @event,Headers headers, CancellationToken cancellationToken)
    {
    
    
    }
}

```

`CustomProfile.cs`

```csharp

public class OrderSubscriptionProfile : SubscriptionProfile
{
    public CustomProfile()
    {
        WhenOccurs<OrderPlacedEvent>().ToDo<EmailService>() 
                                      .CatchExceptionToDo<ExceptionLoggingHandler>();
    }
}
```

### Implementing the `Func<object,IDictionary<string,object>,CancellationToken>` method

```csharp

public class EmailService : IEmailService
{

	public Task SendAsync(OrderPlacedEvent @event, IDictionary<string,object> headers, CancellationToken cancellationToken)
	{
		// 
	}
}

```

`CustomProfile.cs`

```csharp

public class OrderSubscriptionProfile : SubscriptionProfile
{
    public CustomProfile()
    {
        WhenOccurs<OrderPlacedEvent>().ToDo<IEmailService>(s=>s.SendAsync) 
                                      .CatchExceptionToDo<ExceptionLoggingHandler>();
    }
}
```





--- 

## SimpleEventBus.RabbitMq


```csharp
void Main()
{
	IServiceCollection services = new ServiceCollection();
	services.AddEventBus
	(
		e => e.UseRabbitMq(o => 
		{
			o.UserName = "guest";
		},
		RabbitMqBindingConfiguration.ConfigureBindings  
	));
	
}

public static class RabbitMqBindingConfiguration
{
	public static void ConfigureBindings(RabbitMqBindingOption option) 
	{
		option.DeclareGlobalExchange("sample.exchange")
              .DeclareGlobalQueue("sample.queue");
		
		option.ForEvent<OrderPlacedEvent>()
              .DeclareExchange("sample.exchange.order")
              .DeclareQueue(nameof(OrderPlacedEvent));
	}
}

public class OrderPlacedEvent 
{
	
}
```
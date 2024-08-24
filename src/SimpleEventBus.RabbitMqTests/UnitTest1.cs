using SimpleEventBus.RabbitMq;

namespace SimpleEventBus.RabbitMqTests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var bindOption = new RabbitMqBindingOption();

        bindOption.ForEvent<OrderCreatedEvent>()
                  .DeclareExchange("")
                  .DeclareQueue("");

        
        
        
    }
    
    
    public class OrderCreatedEvent
    {
        
    }
}
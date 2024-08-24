namespace SimpleEventBus.RabbitMq;

public class RabbitMqBindOption
{
    public Dictionary<Type, string> BindExchanges { get; set; } = new Dictionary<Type, string>();

    public Dictionary<Type, string> BindQueues { get; set; } = new Dictionary<Type, string>();
    
    
    
    
    
}


public static class RabbitMqBindBuilder
{
    public static RabbitMqBindOption BindExchange<TEvent>(this RabbitMqBindOption option,string exchangeName)
    {
        option.BindExchanges.TryAdd(typeof(TEvent), exchangeName);
        return option;
    }
    
    public static RabbitMqBindOption BindQueue<TEvent>(this RabbitMqBindOption option,string queueName)
    {
        option.BindQueues.TryAdd(typeof(TEvent), queueName);
        return option;
    }
}
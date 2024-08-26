namespace SimpleEventBus.Subscriber;

public class Subscriptions
{
    public Dictionary<string, Type> EventMapper { get; set; } = new Dictionary<string, Type>();

    
}
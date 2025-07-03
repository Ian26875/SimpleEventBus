namespace SimpleEventBus.Serialization;

public interface ISerializer
{ 
    ReadOnlyMemory<byte> Serialize<TEvent>(TEvent @event);
    
   object? Deserialize(string content, Type type);
}
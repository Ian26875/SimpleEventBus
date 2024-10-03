using System.Text.Json;
using SimpleEventBus.Event;

namespace SimpleEventBus.Serialization;

public interface ISerializer
{ 
    ReadOnlyMemory<byte> Serialize<TEvent>(TEvent @event);
    
   object Deserialize(string content, Type type);
}

public class JsonSerializer : ISerializer
{
    public ReadOnlyMemory<byte> Serialize<TEvent>(TEvent @event)
    {
        return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(@event);
    }

    public object Deserialize(string content, Type type)
    {
        return System.Text.Json.JsonSerializer.Deserialize(content, type,
            new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
    }
}
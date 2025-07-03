using System.Text.Json;

namespace SimpleEventBus.Serialization;

public class JsonSerializer : ISerializer
{
    public ReadOnlyMemory<byte> Serialize<TEvent>(TEvent @event)
    {
        return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(@event);
    }

    public object? Deserialize(string content, Type type)
    {
        return System.Text.Json.JsonSerializer.Deserialize
        (
            content, 
            type,
            new JsonSerializerOptions {PropertyNameCaseInsensitive = true}
        );
    }
}
using System.Text.Json;

namespace SimpleEventBus.Serialization;

public class JsonSerializer : ISerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ReadOnlyMemory<byte> Serialize<TEvent>(TEvent @event)
    {
        return System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(@event);
    }

    public object? Deserialize(string content, Type type)
    {
        return System.Text.Json.JsonSerializer.Deserialize(content, type, Options);
    }

    public object? Deserialize(ReadOnlyMemory<byte> data, Type type)
    {
        return System.Text.Json.JsonSerializer.Deserialize(data.Span, type, Options);
    }
}

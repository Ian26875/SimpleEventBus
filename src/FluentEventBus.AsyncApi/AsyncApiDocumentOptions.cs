namespace FluentEventBus.AsyncApi;

/// <summary>
/// Describes the AsyncAPI document to generate (the `info` section).
/// </summary>
public class AsyncApiDocumentOptions
{
    /// <summary>Application title shown in the document info.</summary>
    public string Title { get; set; } = "FluentEventBus Application";

    /// <summary>Application version shown in the document info.</summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>Optional application description.</summary>
    public string? Description { get; set; }

    /// <summary>Servers to declare in the document, keyed by server name.</summary>
    internal Dictionary<string, ServerInfo> Servers { get; } = new();

    /// <summary>Message examples registered per event type.</summary>
    internal Dictionary<Type, List<MessageExample>> Examples { get; } = new();

    /// <summary>
    /// Declares a server (broker) in the generated document, e.g.
    /// <c>WithServer("production", "rabbitmq.internal:5672", "amqp")</c>.
    /// </summary>
    /// <param name="name">Server key in the document (e.g. "production", "sit").</param>
    /// <param name="host">Host and optional port, without protocol prefix (e.g. "rabbitmq.internal:5672").</param>
    /// <param name="protocol">Protocol name (e.g. "amqp", "kafka", "inmemory").</param>
    /// <param name="description">Optional human-readable description.</param>
    /// <param name="protocolVersion">Optional protocol version (e.g. "0.9.1").</param>
    public AsyncApiDocumentOptions WithServer(string name,
                                              string host,
                                              string protocol,
                                              string? description = null,
                                              string? protocolVersion = null)
    {
        Servers[name] = new ServerInfo(host, protocol, description, protocolVersion);
        return this;
    }

    internal sealed record ServerInfo(string Host, string Protocol, string? Description, string? ProtocolVersion);

    /// <summary>
    /// Registers a sample payload shown as a message example in the generated document.
    /// Can be called multiple times per event type.
    /// </summary>
    /// <typeparam name="TEvent">The event type the example belongs to.</typeparam>
    /// <param name="payload">A sample event instance.</param>
    /// <param name="name">Optional machine-friendly example name.</param>
    /// <param name="summary">Optional short description of what the example shows.</param>
    public AsyncApiDocumentOptions WithExample<TEvent>(TEvent payload, string? name = null, string? summary = null)
        where TEvent : class
    {
        if (!Examples.TryGetValue(typeof(TEvent), out var list))
        {
            list = new List<MessageExample>();
            Examples[typeof(TEvent)] = list;
        }

        list.Add(new MessageExample(payload, name, summary));
        return this;
    }

    internal sealed record MessageExample(object Payload, string? Name, string? Summary);
}

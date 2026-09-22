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

    /// <summary>Message examples registered per event type.</summary>
    internal Dictionary<Type, List<MessageExample>> Examples { get; } = new();

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

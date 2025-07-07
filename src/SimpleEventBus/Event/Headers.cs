namespace SimpleEventBus.Event;

/// <summary>
/// Represents a collection of metadata headers associated with an event.
/// Inherits from <see cref="Dictionary{string, object}"/> to allow flexible key-value storage.
/// </summary>
/// <remarks>
/// Common usage includes passing correlation IDs, retry counters, trace IDs, and custom user-defined headers.
/// </remarks>
public class Headers : Dictionary<string, object>
{
    /// <summary>
    /// Gets or sets the Correlation ID associated with this event.
    /// Used for distributed tracing and end-to-end diagnostics.
    /// </summary>
    public string? CorrelationId
    {
        get => TryGetValue("CorrelationId", out var value) ? value?.ToString() : null;
        set => this["CorrelationId"] = value!;
    }
}

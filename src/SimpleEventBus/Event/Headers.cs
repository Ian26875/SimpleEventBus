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
    /// <summary>Well-known header key for <see cref="CorrelationId"/>.</summary>
    public const string CorrelationIdKey = "CorrelationId";

    /// <summary>Well-known header key for <see cref="MessageId"/>.</summary>
    public const string MessageIdKey = "MessageId";

    /// <summary>Well-known header key for <see cref="OccurredAt"/>.</summary>
    public const string OccurredAtKey = "OccurredAt";

    /// <summary>
    /// Gets or sets the Correlation ID associated with this event.
    /// Used for distributed tracing and end-to-end diagnostics.
    /// </summary>
    public string? CorrelationId
    {
        get => TryGetValue(CorrelationIdKey, out var value) ? value?.ToString() : null;
        set => this[CorrelationIdKey] = value!;
    }

    /// <summary>
    /// Gets or sets the unique id of this message. Assigned automatically at publish
    /// time when absent. Consumers use it for at-least-once deduplication.
    /// </summary>
    public string? MessageId
    {
        get => TryGetValue(MessageIdKey, out var value) ? value?.ToString() : null;
        set => this[MessageIdKey] = value!;
    }

    /// <summary>
    /// Gets or sets the UTC time the event was published, stored as an ISO-8601
    /// round-trip string so it survives transport header typing. Assigned
    /// automatically at publish time when absent.
    /// </summary>
    public DateTimeOffset? OccurredAt
    {
        get => TryGetValue(OccurredAtKey, out var value) &&
               DateTimeOffset.TryParse(value?.ToString(), null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsed)
            ? parsed
            : null;
        set => this[OccurredAtKey] = value?.ToString("O")!;
    }
}

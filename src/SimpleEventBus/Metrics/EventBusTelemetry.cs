namespace SimpleEventBus.Metrics;

/// <summary>
/// Contains shared constants for SimpleEventBus telemetry metrics.
/// Use these constants when registering meters or tagging metrics.
/// </summary>
public static class EventBusTelemetry
{
    /// <summary>
    /// The global meter name for all EventBus-related metrics.
    /// </summary>
    public const string MeterName = "SimpleEventBus";

    /// <summary>
    /// Counter name: total number of events published.
    /// </summary>
    public const string EventPublishedCounter = "eventbus_events_published_total";

    /// <summary>
    /// Counter name: total number of events received.
    /// </summary>
    public const string EventReceivedCounter = "eventbus_events_received_total";

    /// <summary>
    /// Counter name: total number of event publish failures.
    /// </summary>
    public const string EventPublishFailureCounter = "eventbus_publish_failures_total";

    /// <summary>
    /// Label name used to tag metrics with event name.
    /// </summary>
    public const string TagEventName = "event";
}
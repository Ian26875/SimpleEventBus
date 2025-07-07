using System.Diagnostics.Metrics;

namespace SimpleEventBus.Metrics;
/// <summary>
/// Provides metrics tracking for the event bus system using OpenTelemetry.
/// Includes counters for published events, received events, and publish failures.
/// </summary>
internal static class EventBusMetrics
{

    /// <summary>
    /// The meter instance used to register and emit metrics.
    /// </summary>
    private static readonly Meter Meter = new(EventBusTelemetry.MeterName);

    /// <summary>
    /// Counter for the total number of events successfully published.
    /// </summary>
    private static readonly Counter<long> PublishedEvents =
        Meter.CreateCounter<long>(
            name: EventBusTelemetry.EventPublishedCounter,
            unit: "events",
            description: "Total number of events published"
        );

    /// <summary>
    /// Counter for the total number of events received by the subscriber.
    /// </summary>
    private static readonly Counter<long> ReceivedEvents =
        Meter.CreateCounter<long>(
            name: EventBusTelemetry.EventReceivedCounter,
            unit: "events",
            description: "Total number of events received"
        );

    /// <summary>
    /// Counter for the total number of failed publish attempts.
    /// </summary>
    private static readonly Counter<long> PublishFailures =
        Meter.CreateCounter<long>(
            name: EventBusTelemetry.EventPublishFailureCounter,
            unit: "errors",
            description: "Total number of failed publish attempts"
        );

    /// <summary>
    /// Increments the published event counter by 1 for the given event name.
    /// </summary>
    /// <param name="eventName">The name of the published event.</param>
    public static void AddPublished(string eventName) =>
        PublishedEvents.Add(1, new KeyValuePair<string, object?>(EventBusTelemetry.TagEventName, eventName));

    /// <summary>
    /// Increments the received event counter by 1 for the given event name.
    /// </summary>
    /// <param name="eventName">The name of the received event.</param>
    public static void AddReceived(string eventName) =>
        ReceivedEvents.Add(1, new KeyValuePair<string, object?>(EventBusTelemetry.TagEventName, eventName));

    /// <summary>
    /// Increments the publish failure counter by 1 for the given event name.
    /// </summary>
    /// <param name="eventName">The name of the event that failed to publish.</param>
    public static void AddFailure(string eventName) =>
        PublishFailures.Add(1, new KeyValuePair<string, object?>(EventBusTelemetry.TagEventName, eventName));
}

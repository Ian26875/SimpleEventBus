using System.Diagnostics;
using FluentEventBus.Event;

namespace FluentEventBus.Metrics;

/// <summary>
/// Distributed tracing for the event bus. Publish creates a producer span and injects
/// the W3C traceparent into the message headers; consume creates a consumer span linked
/// to it, so publish→consume shows up as one trace across services.
/// Spans are only created when a listener (e.g. the OpenTelemetry SDK) is attached.
/// </summary>
internal static class EventBusActivitySource
{
    /// <summary>Well-known header carrying the W3C trace context.</summary>
    internal const string TraceParentHeader = "traceparent";

    /// <summary>Well-known header carrying the W3C trace state.</summary>
    internal const string TraceStateHeader = "tracestate";

    private static readonly ActivitySource Source = new(EventBusTelemetry.ActivitySourceName);

    /// <summary>
    /// Starts a producer span for a publish and injects the trace context into the headers.
    /// </summary>
    public static Activity? StartPublish(string eventName, Headers headers)
    {
        var activity = Source.StartActivity($"publish {eventName}", ActivityKind.Producer);
        if (activity is null)
        {
            return null;
        }

        activity.SetTag("messaging.operation.type", "send");
        activity.SetTag("messaging.destination.name", eventName);
        if (headers.MessageId is not null)
        {
            activity.SetTag("messaging.message.id", headers.MessageId);
        }

        headers[TraceParentHeader] = activity.Id!;
        if (!string.IsNullOrEmpty(activity.TraceStateString))
        {
            headers[TraceStateHeader] = activity.TraceStateString;
        }

        return activity;
    }

    /// <summary>
    /// Starts a consumer span for processing a received event, parented to the
    /// trace context found in the headers (when present).
    /// </summary>
    public static Activity? StartConsume(string eventName, Headers headers)
    {
        var parentContext = default(ActivityContext);
        if (headers.TryGetValue(TraceParentHeader, out var traceParent))
        {
            headers.TryGetValue(TraceStateHeader, out var traceState);
            ActivityContext.TryParse(traceParent?.ToString(), traceState?.ToString(), out parentContext);
        }

        var activity = Source.StartActivity($"process {eventName}", ActivityKind.Consumer, parentContext);
        if (activity is null)
        {
            return null;
        }

        activity.SetTag("messaging.operation.type", "process");
        activity.SetTag("messaging.destination.name", eventName);
        if (headers.MessageId is not null)
        {
            activity.SetTag("messaging.message.id", headers.MessageId);
        }

        return activity;
    }

    /// <summary>
    /// Marks the activity as failed with the exception message.
    /// </summary>
    public static void RecordFailure(Activity? activity, Exception exception)
    {
        activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
    }
}

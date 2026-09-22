using FluentEventBus.Metrics;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace FluentEventBus.OpenTelemetry;

/// <summary>
/// OpenTelemetry registration extensions for FluentEventBus.
/// </summary>
public static class OpenTelemetryBuilderExtensions
{
    /// <summary>
    /// Meter name of the in-memory transport. Kept as a literal so this package
    /// does not need a reference to FluentEventBus.InMemory.
    /// </summary>
    private const string InMemoryMeterName = "FluentEventBus.InMemory";

    /// <summary>
    /// Subscribes the tracer provider to FluentEventBus publish/consume spans.
    /// Trace context is propagated through message headers (W3C traceparent),
    /// so publisher and consumer services share one distributed trace.
    /// </summary>
    public static TracerProviderBuilder AddFluentEventBusInstrumentation(this TracerProviderBuilder builder)
    {
        return builder.AddSource(EventBusTelemetry.ActivitySourceName);
    }

    /// <summary>
    /// Subscribes the meter provider to FluentEventBus metrics:
    /// published/received/failure counters and the in-memory queue gauges.
    /// </summary>
    public static MeterProviderBuilder AddFluentEventBusInstrumentation(this MeterProviderBuilder builder)
    {
        return builder
            .AddMeter(EventBusTelemetry.MeterName)
            .AddMeter(InMemoryMeterName);
    }
}

using System.Diagnostics;
using FluentAssertions;
using FluentEventBus.Event;
using FluentEventBus.Metrics;

namespace FluentEventBus.Tests.Metrics;

public class EventBusActivitySourceTests : IDisposable
{
    private readonly ActivityListener _listener;

    public EventBusActivitySourceTests()
    {
        _listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == EventBusTelemetry.ActivitySourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded
        };
        ActivitySource.AddActivityListener(_listener);
    }

    [Fact(DisplayName = "StartPublish_ShouldInjectTraceParentHeader")]
    public void StartPublish_ShouldInjectTraceParentHeader()
    {
        var headers = new Headers();

        using var activity = EventBusActivitySource.StartPublish("order.placed.v1", headers);

        activity.Should().NotBeNull();
        headers.Should().ContainKey(EventBusActivitySource.TraceParentHeader);
        headers[EventBusActivitySource.TraceParentHeader].Should().Be(activity!.Id);
    }

    [Fact(DisplayName = "StartConsume_ShouldJoinTraceFromHeaders")]
    public void StartConsume_ShouldJoinTraceFromHeaders()
    {
        var headers = new Headers();
        string publishTraceId;
        using (var publish = EventBusActivitySource.StartPublish("order.placed.v1", headers))
        {
            publishTraceId = publish!.TraceId.ToString();
        }

        using var consume = EventBusActivitySource.StartConsume("order.placed.v1", headers);

        consume.Should().NotBeNull();
        consume!.TraceId.ToString().Should().Be(publishTraceId);
    }

    [Fact(DisplayName = "StartConsume_WithoutTraceParent_ShouldStartNewTrace")]
    public void StartConsume_WithoutTraceParent_ShouldStartNewTrace()
    {
        using var consume = EventBusActivitySource.StartConsume("order.placed.v1", new Headers());

        consume.Should().NotBeNull();
    }

    public void Dispose()
    {
        _listener.Dispose();
    }
}

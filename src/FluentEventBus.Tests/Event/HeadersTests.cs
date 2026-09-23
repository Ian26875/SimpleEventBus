using FluentAssertions;
using FluentEventBus.Event;
using FluentEventBus.Naming;
using FluentEventBus.Serialization;

namespace FluentEventBus.Tests.Event;

public class HeadersTests
{
    [Fact(DisplayName = "OccurredAt_SetAndGet_ShouldRoundTripThroughIsoString")]
    public void OccurredAt_SetAndGet_ShouldRoundTripThroughIsoString()
    {
        var headers = new Headers();
        var now = DateTimeOffset.UtcNow;

        headers.OccurredAt = now;

        headers[Headers.OccurredAtKey].Should().BeOfType<string>();
        headers.OccurredAt.Should().Be(now);
    }

    [Fact(DisplayName = "MessageId_Unset_ShouldBeNull")]
    public void MessageId_Unset_ShouldBeNull()
    {
        new Headers().MessageId.Should().BeNull();
    }

    [Fact(DisplayName = "PublishAsync_ShouldAutoFillMessageIdAndOccurredAt")]
    public async Task PublishAsync_ShouldAutoFillMessageIdAndOccurredAt()
    {
        var publisher = new CapturingPublisher();

        await publisher.PublishAsync(new TestEvent(Guid.NewGuid(), "test"));

        publisher.Captured.Should().NotBeNull();
        publisher.Captured!.Headers.MessageId.Should().NotBeNullOrWhiteSpace();
        publisher.Captured.Headers.OccurredAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "PublishAsync_WithExistingMessageId_ShouldNotOverwrite")]
    public async Task PublishAsync_WithExistingMessageId_ShouldNotOverwrite()
    {
        var publisher = new CapturingPublisher();
        var headers = new Headers { MessageId = "my-id" };

        await publisher.PublishAsync(new TestEvent(Guid.NewGuid(), "test"), headers);

        publisher.Captured!.Headers.MessageId.Should().Be("my-id");
    }

    private sealed class CapturingPublisher : AbstractEventPublisher
    {
        public EventContext? Captured { get; private set; }

        public CapturingPublisher() : base(new JsonSerializer(), new StubEventNameRegistry())
        {
        }

        protected override Task PublishEventAsync(EventContext eventContext, CancellationToken cancellationToken = default)
        {
            Captured = eventContext;
            return Task.CompletedTask;
        }
    }

    private sealed class StubEventNameRegistry : IEventNameRegistry
    {
        public void Register(Type eventType)
        {
        }

        public string GetEventName(Type type) => type.Name;

        public Type GetEventType(string type) => typeof(TestEvent);
    }
}

using FluentAssertions;
using SimpleEventBus;
using SimpleEventBus.Event;
using SimpleEventBus.Profile;
using SimpleEventBus.Schema;
using SimpleEventBus.Subscriber;

namespace SimpleEventBus.Tests;

public class SubscriptionProfileManagerTests
{
    [Fact(DisplayName = "Initialize_WithMultipleProfiles_ShouldRegisterEachEventOnce")]
    public void Initialize_WithMultipleProfiles_ShouldRegisterEachEventOnce()
    {
        var profiles = new SubscriptionProfile[]
        {
            new ProfileA(),
            new ProfileB()
        };

        var mapper = new TestEventMapper();
        var manager = new SubscriptionProfileManager(profiles, mapper);

        manager.Invoking(x => x.Initialize()).Should().NotThrow();
        mapper.RegisteredTypes.Should().BeEquivalentTo(new[] { typeof(EventA), typeof(EventB) });
    }

    private sealed class ProfileA : SubscriptionProfile
    {
        public ProfileA()
        {
            this.WhenOccurs<EventA>().ToDo<HandlerA>();
        }
    }

    private sealed class ProfileB : SubscriptionProfile
    {
        public ProfileB()
        {
            this.WhenOccurs<EventB>().ToDo<HandlerB>();
        }
    }

    private sealed class HandlerA : IEventHandler<EventA>
    {
        public Task HandleAsync(EventA @event, Headers headers, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class HandlerB : IEventHandler<EventB>
    {
        public Task HandleAsync(EventB @event, Headers headers, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed record EventA(Guid Id);
    private sealed record EventB(Guid Id);

    private sealed class TestEventMapper : IEventMapper
    {
        private readonly HashSet<Type> _types = new();

        public IReadOnlyCollection<Type> RegisteredTypes => _types;

        public void Register(Type eventType)
        {
            if (!_types.Add(eventType))
            {
                throw new ArgumentException($"Duplicate registration for {eventType.FullName}.");
            }
        }

        public string GetEventName(Type type) => type.Name;

        public Type GetEventType(string type)
        {
            return _types.First(t => t.Name == type);
        }
    }
}

using Moq;
using SimpleEventBus.Profile;
using SimpleEventBus.Subscriber.Executors;
using FluentAssertions;

namespace SimpleEventBus.Tests.Profile;

public class SubscriptionProfileTests
{
    private class TestSubscriptionProfile : SubscriptionProfile { }

    [Fact]
    public void AddSubscription_Should_Add_EventHandlerExecutor_When_Not_Already_Registered()
    {
        // Arrange
        var profile = new TestSubscriptionProfile();
        var eventType = typeof(string);
        var executor = new Mock<IEventHandlerExecutor>().Object;

        // Act
        profile.AddSubscription(eventType, executor);

        // Assert
        profile.EventHandlerExecutors[eventType].Should().Contain(executor);
    }

    [Fact]
    public void AddSubscription_Should_ThrowException_When_EventHandlerExecutor_Already_Registered()
    {
        // Arrange
        var profile = new TestSubscriptionProfile();
        var eventType = typeof(string);
        var executor = new Mock<IEventHandlerExecutor>().Object;

        // First subscription
        profile.AddSubscription(eventType, executor);

        // Act
        Action action = () => profile.AddSubscription(eventType, executor);

        // Assert
        action.Should().Throw<ArgumentException>()
              .WithMessage($"Handler type '{executor}' is already registered for event type '{eventType.FullName}'.");
    }

    [Fact]
    public void AddSubscription_Should_Add_EventHandlerType_When_Not_Already_Registered()
    {
        // Arrange
        var profile = new TestSubscriptionProfile();
        var eventType = typeof(string);
        var handlerType = typeof(int);

        // Act
        profile.AddSubscription(eventType, handlerType);

        // Assert
        profile.EventHandlers[eventType].Should().Contain(handlerType);
    }

    [Fact]
    public void AddSubscription_Should_ThrowException_When_EventHandlerType_Already_Registered()
    {
        // Arrange
        var profile = new TestSubscriptionProfile();
        var eventType = typeof(string);
        var handlerType = typeof(int);

        // First subscription
        profile.AddSubscription(eventType, handlerType);

        // Act
        Action act = () => profile.AddSubscription(eventType, handlerType);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Handler type '{handlerType.FullName}' is already registered for event type '{eventType.FullName}'.");
    }

    [Fact]
    public void AddErrorFilter_Should_Add_ErrorHandlerType_When_Not_Already_Registered()
    {
        // Arrange
        var profile = new TestSubscriptionProfile();
        var eventType = typeof(string);
        var handlerType = typeof(int);

        // Act
        profile.AddErrorFilter(eventType, handlerType);

        // Assert
        profile.ErrorHandlers[eventType].Should().Contain(handlerType);
    }

    [Fact]
    public void AddErrorFilter_Should_ThrowException_When_ErrorHandlerType_Already_Registered()
    {
        // Arrange
        var profile = new TestSubscriptionProfile();
        var eventType = typeof(string);
        var handlerType = typeof(int);

        // First subscription
        profile.AddErrorFilter(eventType, handlerType);

        // Act
        Action act = () => profile.AddErrorFilter(eventType, handlerType);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage($"Handler type '{handlerType.FullName}' is already registered for event type '{eventType.FullName}'.");
    }
}

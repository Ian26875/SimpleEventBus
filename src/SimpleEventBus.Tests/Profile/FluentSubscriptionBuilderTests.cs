using System.Linq.Expressions;
using FluentAssertions;
using SimpleEventBus.Event;
using SimpleEventBus.Profile;
using SimpleEventBus.Subscriber.Executors;

namespace SimpleEventBus.Tests.Profile;

public class FluentSubscriptionBuilderTests
{
     [Fact]
    public void Constructor_ShouldInitializeProfileCorrectly()
    {
        // Arrange
        var subscriptionProfile = new TestSubscriptionProfile();

        // Act
        var builder = new FluentSubscriptionBuilder<TestEvent>(subscriptionProfile);

        // Assert
        builder.Profile.Should().Be(subscriptionProfile);
    }

    [Fact]
    public void ToDo_Should_AddSubscription_With_InterfaceEventHandlerExecutor()
    {
        // Arrange
        var subscriptionProfile = new TestSubscriptionProfile();
        var builder = new FluentSubscriptionBuilder<TestEvent>(subscriptionProfile);

        // Act
        builder.ToDo<TestHandler>();

        // Assert
        subscriptionProfile.EventHandlerExecutors.Should().ContainKey(typeof(TestEvent));
        var executor = subscriptionProfile.EventHandlerExecutors[typeof(TestEvent)];
        executor.Should().ContainSingle()
            .Which.Should().BeOfType<InterfaceEventHandlerExecutor<TestEvent, TestHandler>>();
    }

    [Fact]
    public void ToDo_Should_AddSubscription_With_ExpressionEventHandlerExecutor()
    {
        // Arrange
        var subscriptionProfile = new TestSubscriptionProfile();
        var builder = new FluentSubscriptionBuilder<TestEvent>(subscriptionProfile);
        Expression<Func<TestHandler, Func<TestEvent, Headers, CancellationToken, Task>>> expression = h => h.HandleAsync;

        // Act
        builder.ToDo(expression);

        // Assert
        subscriptionProfile.EventHandlerExecutors.Should().ContainKey(typeof(TestEvent));
        var executor = subscriptionProfile.EventHandlerExecutors[typeof(TestEvent)];
        executor.Should().ContainSingle()
            .Which.Should().BeOfType<ExpressionEventHandlerExecutor<TestEvent, TestHandler>>();
    }

    [Fact]
    public void CatchExceptionToDo_Should_AddErrorFilter_With_SpecifiedErrorHandler()
    {
        // Arrange
        var subscriptionProfile = new TestSubscriptionProfile();
        var builder = new FluentSubscriptionBuilder<TestEvent>(subscriptionProfile);

        // Act
        builder.CatchExceptionToDo<TestErrorHandler>();

        // Assert
        subscriptionProfile.ErrorHandlers.Should().ContainKey(typeof(TestEvent));
        var errorHandlers = subscriptionProfile.ErrorHandlers[typeof(TestEvent)];
        errorHandlers.Should().ContainSingle()
            .Which.Should().Be(typeof(TestErrorHandler));
    }
}
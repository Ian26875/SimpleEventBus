using System.Linq.Expressions;
using FluentAssertions;
using SimpleEventBus.Event;
using SimpleEventBus.Subscriber.Executors;

namespace SimpleEventBus.Tests.Subscriber.Executors;

public class ExpressionEventHandlerExecutorTests
{
    [Fact(DisplayName = "Constructor_ShouldInitializePropertiesCorrectly")]
    public void Constructor_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        Expression<Func<TestHandler, Func<TestEvent, Headers, CancellationToken, Task>>> expression = h => h.Handle;

        // Act
        var executor = new ExpressionEventHandlerExecutor<TestEvent, TestHandler>(expression);

        // Assert
        executor.HandlerType.Should().Be(typeof(TestHandler));
        executor.EventType.Should().Be(typeof(TestEvent));
        executor.MethodInfo.Name.Should().Be("Handle");
    }

    [Fact(DisplayName = "CreateHandlerDelegate_ShouldReturnValidHandlerDelegate")]
    public async Task CreateHandlerDelegate_ShouldReturnValidHandlerDelegate()
    {
        // Arrange
        Expression<Func<TestHandler, Func<TestEvent, Headers, CancellationToken, Task>>> expression = h => h.Handle;
        var executor = new ExpressionEventHandlerExecutor<TestEvent, TestHandler>(expression);

        var testHandler = new TestHandler();
        var testEvent = new TestEvent(Guid.NewGuid(),"Test");
        var headers = new Headers();
        var cancellationToken = CancellationToken.None;

        var handlerDelegate = executor.CreateHandlerDelegate();

        // Act
        await handlerDelegate(testHandler, testEvent, headers, cancellationToken);

        // Assert
        testHandler.HandledEvent.Should().Be(testEvent);
        testHandler.HandledHeaders.Should().BeEmpty();
    }

    [Fact(DisplayName = "CreateHandlerDelegate_WhenHandlerTypeMismatch_ShouldThrowArgumentException")]
    public void CreateHandlerDelegate_WhenHandlerTypeMismatch_ShouldThrowArgumentException()
    {
        // Arrange
        Expression<Func<TestHandler, Func<TestEvent, Headers, CancellationToken, Task>>> expression = h => h.Handle;
        var executor = new ExpressionEventHandlerExecutor<TestEvent, TestHandler>(expression);

        var invalidHandler = new object();
        var testEvent = new TestEvent(Guid.NewGuid(),"Test");
        var headers = new Headers();
        var cancellationToken = CancellationToken.None;

        var handlerDelegate = executor.CreateHandlerDelegate();

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => handlerDelegate(invalidHandler, testEvent, headers, cancellationToken));
    }

    [Fact(DisplayName = "CreateHandlerDelegate_WhenEventTypeMismatch_ShouldThrowArgumentException")]
    public void CreateHandlerDelegate_WhenEventTypeMismatch_ShouldThrowArgumentException()
    {
        // Arrange
        Expression<Func<TestHandler, Func<TestEvent, Headers, CancellationToken, Task>>> expression = h => h.Handle;
        var executor = new ExpressionEventHandlerExecutor<TestEvent, TestHandler>(expression);

        var testHandler = new TestHandler();
        var invalidEvent = new object();
        var headers = new Headers();
        var cancellationToken = CancellationToken.None;

        var handlerDelegate = executor.CreateHandlerDelegate();

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => handlerDelegate(testHandler, invalidEvent, headers, cancellationToken));
    }
}
using System.Linq.Expressions;
using FluentAssertions;
using SimpleEventBus.Event;
using SimpleEventBus.Subscriber.Executors;

namespace SimpleEventBus.Tests.Subscriber.Executors;

public class ExpressionEventHandlerExecutorTests
{
    [Fact]
    public void Constructor_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        Expression<Func<ExpressionTestEventHandler, Func<TestEvent, Headers, CancellationToken, Task>>> expression = h => h.HandleAsync;

        // Act
        var executor = new ExpressionEventHandlerExecutor<TestEvent, ExpressionTestEventHandler>(expression);

        // Assert
        executor.HandlerType.Should().Be(typeof(ExpressionTestEventHandler));
        executor.EventType.Should().Be(typeof(TestEvent));
        executor.MethodInfo.Name.Should().Be(nameof(ExpressionTestEventHandler.HandleAsync));
    }

    [Fact]
    public async Task CreateHandlerDelegate_ShouldReturnValidHandlerDelegate()
    {
        // Arrange
        Expression<Func<ExpressionTestEventHandler, Func<TestEvent, Headers, CancellationToken, Task>>> expression = h => h.HandleAsync;
        var executor = new ExpressionEventHandlerExecutor<TestEvent, ExpressionTestEventHandler>(expression);

        var testHandler = new ExpressionTestEventHandler();
        var testEvent = new TestEvent(Guid.NewGuid(), "Test");
        var headers = new Headers();
        var cancellationToken = CancellationToken.None;

        var handlerDelegate = executor.CreateHandlerDelegate();

        // Act
        await handlerDelegate(testHandler, testEvent, headers, cancellationToken);

        // Assert
        testHandler.TestEvent.Should().Be(testEvent);
        testHandler.Headers.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateHandlerDelegate_WhenHandlerTypeMismatch_ShouldThrowArgumentException()
    {
        // Arrange
        Expression<Func<TestHandler, Func<TestEvent, Headers, CancellationToken, Task>>> expression = h => h.HandleAsync;
        var executor = new ExpressionEventHandlerExecutor<TestEvent, TestHandler>(expression);

        var invalidHandler = new object();
        var testEvent = new TestEvent(Guid.NewGuid(), "Test");
        var headers = new Headers();
        var cancellationToken = CancellationToken.None;

        var handlerDelegate = executor.CreateHandlerDelegate();

        // Act
        Func<Task> act = () => handlerDelegate(invalidHandler, testEvent, headers, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("The handler or event type does not match.");
    }

    [Fact]
    public async Task CreateHandlerDelegate_WhenEventTypeMismatch_ShouldThrowArgumentException()
    {
        // Arrange
        Expression<Func<ExpressionTestEventHandler, Func<TestEvent, Headers, CancellationToken, Task>>> expression = h => h.HandleAsync;
        var executor = new ExpressionEventHandlerExecutor<TestEvent, ExpressionTestEventHandler>(expression);

        var testHandler = new ExpressionTestEventHandler();
        var invalidEvent = new object();
        var headers = new Headers();
        var cancellationToken = CancellationToken.None;

        var handlerDelegate = executor.CreateHandlerDelegate();

        // Act
        Func<Task> act = () => handlerDelegate(testHandler, invalidEvent, headers, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
                          .WithMessage("The handler or event type does not match.");
    }
}

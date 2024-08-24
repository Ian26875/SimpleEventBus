using FluentAssertions;
using SimpleEventBus.RabbitMq;

namespace SimpleEventBus.RabbitMqTests;

public class EventBinderTests
{
    [Fact(DisplayName = "EventBinder_聲明交換器_應將交換器名稱綁定到事件類型")]
    public void DeclareExchange_ShouldBindExchangeNameToEventType()
    {
        // Arrange
        var options = new RabbitMqBindingOption();
        var binder = new EventBinder<TestEvent>(options);
        var exchangeName = "TestExchange";

        // Act
        binder.DeclareExchange(exchangeName);

        // Assert
        options.ExchangeBindings.Should().ContainKey(typeof(TestEvent));
        options.ExchangeBindings[typeof(TestEvent)].Should().Be(exchangeName);
    }

    [Fact(DisplayName = "EventBinder_聲明佇列_應將佇列名稱綁定到事件類型")]
    public void DeclareQueue_ShouldBindQueueNameToEventType()
    {
        // Arrange
        var options = new RabbitMqBindingOption();
        var binder = new EventBinder<TestEvent>(options);
        var queueName = "TestQueue";

        // Act
        binder.DeclareQueue(queueName);

        // Assert
        options.QueueBindings.Should().ContainKey(typeof(TestEvent));
        options.QueueBindings[typeof(TestEvent)].Should().Be(queueName);
    }

    [Fact(DisplayName = "EventBinder_聲明交換器和佇列_應能多次聲明並覆蓋舊值")]
    public void DeclareExchangeAndQueue_ShouldAllowMultipleDeclarationsAndOverrideOldValue()
    {
        // Arrange
        var options = new RabbitMqBindingOption();
        var binder = new EventBinder<TestEvent>(options);
        var initialExchangeName = "InitialExchange";
        var updatedExchangeName = "UpdatedExchange";
        var initialQueueName = "InitialQueue";
        var updatedQueueName = "UpdatedQueue";

        // Act
        binder.DeclareExchange(initialExchangeName).DeclareExchange(updatedExchangeName);
        binder.DeclareQueue(initialQueueName).DeclareQueue(updatedQueueName);

        // Assert
        options.ExchangeBindings[typeof(TestEvent)].Should().Be(updatedExchangeName);
        options.QueueBindings[typeof(TestEvent)].Should().Be(updatedQueueName);
    }
}

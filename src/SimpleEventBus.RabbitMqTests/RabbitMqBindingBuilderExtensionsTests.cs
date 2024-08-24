using FluentAssertions;
using SimpleEventBus.RabbitMq;

namespace SimpleEventBus.RabbitMqTests;

public class RabbitMqBindingBuilderExtensionsTests
{
    [Fact(DisplayName = "ForEvent_為指定事件創建EventBinder_應返回EventBinder實例")]
    public void ForEvent_ShouldReturnEventBinderInstanceForGivenEvent()
    {
        // Arrange
        var options = new RabbitMqBindingOption();

        // Act
        var binder = options.ForEvent<TestEvent>();

        // Assert
        binder.Should().NotBeNull();
        binder.Should().BeOfType<EventBinder<TestEvent>>();
    }
}
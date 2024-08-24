using FluentAssertions;
using SimpleEventBus.RabbitMq;

namespace SimpleEventBus.RabbitMqTests;

public class RabbitMqBindingOptionTests
{
    [Fact(DisplayName = "RabbitMqBindingOption_初始化_應創建空字典")]
    public void RabbitMqBindingOption_Initialized_ShouldCreateEmptyDictionaries()
    {
        // Arrange & Act
        var options = new RabbitMqBindingOption();

        // Assert
        options.ExchangeBindings.Should().NotBeNull().And.BeEmpty();
        options.QueueBindings.Should().NotBeNull().And.BeEmpty();
    }
}
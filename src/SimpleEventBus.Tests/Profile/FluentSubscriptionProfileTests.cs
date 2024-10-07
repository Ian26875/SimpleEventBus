using FluentAssertions;
using SimpleEventBus.Profile;

namespace SimpleEventBus.Tests.Profile;

public class FluentSubscriptionProfileTests
{
    [Fact]
    public void WhenOccurs_Should_Return_FluentSubscriptionBuilder_For_EventType()
    {
        // Arrange
        var subscriptionProfile = new TestSubscriptionProfile();

        // Act
        var actual = subscriptionProfile.WhenOccurs<TestEvent>();

        // Assert
        actual.Should().NotBeNull();
        actual.Should().BeOfType<FluentSubscriptionBuilder<TestEvent>>();
    }
}
using FluentAssertions;
using Moq;
using SimpleEventBus.Profile;
using SimpleEventBus.Schema;
using SimpleEventBus.Subscriber.Executors;

namespace SimpleEventBus.Tests.Profile;

public class SubscriptionProfileManagerTests
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenProfilesAreNull()
    {
        // Act
        Action act = () => new SubscriptionProfileManager(null, Mock.Of<ISchemaRegistry>());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*profiles*");
    }

    [Fact]
    public void Constructor_ShouldAggregateProfilesCorrectly()
    {
        // Arrange
        var schemaRegistryMock = new Mock<ISchemaRegistry>();
        var profile1 = new TestSubscriptionProfile();
        var profile2 = new TestSubscriptionProfile();

        var eventHandlerExecutorMock = new Mock<IEventHandlerExecutor>();

        profile1.AddSubscription(typeof(TestEvent),eventHandlerExecutorMock.Object);
        profile1.AddSubscription(typeof(Test2Event),eventHandlerExecutorMock.Object);

        var profiles = new List<SubscriptionProfile> { profile1, profile2 };

        // Act
        var manager = new SubscriptionProfileManager(profiles, schemaRegistryMock.Object);

        // Assert
        manager.HasSubscriptionsForEvent(typeof(TestEvent)).Should().BeTrue();
        manager.HasSubscriptionsForEvent(typeof(Test2Event)).Should().BeTrue();
        manager.GetEventHandlerExecutorForEvent(typeof(TestEvent)).Should().HaveCount(1);
        manager.GetEventHandlerExecutorForEvent(typeof(Test2Event)).Should().HaveCount(1);
    }

    [Fact]
    public void HasSubscriptionsForEvent_ShouldReturnTrue_WhenEventTypeExists()
    {
        // Arrange
        var schemaRegistryMock = new Mock<ISchemaRegistry>();
        
        var profile = new TestSubscriptionProfile();
        
        var eventHandlerExecutorMock = new Mock<IEventHandlerExecutor>();

        profile.AddSubscription(typeof(TestEvent),eventHandlerExecutorMock.Object);
        
        var profiles = new List<SubscriptionProfile> { profile };

        var manager = new SubscriptionProfileManager(profiles, schemaRegistryMock.Object);

        // Act
        var actual = manager.HasSubscriptionsForEvent(typeof(TestEvent));

        // Assert
        actual.Should().BeTrue();
    }

    [Fact]
    public void HasSubscriptionsForEvent_ShouldReturnFalse_WhenEventTypeDoesNotExist()
    {
        // Arrange
        var schemaRegistryMock = new Mock<ISchemaRegistry>();
        var profile = new Mock<SubscriptionProfile>();
        var profiles = new List<SubscriptionProfile> { profile.Object };

        var manager = new SubscriptionProfileManager(profiles, schemaRegistryMock.Object);

        // Act
        var result = manager.HasSubscriptionsForEvent(typeof(int));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetEventHandlerExecutorForEvent_ShouldReturnEventHandlerExecutors_WhenEventTypeExists()
    {
        // Arrange
        var schemaRegistryMock = new Mock<ISchemaRegistry>();
        var profile = new TestSubscriptionProfile();
        var eventHandlerExecutorMock = new Mock<IEventHandlerExecutor>();

        profile.AddSubscription(typeof(TestEvent), eventHandlerExecutorMock.Object );

        var profiles = new List<SubscriptionProfile> { profile };
        var manager = new SubscriptionProfileManager(profiles, schemaRegistryMock.Object);

        // Act
        var actual = manager.GetEventHandlerExecutorForEvent(typeof(TestEvent));

        // Assert
        actual.Should().Contain(eventHandlerExecutorMock.Object);
    }

    [Fact]
    public void GetEventHandlerExecutorForEvent_ShouldThrowArgumentNullException_WhenEventTypeIsNull()
    {
        // Arrange
        var manager = new SubscriptionProfileManager(new List<SubscriptionProfile>(), Mock.Of<ISchemaRegistry>());

        // Act
        Action act = () => manager.GetEventHandlerExecutorForEvent(null);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*eventType*");
    }

    [Fact]
    public void GetErrorHandlersForEvent_ShouldReturnErrorHandlers_WhenEventTypeExists()
    {
        // Arrange
        var schemaRegistryMock = new Mock<ISchemaRegistry>();
        var profile = new TestSubscriptionProfile();
        profile.AddErrorFilter(typeof(TestEvent),typeof(TestErrorHandler));
       
        
        var profiles = new List<SubscriptionProfile> { profile };
        var manager = new SubscriptionProfileManager(profiles, schemaRegistryMock.Object);

        // Act
        var actual = manager.GetErrorHandlersForEvent(typeof(TestEvent));

        // Assert
        actual.Should().Contain(typeof(TestErrorHandler));
    }

    [Fact]
    public void GetErrorHandlersForEvent_ShouldThrowArgumentNullException_WhenEventTypeIsNull()
    {
        // Arrange
        var manager = new SubscriptionProfileManager(new List<SubscriptionProfile>(), Mock.Of<ISchemaRegistry>());

        // Act
        Action act = () => manager.GetErrorHandlersForEvent(null);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithMessage("*eventType*");
    }
}
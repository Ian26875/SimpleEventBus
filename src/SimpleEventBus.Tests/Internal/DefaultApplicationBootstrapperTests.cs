using SimpleEventBus.Internal;
using Moq;

namespace SimpleEventBus.Tests.Internal;


public class DefaultApplicationBootstrapperTests
{
    [Fact]
    public async Task StartAsync_ShouldCallAllInitializers()
    {
        // Arrange
        var initializer1 = new Mock<IInitializer>();
        var initializer2 = new Mock<IInitializer>();
        var initializers = new List<IInitializer> { initializer1.Object, initializer2.Object };

        var terminators = new List<ITerminator>();
        var bootstrapper = new DefaultApplicationBootstrapper(initializers, terminators);

        var cancellationToken = CancellationToken.None;

        // Act
        await bootstrapper.StartAsync(cancellationToken);

        // Assert
        initializer1.Verify(i => i.InitializeAsync(cancellationToken), Times.Once);
        initializer2.Verify(i => i.InitializeAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task StopAsync_ShouldCallAllTerminators()
    {
        // Arrange
        var terminator1 = new Mock<ITerminator>();
        var terminator2 = new Mock<ITerminator>();
        var terminators = new List<ITerminator> { terminator1.Object, terminator2.Object };

        var initializers = new List<IInitializer>();
        var bootstrapper = new DefaultApplicationBootstrapper(initializers, terminators);

        var cancellationToken = CancellationToken.None;

        // Act
        await bootstrapper.StopAsync(cancellationToken);

        // Assert
        terminator1.Verify(t => t.TerminateAsync(cancellationToken), Times.Once);
        terminator2.Verify(t => t.TerminateAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_ShouldCallAllInitializers()
    {
        // Arrange
        var initializer1 = new Mock<IInitializer>();
        var initializer2 = new Mock<IInitializer>();
        var initializers = new List<IInitializer> { initializer1.Object, initializer2.Object };

        var terminators = new List<ITerminator>();
        var bootstrapper = new DefaultApplicationBootstrapper(initializers, terminators);

        var cancellationToken = CancellationToken.None;

        // Act
        await bootstrapper.InitializeAsync(cancellationToken);

        // Assert
        initializer1.Verify(i => i.InitializeAsync(cancellationToken), Times.Once);
        initializer2.Verify(i => i.InitializeAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task TerminateAsync_ShouldCallAllTerminators()
    {
        // Arrange
        var terminator1 = new Mock<ITerminator>();
        var terminator2 = new Mock<ITerminator>();
        var terminators = new List<ITerminator> { terminator1.Object, terminator2.Object };

        var initializers = new List<IInitializer>();
        var bootstrapper = new DefaultApplicationBootstrapper(initializers, terminators);

        var cancellationToken = CancellationToken.None;

        // Act
        await bootstrapper.TerminateAsync(cancellationToken);

        // Assert
        terminator1.Verify(t => t.TerminateAsync(cancellationToken), Times.Once);
        terminator2.Verify(t => t.TerminateAsync(cancellationToken), Times.Once);
    }
}

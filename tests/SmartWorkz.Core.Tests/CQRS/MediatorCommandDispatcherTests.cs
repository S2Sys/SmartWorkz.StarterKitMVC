namespace SmartWorkz.Core.Tests.CQRS;

using Microsoft.Extensions.DependencyInjection;
using Moq;
using SmartWorkz.Shared.CQRS;
using Xunit;

/// <summary>
/// Tests for the MediatorCommandDispatcher.
/// </summary>
public class MediatorCommandDispatcherTests
{
    [Fact]
    public async Task DispatchAsync_WithValidCommand_CallsCorrectHandler()
    {
        // Arrange
        var command = new TestCommand { Value = "test" };
        var mockHandler = new Mock<ICommandHandler<TestCommand>>();
        mockHandler.Setup(h => h.HandleAsync(command, CancellationToken.None))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var services = new ServiceCollection();
        services.AddScoped(_ => mockHandler.Object);
        var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new MediatorCommandDispatcher(serviceProvider);

        // Act
        await dispatcher.DispatchAsync(command);

        // Assert
        mockHandler.Verify(h => h.HandleAsync(command, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_WithNullCommand_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new MediatorCommandDispatcher(serviceProvider);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            dispatcher.DispatchAsync<TestCommand>(null!));
    }

    [Fact]
    public async Task DispatchAsync_WithNoCancellationToken_UseDefault()
    {
        // Arrange
        var command = new TestCommand { Value = "test" };
        var mockHandler = new Mock<ICommandHandler<TestCommand>>();
        mockHandler.Setup(h => h.HandleAsync(command, default))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var services = new ServiceCollection();
        services.AddScoped(_ => mockHandler.Object);
        var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new MediatorCommandDispatcher(serviceProvider);

        // Act
        await dispatcher.DispatchAsync(command);

        // Assert
        mockHandler.Verify(h => h.HandleAsync(command, default), Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_WithCancellationToken_PassesToHandler()
    {
        // Arrange
        var command = new TestCommand { Value = "test" };
        var cancellationToken = new CancellationToken(false);
        var mockHandler = new Mock<ICommandHandler<TestCommand>>();
        mockHandler.Setup(h => h.HandleAsync(command, cancellationToken))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var services = new ServiceCollection();
        services.AddScoped(_ => mockHandler.Object);
        var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new MediatorCommandDispatcher(serviceProvider);

        // Act
        await dispatcher.DispatchAsync(command, cancellationToken);

        // Assert
        mockHandler.Verify(h => h.HandleAsync(command, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_WhenHandlerNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new TestCommand { Value = "test" };
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new MediatorCommandDispatcher(serviceProvider);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            dispatcher.DispatchAsync(command));
    }

    [Fact]
    public async Task DispatchAsync_WhenHandlerThrowsException_PropagatesToCaller()
    {
        // Arrange
        var command = new TestCommand { Value = "test" };
        var expectedException = new InvalidOperationException("Handler error");
        var mockHandler = new Mock<ICommandHandler<TestCommand>>();
        mockHandler.Setup(h => h.HandleAsync(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var services = new ServiceCollection();
        services.AddScoped(_ => mockHandler.Object);
        var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new MediatorCommandDispatcher(serviceProvider);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            dispatcher.DispatchAsync(command));
        Assert.Equal("Handler error", exception.Message);
    }

    [Fact]
    public async Task DispatchAsync_WithMultipleCommands_DispatchesEachCorrectly()
    {
        // Arrange
        var command1 = new TestCommand { Value = "test1" };
        var command2 = new TestCommand { Value = "test2" };

        var mockHandler = new Mock<ICommandHandler<TestCommand>>();
        mockHandler.Setup(h => h.HandleAsync(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var services = new ServiceCollection();
        services.AddScoped(_ => mockHandler.Object);
        var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new MediatorCommandDispatcher(serviceProvider);

        // Act
        await dispatcher.DispatchAsync(command1);
        await dispatcher.DispatchAsync(command2);

        // Assert
        mockHandler.Verify(h => h.HandleAsync(command1, default), Times.Once);
        mockHandler.Verify(h => h.HandleAsync(command2, default), Times.Once);
    }

    [Fact]
    public async Task DispatchAsync_WithDifferentCommandTypes_UsesCorrectHandlers()
    {
        // Arrange
        var command1 = new TestCommand { Value = "test1" };
        var command2 = new AnotherTestCommand { Name = "test2" };

        var mockHandler1 = new Mock<ICommandHandler<TestCommand>>();
        mockHandler1.Setup(h => h.HandleAsync(command1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var mockHandler2 = new Mock<ICommandHandler<AnotherTestCommand>>();
        mockHandler2.Setup(h => h.HandleAsync(command2, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var services = new ServiceCollection();
        services.AddScoped(_ => mockHandler1.Object);
        services.AddScoped(_ => mockHandler2.Object);
        var serviceProvider = services.BuildServiceProvider();

        var dispatcher = new MediatorCommandDispatcher(serviceProvider);

        // Act
        await dispatcher.DispatchAsync(command1);
        await dispatcher.DispatchAsync(command2);

        // Assert
        mockHandler1.Verify(h => h.HandleAsync(command1, It.IsAny<CancellationToken>()), Times.Once);
        mockHandler2.Verify(h => h.HandleAsync(command2, It.IsAny<CancellationToken>()), Times.Once);
    }

    // Test command classes
    public class TestCommand : ICommand
    {
        public string Value { get; set; } = string.Empty;
    }

    public class AnotherTestCommand : ICommand
    {
        public string Name { get; set; } = string.Empty;
    }
}

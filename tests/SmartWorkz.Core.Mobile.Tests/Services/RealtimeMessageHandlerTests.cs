namespace SmartWorkz.Mobile.Tests.Services;

using System;
using System.Threading.Tasks;
using Xunit;
using SmartWorkz.Mobile.Services.Implementations;
using SmartWorkz.Mobile.Models;
using Microsoft.Extensions.Logging;
using Moq;

public class RealtimeMessageHandlerTests
{
    [Fact]
    public async Task RegisterHandler_ValidMethod_RoutsMessageCorrectly()
    {
        // Arrange
        var handler = new RealtimeMessageHandler();
        var receivedMessage = false;

        handler.RegisterHandler("OrderUpdated", (msg) =>
        {
            receivedMessage = true;
            return Task.CompletedTask;
        });

        var message = new RealtimeMessage("Orders", "OrderUpdated", null, DateTime.UtcNow, "123");

        // Act
        var result = await handler.HandleAsync(message);

        // Assert
        Assert.True(result);
        Assert.True(receivedMessage);
    }

    [Fact]
    public async Task HandleAsync_UnknownMethod_ReturnsFalse()
    {
        // Arrange
        var handler = new RealtimeMessageHandler();
        var message = new RealtimeMessage("Orders", "UnknownMethod", null, DateTime.UtcNow, "123");

        // Act
        var result = await handler.HandleAsync(message);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RegisterHandler_CaseSensitive_RoutesCorrectly()
    {
        // Arrange
        var handler = new RealtimeMessageHandler();
        var receivedMessage = false;

        handler.RegisterHandler("OrderUpdated", (msg) =>
        {
            receivedMessage = true;
            return Task.CompletedTask;
        });

        var message = new RealtimeMessage("Orders", "orderupdated", null, DateTime.UtcNow, "123");

        // Act
        var result = await handler.HandleAsync(message);

        // Assert - Should match due to OrdinalIgnoreCase
        Assert.True(result);
        Assert.True(receivedMessage);
    }

    [Fact]
    public async Task RegisterHandler_MultipleHandlers_RoutesEach()
    {
        // Arrange
        var handler = new RealtimeMessageHandler();
        var orderUpdated = false;
        var userLoggedIn = false;

        handler.RegisterHandler("OrderUpdated", (msg) =>
        {
            orderUpdated = true;
            return Task.CompletedTask;
        });

        handler.RegisterHandler("UserLoggedIn", (msg) =>
        {
            userLoggedIn = true;
            return Task.CompletedTask;
        });

        // Act
        await handler.HandleAsync(new RealtimeMessage("Orders", "OrderUpdated", null, DateTime.UtcNow, "1"));
        await handler.HandleAsync(new RealtimeMessage("User", "UserLoggedIn", null, DateTime.UtcNow, "2"));

        // Assert
        Assert.True(orderUpdated);
        Assert.True(userLoggedIn);
    }

    [Fact]
    public async Task HandleAsync_HandlerThrowsException_CatchesAndLogsError()
    {
        // Arrange
        var handler = new RealtimeMessageHandler();
        handler.RegisterHandler("FailingMethod", (msg) =>
        {
            throw new InvalidOperationException("Test error");
        });

        var message = new RealtimeMessage("Orders", "FailingMethod", null, DateTime.UtcNow, "123");

        // Act
        var result = await handler.HandleAsync(message);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ClearHandlers_RemovesAllHandlers()
    {
        // Arrange
        var handler = new RealtimeMessageHandler();
        handler.RegisterHandler("Method1", (msg) => Task.CompletedTask);
        handler.RegisterHandler("Method2", (msg) => Task.CompletedTask);

        // Act
        handler.ClearHandlers();

        // Assert
        Assert.Equal(0, handler.HandlerCount);
    }

    [Fact]
    public void HandlerCount_ReturnsCorrectCount()
    {
        // Arrange
        var handler = new RealtimeMessageHandler();

        // Act & Assert
        Assert.Equal(0, handler.HandlerCount);

        handler.RegisterHandler("Method1", (msg) => Task.CompletedTask);
        Assert.Equal(1, handler.HandlerCount);

        handler.RegisterHandler("Method2", (msg) => Task.CompletedTask);
        Assert.Equal(2, handler.HandlerCount);
    }

    [Fact]
    public async Task RegisterHandler_WithPayload_PassesPayloadToHandler()
    {
        // Arrange
        var handler = new RealtimeMessageHandler();
        var capturedPayload = default(object);

        handler.RegisterHandler("OrderUpdated", (msg) =>
        {
            capturedPayload = msg.Payload;
            return Task.CompletedTask;
        });

        var payload = new { OrderId = "456", Status = "Shipped" };
        var message = new RealtimeMessage("Orders", "OrderUpdated", payload, DateTime.UtcNow, "123");

        // Act
        var result = await handler.HandleAsync(message);

        // Assert
        Assert.True(result);
        Assert.NotNull(capturedPayload);
        Assert.Equal(payload, capturedPayload);
    }

    [Fact]
    public void RegisterHandler_NullMethod_ThrowsArgumentException()
    {
        // Arrange
        var handler = new RealtimeMessageHandler();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            handler.RegisterHandler(null!, (msg) => Task.CompletedTask));
    }

    [Fact]
    public void RegisterHandler_EmptyMethod_ThrowsArgumentException()
    {
        // Arrange
        var handler = new RealtimeMessageHandler();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            handler.RegisterHandler(string.Empty, (msg) => Task.CompletedTask));
    }

    [Fact]
    public void RegisterHandler_NullHandler_ThrowsArgumentNullException()
    {
        // Arrange
        var handler = new RealtimeMessageHandler();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            handler.RegisterHandler("Method1", null!));
    }

    [Fact]
    public async Task HandleAsync_NullMessage_ThrowsArgumentNullException()
    {
        // Arrange
        var handler = new RealtimeMessageHandler();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            handler.HandleAsync(null!));
    }

    [Fact]
    public async Task RegisterHandler_HandlerAsync_ExecutesAsynchronously()
    {
        // Arrange
        var handler = new RealtimeMessageHandler();
        var handlerExecuted = false;

        handler.RegisterHandler("AsyncMethod", async (msg) =>
        {
            await Task.Delay(10);
            handlerExecuted = true;
        });

        var message = new RealtimeMessage("Orders", "AsyncMethod", null, DateTime.UtcNow, "123");

        // Act
        var result = await handler.HandleAsync(message);

        // Assert
        Assert.True(result);
        Assert.True(handlerExecuted);
    }

    [Fact]
    public async Task RegisterHandler_ReplaceExistingHandler_UsesNewHandler()
    {
        // Arrange
        var handler = new RealtimeMessageHandler();
        var firstHandlerCalled = false;
        var secondHandlerCalled = false;

        handler.RegisterHandler("Method1", (msg) =>
        {
            firstHandlerCalled = true;
            return Task.CompletedTask;
        });

        handler.RegisterHandler("Method1", (msg) =>
        {
            secondHandlerCalled = true;
            return Task.CompletedTask;
        });

        var message = new RealtimeMessage("Orders", "Method1", null, DateTime.UtcNow, "123");

        // Act
        var result = await handler.HandleAsync(message);

        // Assert
        Assert.True(result);
        Assert.False(firstHandlerCalled);
        Assert.True(secondHandlerCalled);
    }

    [Fact]
    public void RealtimeMessageHandler_WithLogger_LogsDebugMessages()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<RealtimeMessageHandler>>();
        var handler = new RealtimeMessageHandler(mockLogger.Object);

        // Act
        handler.RegisterHandler("Method1", (msg) => Task.CompletedTask);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Registered handler")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

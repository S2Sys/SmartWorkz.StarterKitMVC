namespace SmartWorkz.Core.Tests.Diagnostics;

using SmartWorkz.Shared;

public class WebSocketClientTests
{
    #region IsConnected Tests

    [Fact]
    public void IsConnected_BeforeConnect_ReturnsFalse()
    {
        // Arrange
        using var client = new WebSocketClient();

        // Act
        var isConnected = client.IsConnected;

        // Assert
        Assert.False(isConnected);
    }

    #endregion

    #region ConnectAsync Tests

    [Fact]
    public async Task ConnectAsync_WithInvalidUri_ThrowsException()
    {
        // Arrange
        var uri = new Uri("ws://invalid.invalid.invalid.localhost:9999");

        // Act & Assert
        // WebSocketException is thrown for connection failures
        await Assert.ThrowsAsync<System.Net.WebSockets.WebSocketException>(() => client.ConnectAsync(uri));
    }

    #endregion

    #region SendAsync Tests

    [Fact]
    public async Task SendAsync_NotConnected_ThrowsException()
    {
        // Arrange

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.SendAsync("test message", CancellationToken.None));
    }

    [Fact]
    public async Task SendAsync_AfterDispose_ThrowsException()
    {
        // Arrange
        var client = new WebSocketClient();
        client.Dispose();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            client.SendAsync("test message", CancellationToken.None));
    }

    #endregion

    #region ReceiveAsync Tests

    [Fact]
    public async Task ReceiveAsync_NotConnected_ThrowsException()
    {
        // Arrange

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            client.ReceiveAsync(CancellationToken.None));
    }

    [Fact]
    public async Task ReceiveAsync_AfterDispose_ThrowsException()
    {
        // Arrange
        var client = new WebSocketClient();
        client.Dispose();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            client.ReceiveAsync(CancellationToken.None));
    }

    #endregion

    #region CloseAsync Tests

    [Fact]
    public async Task CloseAsync_NotConnected_DoesNotThrow()
    {
        // Arrange

        // Act & Assert - should not throw
        await client.CloseAsync(CancellationToken.None);
    }

    [Fact]
    public async Task CloseAsync_AfterDispose_DoesNotThrow()
    {
        // Arrange
        var client = new WebSocketClient();
        client.Dispose();

        // Act & Assert - should not throw even after dispose
        await client.CloseAsync(CancellationToken.None);
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_MultipleTimes_DoesNotThrow()
    {
        // Arrange
        var client = new WebSocketClient();

        // Act & Assert
        client.Dispose();
        client.Dispose(); // Second dispose should not throw
    }

    [Fact]
    public void Dispose_SetResourcesNull()
    {
        // Arrange
        var client = new WebSocketClient();

        // Act
        client.Dispose();

        // Assert - verify that subsequent operations throw
        Assert.Throws<ObjectDisposedException>(() =>
        {
            // Try to use client after dispose
            try
            {
                client.SendAsync("test", CancellationToken.None).Wait();
            }
            catch (AggregateException ae) when (ae.InnerException is ObjectDisposedException)
            {
                throw ae.InnerException;
            }
        });
    }

    [Fact]
    public void WebSocketClient_ImplementsIWebSocketClient()
    {
        // Arrange & Act
        var client = new WebSocketClient();

        // Assert
        Assert.IsAssignableFrom<IWebSocketClient>(client);

        // Cleanup
        client.Dispose();
    }

    [Fact]
    public void WebSocketClient_ImplementsIDisposable()
    {
        // Arrange & Act
        var client = new WebSocketClient();

        // Assert
        Assert.IsAssignableFrom<IDisposable>(client);

        // Cleanup
        client.Dispose();
    }

    [Fact]
    public void WebSocketClient_IsSealed()
    {
        // Arrange
        var type = typeof(WebSocketClient);

        // Act & Assert
        Assert.True(type.IsSealed, "WebSocketClient should be sealed");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task WebSocketClient_CanBeUsedWithUsingStatement()
    {
        // Arrange & Act

        // Assert - no exception should occur
        Assert.NotNull(client);
        Assert.False(client.IsConnected);

        // Cleanup is handled by using statement
        await Task.CompletedTask;
    }

    [Fact]
    public async Task WebSocketClient_ThrowsOnSendWhenNotConnected()
    {
        // Arrange

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.SendAsync("test", CancellationToken.None));

        Assert.NotNull(exception);
        Assert.Contains("not connected", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task WebSocketClient_ThrowsOnReceiveWhenNotConnected()
    {
        // Arrange

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.ReceiveAsync(CancellationToken.None));

        Assert.NotNull(exception);
        Assert.Contains("not connected", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}

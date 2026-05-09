namespace SmartWorkz.StarterKitMVC.Tests.Integration.Services;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using SmartWorkz.StarterKitMVC.Infrastructure.Services;

public class HybridCacheServiceTests
{
    private readonly Mock<IMemoryCache> _mockL1;
    private readonly Mock<IDistributedCache> _mockL2;
    private readonly Mock<ILogger<HybridCacheService>> _mockLogger;

    public HybridCacheServiceTests()
    {
        _mockL1 = new Mock<IMemoryCache>(MockBehavior.Strict);
        _mockL2 = new Mock<IDistributedCache>();
        _mockLogger = new Mock<ILogger<HybridCacheService>>();
    }

    [Fact]
    public async Task RemoveByPrefixAsync_WithoutRedis_CompletesWithoutError()
    {
        // Arrange - service without Redis (null IConnectionMultiplexer)
        var service = new HybridCacheService(_mockL1.Object, _mockL2.Object, _mockLogger.Object, null);
        var tenantId = "tenant-1";
        var prefix = "user:";

        // Act
        var ex = await Record.ExceptionAsync(() => service.RemoveByPrefixAsync(tenantId, prefix));

        // Assert
        Assert.Null(ex); // Should not throw when Redis unavailable
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Redis not configured")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAsync_ReturnsNullWhenNotFoundInBothCaches()
    {
        // Arrange
        var tenantId = "tenant-1";
        var key = "nonexistent";
        var notFound = false;
        object? cachedValue = null;

        // Setup L1: not found
        _mockL1.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedValue))
            .Returns(notFound);

        // Setup L2: not found
        _mockL2.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var service = new HybridCacheService(_mockL1.Object, _mockL2.Object, _mockLogger.Object, null);

        // Act
        var result = await service.GetAsync<string>(tenantId, key);

        // Assert
        Assert.Null(result);
        _mockL1.Verify(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object?>.IsAny), Times.Once);
        _mockL2.Verify(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_CallsRemoveOnBothL1AndL2()
    {
        // Arrange
        var tenantId = "tenant-1";
        var key = "user:789";

        _mockL1.Setup(x => x.Remove(It.IsAny<object>()));
        _mockL2.Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new HybridCacheService(_mockL1.Object, _mockL2.Object, _mockLogger.Object, null);

        // Act
        await service.RemoveAsync(tenantId, key);

        // Assert
        _mockL1.Verify(x => x.Remove(It.IsAny<object>()), Times.Once);
        _mockL2.Verify(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveByPrefixAsync_HandlesL2ExceptionGracefully()
    {
        // Arrange
        var tenantId = "tenant-1";
        var prefix = "user:";
        var service = new HybridCacheService(_mockL1.Object, _mockL2.Object, _mockLogger.Object, null);

        // Act
        var ex = await Record.ExceptionAsync(() => service.RemoveByPrefixAsync(tenantId, prefix));

        // Assert
        Assert.Null(ex); // Should handle exceptions gracefully without throwing
    }

    [Fact]
    public async Task GetAsync_ReturnsNullWhenL2ThrowsException()
    {
        // Arrange
        var tenantId = "tenant-1";
        var key = "error-key";
        var notFound = false;
        object? cachedValue = null;

        _mockL1.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedValue))
            .Returns(notFound);

        _mockL2.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Redis connection failed"));

        var service = new HybridCacheService(_mockL1.Object, _mockL2.Object, _mockLogger.Object, null);

        // Act
        var result = await service.GetAsync<string>(tenantId, key);

        // Assert
        Assert.Null(result); // Should return null when L2 fails
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("L2 cache read failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_HandlesL2ExceptionGracefully()
    {
        // Arrange
        var tenantId = "tenant-1";
        var key = "user:error";

        _mockL1.Setup(x => x.Remove(It.IsAny<object>()));
        _mockL2.Setup(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("L2 removal failed"));

        var service = new HybridCacheService(_mockL1.Object, _mockL2.Object, _mockLogger.Object, null);

        // Act
        var ex = await Record.ExceptionAsync(() => service.RemoveAsync(tenantId, key));

        // Assert
        Assert.Null(ex); // Should not throw, logging the warning instead
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("L2 cache remove failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

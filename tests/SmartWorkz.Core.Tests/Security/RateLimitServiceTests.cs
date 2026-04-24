namespace SmartWorkz.Core.Tests.Security;

using SmartWorkz.Shared.Security.RateLimit;

public class RateLimitServiceTests
{
    private readonly RateLimitService _service = new();

    #region IsRequestAllowedAsync Tests

    [Fact]
    public async Task IsRequestAllowedAsync_WithinLimit_ReturnsTrue()
    {
        // Arrange
        const string clientId = "test-client-1";
        const int maxRequests = 5;
        const int windowSeconds = 60;

        // Act
        var result = await _service.IsRequestAllowedAsync(clientId, maxRequests, windowSeconds);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsRequestAllowedAsync_ExceedsLimit_ReturnsFalse()
    {
        // Arrange
        const string clientId = "test-client-2";
        const int maxRequests = 2;
        const int windowSeconds = 60;

        // Act - Make requests equal to limit
        await _service.IsRequestAllowedAsync(clientId, maxRequests, windowSeconds);
        await _service.IsRequestAllowedAsync(clientId, maxRequests, windowSeconds);

        // Act - Third request should be denied
        var result = await _service.IsRequestAllowedAsync(clientId, maxRequests, windowSeconds);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsRequestAllowedAsync_MultipleClientsIndependent_ReturnsTrueForAllowedClients()
    {
        // Arrange
        const int maxRequests = 2;
        const int windowSeconds = 60;

        // Act - Client 1 makes 2 requests
        await _service.IsRequestAllowedAsync("client-1", maxRequests, windowSeconds);
        await _service.IsRequestAllowedAsync("client-1", maxRequests, windowSeconds);

        // Act - Client 2 makes 1 request
        var result = await _service.IsRequestAllowedAsync("client-2", maxRequests, windowSeconds);

        // Assert - Client 2 should have tokens available
        Assert.True(result);
    }

    [Fact]
    public async Task IsRequestAllowedAsync_WithNullClientId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.IsRequestAllowedAsync(null, 100, 60)
        );
    }

    [Fact]
    public async Task IsRequestAllowedAsync_WithInvalidMaxRequests_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.IsRequestAllowedAsync("client-1", 0, 60)
        );
    }

    [Fact]
    public async Task IsRequestAllowedAsync_WithInvalidWindowSeconds_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.IsRequestAllowedAsync("client-1", 100, 0)
        );
    }

    #endregion

    #region GetStatusAsync Tests

    [Fact]
    public async Task GetStatusAsync_ReturnsRemainingRequests()
    {
        // Arrange
        const string clientId = "test-client-3";
        const int maxRequests = 10;
        const int windowSeconds = 60;

        // Act - Make 3 requests
        await _service.IsRequestAllowedAsync(clientId, maxRequests, windowSeconds);
        await _service.IsRequestAllowedAsync(clientId, maxRequests, windowSeconds);
        await _service.IsRequestAllowedAsync(clientId, maxRequests, windowSeconds);

        // Act - Get status
        var status = await _service.GetStatusAsync(clientId, maxRequests);

        // Assert
        Assert.Equal(7, status.RemainingRequests);
        Assert.Equal(maxRequests, status.MaxRequests);
        Assert.InRange(status.ResetAfterSeconds, 0, windowSeconds);
    }

    [Fact]
    public async Task GetStatusAsync_NoRequestsMade_ReturnsFullCapacity()
    {
        // Arrange
        const string clientId = "test-client-4";
        const int maxRequests = 15;

        // Act
        var status = await _service.GetStatusAsync(clientId, maxRequests);

        // Assert
        Assert.Equal(maxRequests, status.RemainingRequests);
        Assert.Equal(maxRequests, status.MaxRequests);
    }

    [Fact]
    public async Task GetStatusAsync_WithNullClientId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetStatusAsync(null, 100)
        );
    }

    [Fact]
    public async Task GetStatusAsync_WithInvalidMaxRequests_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetStatusAsync("client-1", 0)
        );
    }

    [Fact]
    public async Task GetStatusAsync_ReturnsValidResetTime()
    {
        // Arrange
        const string clientId = "test-client-5";
        const int maxRequests = 5;
        var beforeTime = DateTime.UtcNow;

        // Act
        var status = await _service.GetStatusAsync(clientId, maxRequests);
        var afterTime = DateTime.UtcNow;

        // Assert
        Assert.NotEqual(default(DateTime), status.ResetTime);
        Assert.True(status.ResetTime >= beforeTime);
        Assert.True(status.ResetTime <= afterTime.AddSeconds(65));
    }

    #endregion

    #region ResetAsync Tests

    [Fact]
    public async Task ResetAsync_ClearsRateLimit()
    {
        // Arrange
        const string clientId = "test-client-6";
        const int maxRequests = 2;
        const int windowSeconds = 60;

        // Act - Exhaust the limit
        await _service.IsRequestAllowedAsync(clientId, maxRequests, windowSeconds);
        await _service.IsRequestAllowedAsync(clientId, maxRequests, windowSeconds);

        // Verify limit is exhausted
        var resultBeforeReset = await _service.IsRequestAllowedAsync(clientId, maxRequests, windowSeconds);
        Assert.False(resultBeforeReset);

        // Act - Reset the limit
        var resetResult = await _service.ResetAsync(clientId);

        // Assert - Reset was successful
        Assert.True(resetResult);

        // Act - New request should be allowed
        var resultAfterReset = await _service.IsRequestAllowedAsync(clientId, maxRequests, windowSeconds);

        // Assert
        Assert.True(resultAfterReset);
    }

    [Fact]
    public async Task ResetAsync_WithNullClientId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.ResetAsync(null)
        );
    }

    [Fact]
    public async Task ResetAsync_NonExistentClientId_ReturnsFalse()
    {
        // Act
        var result = await _service.ResetAsync("non-existent-client");

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Token Bucket Refill Tests

    [Fact]
    public async Task TokenBucketRefill_WaitAndRetry_RefillsTokens()
    {
        // Arrange
        const string clientId = "test-client-7";
        const int maxRequests = 5;
        const int windowSeconds = 3;

        // Act - Use some tokens
        await _service.IsRequestAllowedAsync(clientId, maxRequests, windowSeconds);
        await _service.IsRequestAllowedAsync(clientId, maxRequests, windowSeconds);
        await _service.IsRequestAllowedAsync(clientId, maxRequests, windowSeconds);

        var statusBefore = await _service.GetStatusAsync(clientId, maxRequests);

        // Wait for partial refill (1 second of 3-second window)
        await Task.Delay(1000);

        var statusAfter = await _service.GetStatusAsync(clientId, maxRequests);

        // Assert - Tokens should have been refilled
        Assert.True(statusAfter.RemainingRequests > statusBefore.RemainingRequests);
    }

    #endregion

    #region Concurrent Access Tests

    [Fact]
    public async Task IsRequestAllowedAsync_ConcurrentRequests_ThreadSafe()
    {
        // Arrange
        const string clientId = "test-client-8";
        const int maxRequests = 50;
        const int windowSeconds = 60;
        const int taskCount = 100;

        // Act
        var tasks = Enumerable.Range(0, taskCount)
            .Select(_ => _service.IsRequestAllowedAsync(clientId, maxRequests, windowSeconds))
            .ToList();

        var results = await Task.WhenAll(tasks);

        // Assert - First 50 should be allowed, rest should be denied
        var allowedCount = results.Count(r => r);
        Assert.Equal(maxRequests, allowedCount);

        var deniedCount = results.Count(r => !r);
        Assert.Equal(taskCount - maxRequests, deniedCount);
    }

    #endregion
}

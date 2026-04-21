namespace SmartWorkz.Mobile.Tests.Services;

using Microsoft.Extensions.Logging;
using Moq;
using SmartWorkz.Shared;
using SmartWorkz.Mobile;

public class RequestDeduplicationServiceTests
{
    private readonly Mock<ILogger<RequestDeduplicationService>> _logger = new();

    [Fact]
    public async Task GetOrExecuteAsync_FirstCall_ExecutesAndCaches()
    {
        // Arrange
        var svc = new RequestDeduplicationService(_logger.Object);
        var callCount = 0;
        var tcs = new TaskCompletionSource<Result<string>>();

        async Task<Result<string>> Factory()
        {
            callCount++;
            // Wait for completion signal to keep the factory in-flight
            return await tcs.Task;
        }

        // Act: Start first call (will wait on tcs)
        var task1 = svc.GetOrExecuteAsync("key1", Factory);

        // Act: Start second identical call while first is still in-flight
        var task2 = svc.GetOrExecuteAsync("key1", Factory);

        // Both tasks should be waiting
        Assert.False(task1.IsCompleted);
        Assert.False(task2.IsCompleted);

        // Complete the factory
        tcs.SetResult(Result.Ok("value"));

        // Wait for both to complete
        var result1 = await task1;
        var result2 = await task2;

        // Assert: Factory called exactly once (second call reused first call's task)
        Assert.Equal(1, callCount);
        Assert.True(result1.Succeeded);
        Assert.Equal("value", result1.Data);
        Assert.True(result2.Succeeded);
        Assert.Equal("value", result2.Data);
    }

    [Fact]
    public async Task GetOrExecuteAsync_DuplicateCallConcurrent_ExecutesOnce()
    {
        // Arrange
        var svc = new RequestDeduplicationService(_logger.Object);
        var callCount = 0;
        var tcs = new TaskCompletionSource<Result<string>>();

        async Task<Result<string>> Factory()
        {
            callCount++;
            // Wait for completion signal to simulate slow factory
            return await tcs.Task;
        }

        // Act: Start two concurrent calls with identical keys
        var task1 = svc.GetOrExecuteAsync("key1", Factory);
        var task2 = svc.GetOrExecuteAsync("key1", Factory);

        // Both tasks should be waiting for the factory to complete
        Assert.False(task1.IsCompleted);
        Assert.False(task2.IsCompleted);

        // Complete the factory
        tcs.SetResult(Result.Ok("value"));

        // Wait for both to complete
        var result1 = await task1;
        var result2 = await task2;

        // Assert: Factory called exactly once despite two concurrent calls
        Assert.Equal(1, callCount);
        Assert.True(result1.Succeeded);
        Assert.Equal("value", result1.Data);
        Assert.True(result2.Succeeded);
        Assert.Equal("value", result2.Data);
    }

    [Fact]
    public async Task GetOrExecuteAsync_DifferentKeys_ExecutesMultipleTimes()
    {
        // Arrange
        var svc = new RequestDeduplicationService(_logger.Object);
        var callCount = 0;

        async Task<Result<string>> Factory()
        {
            callCount++;
            await Task.Delay(10);
            return Result.Ok("value");
        }

        // Act: Call with key1
        var result1 = await svc.GetOrExecuteAsync("key1", Factory);

        // Assert: First call executed
        Assert.True(result1.Succeeded);
        Assert.Equal("value", result1.Data);
        Assert.Equal(1, callCount);

        // Act: Call with key2 (different key)
        var result2 = await svc.GetOrExecuteAsync("key2", Factory);

        // Assert: Different key causes factory to execute again
        Assert.True(result2.Succeeded);
        Assert.Equal("value", result2.Data);
        Assert.Equal(2, callCount); // Called twice for different keys
    }

    [Fact]
    public async Task GetOrExecuteAsync_FailureReleasesCacheEntry()
    {
        // Arrange
        var svc = new RequestDeduplicationService(_logger.Object);
        var callCount = 0;

        async Task<Result<string>> Factory()
        {
            callCount++;
            await Task.Delay(10);
            return Result.Fail<string>(new Error("TEST.ERROR", "test failure"));
        }

        // Act: First call fails
        var result1 = await svc.GetOrExecuteAsync("key1", Factory);

        // Assert: First call executed and failed
        Assert.False(result1.Succeeded);
        Assert.Equal(1, callCount);

        // Act: Second call with same key
        var result2 = await svc.GetOrExecuteAsync("key1", Factory);

        // Assert: Failure released cache entry, so factory executed again
        Assert.False(result2.Succeeded);
        Assert.Equal(2, callCount); // Called twice - failure released cache
    }

    [Fact]
    public async Task GetOrExecuteAsync_SuccessReleasesCacheEntry()
    {
        // Arrange
        var svc = new RequestDeduplicationService(_logger.Object);
        var callCount = 0;

        async Task<Result<string>> Factory()
        {
            callCount++;
            await Task.Delay(10);
            return Result.Ok("success");
        }

        // Act: First call succeeds
        var result1 = await svc.GetOrExecuteAsync("key1", Factory);

        // Assert: First call executed
        Assert.True(result1.Succeeded);
        Assert.Equal("success", result1.Data);
        Assert.Equal(1, callCount);

        // Wait a bit to ensure completion
        await Task.Delay(50);

        // Act: Second call with same key (after completion)
        var result2 = await svc.GetOrExecuteAsync("key1", Factory);

        // Assert: Success also released cache entry, so factory executed again
        Assert.True(result2.Succeeded);
        Assert.Equal("success", result2.Data);
        Assert.Equal(2, callCount); // Called twice - success also releases cache
    }
}

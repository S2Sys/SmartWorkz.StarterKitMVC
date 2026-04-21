using SmartWorkz.Shared;

namespace SmartWorkz.Core.Tests.Resilience;

public class RateLimiterTests
{
    #region Constructor and Options Tests

    [Fact]
    public void Constructor_WithDefaultOptions_ShouldInitializeSuccessfully()
    {
        // Arrange & Act
        var options = new RateLimiterOptions();
        var rateLimiter = new RateLimiter(options);

        // Assert
        Assert.NotNull(rateLimiter);
        Assert.Equal(100, options.MaxRequests);
        Assert.Equal(60000, options.WindowMilliseconds);
        Assert.Equal(RateLimiterStrategy.TokenBucket, options.Strategy);
    }

    [Fact]
    public void Constructor_WithCustomOptions_ShouldUseProvidedValues()
    {
        // Arrange
        var options = new RateLimiterOptions
        {
            MaxRequests = 50,
            WindowMilliseconds = 30000,
            Strategy = RateLimiterStrategy.TokenBucket
        };

        // Act
        var rateLimiter = new RateLimiter(options);

        // Assert
        Assert.NotNull(rateLimiter);
    }

    [Fact]
    public void RateLimiterOptions_RefillRate_ShouldCalculateCorrectly()
    {
        // Arrange
        var options = new RateLimiterOptions
        {
            MaxRequests = 100,
            WindowMilliseconds = 60000
        };

        // Act
        var refillRate = options.RefillRate;

        // Assert
        Assert.Equal(100.0 / 60000.0, refillRate);
    }

    [Fact]
    public void RateLimiterOptions_IsValid_WithValidOptions_ShouldReturnTrue()
    {
        // Arrange
        var options = new RateLimiterOptions
        {
            MaxRequests = 100,
            WindowMilliseconds = 60000
        };

        // Act
        var isValid = options.IsValid();

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void RateLimiterOptions_IsValid_WithZeroMaxRequests_ShouldReturnFalse()
    {
        // Arrange
        var options = new RateLimiterOptions
        {
            MaxRequests = 0,
            WindowMilliseconds = 60000
        };

        // Act
        var isValid = options.IsValid();

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void RateLimiterOptions_IsValid_WithZeroWindow_ShouldReturnFalse()
    {
        // Arrange
        var options = new RateLimiterOptions
        {
            MaxRequests = 100,
            WindowMilliseconds = 0
        };

        // Act
        var isValid = options.IsValid();

        // Assert
        Assert.False(isValid);
    }

    #endregion

    #region TryAcquireAsync Tests

    [Fact]
    public async Task TryAcquireAsync_WithinLimit_ShouldReturnSuccess()
    {
        // Arrange
        var options = new RateLimiterOptions { MaxRequests = 10, WindowMilliseconds = 60000 };
        var rateLimiter = new RateLimiter(options);
        var identifier = "user:123";

        // Act
        var result = await rateLimiter.TryAcquireAsync(identifier);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task TryAcquireAsync_ExceedingLimit_ShouldReturnFailure()
    {
        // Arrange
        var options = new RateLimiterOptions { MaxRequests = 2, WindowMilliseconds = 60000 };
        var rateLimiter = new RateLimiter(options);
        var identifier = "user:123";

        // Act
        var result1 = await rateLimiter.TryAcquireAsync(identifier);
        var result2 = await rateLimiter.TryAcquireAsync(identifier);
        var result3 = await rateLimiter.TryAcquireAsync(identifier);

        // Assert
        Assert.True(result1.Succeeded && result1.Data);
        Assert.True(result2.Succeeded && result2.Data);
        Assert.True(result3.Succeeded);
        Assert.False(result3.Data);
    }

    [Fact]
    public async Task TryAcquireAsync_WithMultipleCosts_ShouldConsumeCorrectTokens()
    {
        // Arrange
        var options = new RateLimiterOptions { MaxRequests = 10, WindowMilliseconds = 60000 };
        var rateLimiter = new RateLimiter(options);
        var identifier = "user:456";

        // Act
        var result1 = await rateLimiter.TryAcquireAsync(identifier, cost: 5);
        var result2 = await rateLimiter.TryAcquireAsync(identifier, cost: 5);
        var result3 = await rateLimiter.TryAcquireAsync(identifier, cost: 1);

        // Assert
        Assert.True(result1.Succeeded && result1.Data);
        Assert.True(result2.Succeeded && result2.Data);
        Assert.True(result3.Succeeded);
        Assert.False(result3.Data);
    }

    [Fact]
    public async Task TryAcquireAsync_WithInvalidCost_ShouldReturnFailure()
    {
        // Arrange
        var options = new RateLimiterOptions { MaxRequests = 10, WindowMilliseconds = 60000 };
        var rateLimiter = new RateLimiter(options);
        var identifier = "user:789";

        // Act
        var result = await rateLimiter.TryAcquireAsync(identifier, cost: 0);

        // Assert
        Assert.True(result.Succeeded);
        Assert.False(result.Data);
    }

    [Fact]
    public async Task TryAcquireAsync_WithNegativeCost_ShouldReturnFailure()
    {
        // Arrange
        var options = new RateLimiterOptions { MaxRequests = 10, WindowMilliseconds = 60000 };
        var rateLimiter = new RateLimiter(options);
        var identifier = "user:101";

        // Act
        var result = await rateLimiter.TryAcquireAsync(identifier, cost: -1);

        // Assert
        Assert.True(result.Succeeded);
        Assert.False(result.Data);
    }

    [Fact]
    public async Task TryAcquireAsync_DifferentIdentifiers_ShouldBeIsolated()
    {
        // Arrange
        var options = new RateLimiterOptions { MaxRequests = 2, WindowMilliseconds = 60000 };
        var rateLimiter = new RateLimiter(options);

        // Act
        var result1a = await rateLimiter.TryAcquireAsync("user:1");
        var result1b = await rateLimiter.TryAcquireAsync("user:1");
        var result1c = await rateLimiter.TryAcquireAsync("user:1");
        var result2a = await rateLimiter.TryAcquireAsync("user:2");
        var result2b = await rateLimiter.TryAcquireAsync("user:2");
        var result2c = await rateLimiter.TryAcquireAsync("user:2");

        // Assert
        Assert.True(result1a.Data && result1b.Data);
        Assert.False(result1c.Data);
        Assert.True(result2a.Data && result2b.Data);
        Assert.False(result2c.Data);
    }

    [Fact]
    public async Task TryAcquireAsync_WithCostLargerThanMax_ShouldReturnFailure()
    {
        // Arrange
        var options = new RateLimiterOptions { MaxRequests = 10, WindowMilliseconds = 60000 };
        var rateLimiter = new RateLimiter(options);
        var identifier = "user:202";

        // Act
        var result = await rateLimiter.TryAcquireAsync(identifier, cost: 15);

        // Assert - invalid cost returns Fail (Succeeded=false)
        Assert.False(result.Succeeded);
        Assert.NotNull(result.Error);
        Assert.Contains("exceeds", result.Error!.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TryAcquireAsync_AllowsDefaultCostOfOne()
    {
        // Arrange
        var options = new RateLimiterOptions { MaxRequests = 3, WindowMilliseconds = 60000 };
        var rateLimiter = new RateLimiter(options);
        var identifier = "user:303";

        // Act
        var result1 = await rateLimiter.TryAcquireAsync(identifier);
        var result2 = await rateLimiter.TryAcquireAsync(identifier);
        var result3 = await rateLimiter.TryAcquireAsync(identifier);
        var result4 = await rateLimiter.TryAcquireAsync(identifier);

        // Assert
        Assert.True(result1.Data && result2.Data && result3.Data);
        Assert.False(result4.Data);
    }

    #endregion

    #region GetAvailableTokensAsync Tests

    [Fact]
    public async Task GetAvailableTokensAsync_InitialState_ShouldReturnMaxRequests()
    {
        // Arrange
        var options = new RateLimiterOptions { MaxRequests = 100, WindowMilliseconds = 60000 };
        var rateLimiter = new RateLimiter(options);
        var identifier = "user:400";

        // Act
        var availableTokens = await rateLimiter.GetAvailableTokensAsync(identifier);

        // Assert
        Assert.Equal(100, availableTokens);
    }

    [Fact]
    public async Task GetAvailableTokensAsync_AfterAcquisition_ShouldReturnRemainingTokens()
    {
        // Arrange
        var options = new RateLimiterOptions { MaxRequests = 100, WindowMilliseconds = 60000 };
        var rateLimiter = new RateLimiter(options);
        var identifier = "user:500";

        // Act
        await rateLimiter.TryAcquireAsync(identifier, cost: 30);
        var availableTokens = await rateLimiter.GetAvailableTokensAsync(identifier);

        // Assert
        Assert.Equal(70, availableTokens);
    }

    [Fact]
    public async Task GetAvailableTokensAsync_UnknownIdentifier_ShouldReturnMaxRequests()
    {
        // Arrange
        var options = new RateLimiterOptions { MaxRequests = 50, WindowMilliseconds = 60000 };
        var rateLimiter = new RateLimiter(options);

        // Act
        var availableTokens = await rateLimiter.GetAvailableTokensAsync("unknown:user");

        // Assert
        Assert.Equal(50, availableTokens);
    }

    #endregion

    #region ResetAsync Tests

    [Fact]
    public async Task ResetAsync_ShouldRestoreTokens()
    {
        // Arrange
        var options = new RateLimiterOptions { MaxRequests = 10, WindowMilliseconds = 60000 };
        var rateLimiter = new RateLimiter(options);
        var identifier = "user:600";

        // Act
        await rateLimiter.TryAcquireAsync(identifier, cost: 8);
        var beforeReset = await rateLimiter.GetAvailableTokensAsync(identifier);
        await rateLimiter.ResetAsync(identifier);
        var afterReset = await rateLimiter.GetAvailableTokensAsync(identifier);

        // Assert
        Assert.Equal(2, beforeReset);
        Assert.Equal(10, afterReset);
    }

    [Fact]
    public async Task ResetAsync_UnknownIdentifier_ShouldCompleteSuccessfully()
    {
        // Arrange
        var options = new RateLimiterOptions { MaxRequests = 10, WindowMilliseconds = 60000 };
        var rateLimiter = new RateLimiter(options);

        // Act & Assert
        await rateLimiter.ResetAsync("unknown:identifier");
    }

    #endregion

    #region ClearAsync Tests

    [Fact]
    public async Task ClearAsync_ShouldResetAllIdentifiers()
    {
        // Arrange
        var options = new RateLimiterOptions { MaxRequests = 10, WindowMilliseconds = 60000 };
        var rateLimiter = new RateLimiter(options);

        // Act
        await rateLimiter.TryAcquireAsync("user:1", cost: 5);
        await rateLimiter.TryAcquireAsync("user:2", cost: 3);
        await rateLimiter.ClearAsync();
        var tokens1 = await rateLimiter.GetAvailableTokensAsync("user:1");
        var tokens2 = await rateLimiter.GetAvailableTokensAsync("user:2");

        // Assert
        Assert.Equal(10, tokens1);
        Assert.Equal(10, tokens2);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task TryAcquireAsync_ConcurrentRequests_ShouldMaintainLimit()
    {
        // Arrange
        var options = new RateLimiterOptions { MaxRequests = 100, WindowMilliseconds = 60000 };
        var rateLimiter = new RateLimiter(options);
        var identifier = "user:concurrent";
        var successCount = 0;
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 150; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var result = await rateLimiter.TryAcquireAsync(identifier);
                if (result.Succeeded && result.Data)
                {
                    Interlocked.Increment(ref successCount);
                }
            }));
        }
        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(100, successCount);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task TryAcquireAsync_Failure_ShouldReturnDataFalse()
    {
        // Arrange
        var options = new RateLimiterOptions { MaxRequests = 1, WindowMilliseconds = 60000 };
        var rateLimiter = new RateLimiter(options);
        var identifier = "user:700";

        // Act
        await rateLimiter.TryAcquireAsync(identifier);
        var result = await rateLimiter.TryAcquireAsync(identifier);

        // Assert - rate limit hit returns Succeeded=true with Data=false
        Assert.True(result.Succeeded);
        Assert.False(result.Data);
    }

    #endregion

    #region Large Request Handling Tests

    [Fact]
    public async Task TryAcquireAsync_HighMaxRequests_ShouldHandleSuccessfully()
    {
        // Arrange
        var options = new RateLimiterOptions { MaxRequests = 10000, WindowMilliseconds = 60000 };
        var rateLimiter = new RateLimiter(options);
        var identifier = "user:large";

        // Act
        var result = await rateLimiter.TryAcquireAsync(identifier, cost: 5000);

        // Assert
        Assert.True(result.Succeeded && result.Data);
    }

    [Fact]
    public async Task TryAcquireAsync_MultipleIdentifiersStress_ShouldMaintainIsolation()
    {
        // Arrange
        var options = new RateLimiterOptions { MaxRequests = 50, WindowMilliseconds = 60000 };
        var rateLimiter = new RateLimiter(options);
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            var userId = $"user:{i % 10}";
            tasks.Add(rateLimiter.TryAcquireAsync(userId));
        }
        await Task.WhenAll(tasks);

        // Assert
        for (int i = 0; i < 10; i++)
        {
            var tokens = await rateLimiter.GetAvailableTokensAsync($"user:{i}");
            Assert.True(tokens >= 0);
            Assert.True(tokens <= 50);
        }
    }

    #endregion

    #region CancellationToken Tests

    [Fact]
    public async Task TryAcquireAsync_WithCancellationToken_ShouldRespond()
    {
        // Arrange
        var options = new RateLimiterOptions { MaxRequests = 10, WindowMilliseconds = 60000 };
        var rateLimiter = new RateLimiter(options);
        var identifier = "user:cancel";
        using var cts = new CancellationTokenSource();

        // Act
        var result = await rateLimiter.TryAcquireAsync(identifier, cancellationToken: cts.Token);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ResetAsync_WithCancellationToken_ShouldRespond()
    {
        // Arrange
        var options = new RateLimiterOptions { MaxRequests = 10, WindowMilliseconds = 60000 };
        var rateLimiter = new RateLimiter(options);
        var identifier = "user:reset-cancel";

        // Act & Assert
        await rateLimiter.ResetAsync(identifier, cts.Token);
    }

    [Fact]
    public async Task ClearAsync_WithCancellationToken_ShouldRespond()
    {
        // Arrange
        var options = new RateLimiterOptions { MaxRequests = 10, WindowMilliseconds = 60000 };
        var rateLimiter = new RateLimiter(options);

        // Act & Assert
        await rateLimiter.ClearAsync(cts.Token);
    }

    #endregion
}



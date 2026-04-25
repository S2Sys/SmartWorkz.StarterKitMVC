using Xunit;
using SmartWorkz.Core.Shared.Webhooks.Models;

namespace SmartWorkz.Core.Tests.Webhooks;

/// <summary>
/// Unit tests for WebhookRetryPolicy exponential backoff calculation.
/// </summary>
public class WebhookRetryPolicyTests
{
    [Fact]
    public void GetRetryDelayMs_WithFirstAttempt_ReturnsInitialDelay()
    {
        // Arrange
        var policy = new WebhookRetryPolicy { InitialDelayMs = 1000, BackoffMultiplier = 2.0 };

        // Act
        var delay = policy.GetRetryDelayMs(1);

        // Assert
        Assert.Equal(1000, delay);
    }

    [Fact]
    public void GetRetryDelayMs_WithSecondAttempt_DoubleInitialDelay()
    {
        // Arrange
        var policy = new WebhookRetryPolicy { InitialDelayMs = 1000, BackoffMultiplier = 2.0 };

        // Act
        var delay = policy.GetRetryDelayMs(2);

        // Assert
        Assert.Equal(2000, delay);
    }

    [Fact]
    public void GetRetryDelayMs_WithThirdAttempt_QuadrupleInitialDelay()
    {
        // Arrange
        var policy = new WebhookRetryPolicy { InitialDelayMs = 1000, BackoffMultiplier = 2.0 };

        // Act
        var delay = policy.GetRetryDelayMs(3);

        // Assert
        Assert.Equal(4000, delay);
    }

    [Fact]
    public void GetRetryDelayMs_WithFifthAttempt_ExponentialBackoff()
    {
        // Arrange
        var policy = new WebhookRetryPolicy { InitialDelayMs = 1000, BackoffMultiplier = 2.0 };

        // Act
        var delay = policy.GetRetryDelayMs(5);

        // Assert
        // 1000 * 2^(5-1) = 1000 * 16 = 16000
        Assert.Equal(16000, delay);
    }

    [Fact]
    public void GetRetryDelayMs_ExceedingMaxDelay_CapsAtMaxDelay()
    {
        // Arrange
        var policy = new WebhookRetryPolicy
        {
            InitialDelayMs = 1000,
            BackoffMultiplier = 2.0,
            MaxDelayMs = 5000
        };

        // Act - attempt 4: 1000 * 2^3 = 8000, should cap to 5000
        var delay = policy.GetRetryDelayMs(4);

        // Assert
        Assert.Equal(5000, delay);
    }

    [Fact]
    public void GetRetryDelayMs_WithZeroAttempt_ThrowsArgumentException()
    {
        // Arrange
        var policy = new WebhookRetryPolicy();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => policy.GetRetryDelayMs(0));
    }

    [Fact]
    public void GetRetryDelayMs_WithNegativeAttempt_ThrowsArgumentException()
    {
        // Arrange
        var policy = new WebhookRetryPolicy();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => policy.GetRetryDelayMs(-1));
    }

    [Fact]
    public void GetRetryDelayMs_WithLargeAttemptNumber_HandlesOverflowGracefully()
    {
        // Arrange
        var policy = new WebhookRetryPolicy
        {
            InitialDelayMs = 1000,
            BackoffMultiplier = 2.0,
            MaxDelayMs = 300_000
        };

        // Act - very large attempt number that would overflow without protection
        var delay = policy.GetRetryDelayMs(100);

        // Assert - should return MaxDelayMs instead of overflowing
        Assert.Equal(300_000, delay);
        Assert.True(delay <= policy.MaxDelayMs);
    }

    [Fact]
    public void GetRetryDelayMs_WithCustomMultiplier_AppliesCorrectly()
    {
        // Arrange
        var policy = new WebhookRetryPolicy
        {
            InitialDelayMs = 100,
            BackoffMultiplier = 3.0
        };

        // Act - attempt 3: 100 * 3^(3-1) = 100 * 9 = 900
        var delay = policy.GetRetryDelayMs(3);

        // Assert
        Assert.Equal(900, delay);
    }

    [Fact]
    public void GetRetryDelayMs_WithCustomInitialDelay_AppliesCorrectly()
    {
        // Arrange
        var policy = new WebhookRetryPolicy
        {
            InitialDelayMs = 500,
            BackoffMultiplier = 2.0
        };

        // Act - attempt 3: 500 * 2^(3-1) = 500 * 4 = 2000
        var delay = policy.GetRetryDelayMs(3);

        // Assert
        Assert.Equal(2000, delay);
    }

    [Fact]
    public void GetRetryDelayMs_SequentialCalls_ProduceMonotonicallyIncreasingDelays()
    {
        // Arrange
        var policy = new WebhookRetryPolicy
        {
            InitialDelayMs = 1000,
            BackoffMultiplier = 2.0,
            MaxDelayMs = 60_000
        };

        // Act
        var delay1 = policy.GetRetryDelayMs(1);
        var delay2 = policy.GetRetryDelayMs(2);
        var delay3 = policy.GetRetryDelayMs(3);
        var delay4 = policy.GetRetryDelayMs(4);

        // Assert - each delay should be greater than or equal to the previous
        Assert.True(delay1 <= delay2);
        Assert.True(delay2 <= delay3);
        Assert.True(delay3 <= delay4);
    }

    [Fact]
    public void DefaultValues_AreReasonable()
    {
        // Arrange
        var policy = new WebhookRetryPolicy();

        // Assert
        Assert.Equal(5, policy.MaxRetries);
        Assert.Equal(1000, policy.InitialDelayMs);
        Assert.Equal(2.0, policy.BackoffMultiplier);
        Assert.Equal(300_000, policy.MaxDelayMs);
    }

    [Fact]
    public void GetRetryDelayMs_AlwaysReturnsPositiveValue()
    {
        // Arrange
        var policy = new WebhookRetryPolicy();

        // Act & Assert
        for (int attempt = 1; attempt <= 10; attempt++)
        {
            var delay = policy.GetRetryDelayMs(attempt);
            Assert.True(delay > 0, $"Delay for attempt {attempt} should be positive");
        }
    }
}

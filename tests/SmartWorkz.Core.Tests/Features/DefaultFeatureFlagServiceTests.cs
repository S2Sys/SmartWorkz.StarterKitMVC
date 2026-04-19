using SmartWorkz.Core.Shared.Features;

namespace SmartWorkz.Core.Tests.Features;

/// <summary>
/// Tests for DefaultFeatureFlagService - global feature flag store.
/// Validates thread-safe in-memory storage, async operations, and edge cases.
/// </summary>
public class DefaultFeatureFlagServiceTests : IDisposable
{
    private readonly DefaultFeatureFlagService _service;

    public DefaultFeatureFlagServiceTests()
    {
        _service = new DefaultFeatureFlagService();
    }

    public void Dispose()
    {
        // Cleanup if needed
    }

    #region IsEnabledAsync Tests

    [Fact]
    public async Task IsEnabledAsync_EnabledFlag_ReturnsTrue()
    {
        // Arrange
        _service.EnableFlag("NEW_DASHBOARD");

        // Act
        var result = await _service.IsEnabledAsync("NEW_DASHBOARD");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsEnabledAsync_DisabledFlag_ReturnsFalse()
    {
        // Arrange
        _service.EnableFlag("OLD_FEATURE");
        _service.DisableFlag("OLD_FEATURE");

        // Act
        var result = await _service.IsEnabledAsync("OLD_FEATURE");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsEnabledAsync_UnknownFlag_ReturnsFalse()
    {
        // Act
        var result = await _service.IsEnabledAsync("NONEXISTENT_FLAG");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsEnabledAsync_MultipleFlags_ReturnsCorrectState()
    {
        // Arrange
        _service.EnableFlag("FEATURE_A");
        _service.EnableFlag("FEATURE_B");
        _service.DisableFlag("FEATURE_C");

        // Act
        var resultA = await _service.IsEnabledAsync("FEATURE_A");
        var resultB = await _service.IsEnabledAsync("FEATURE_B");
        var resultC = await _service.IsEnabledAsync("FEATURE_C");
        var resultD = await _service.IsEnabledAsync("FEATURE_D");

        // Assert
        Assert.True(resultA);
        Assert.True(resultB);
        Assert.False(resultC);
        Assert.False(resultD);
    }

    [Fact]
    public async Task IsEnabledAsync_WithCancellation_ReturnsCancelledTask()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => _service.IsEnabledAsync("FEATURE", cts.Token));
    }

    [Fact]
    public async Task IsEnabledAsync_EmptyFlagName_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.IsEnabledAsync(""));
    }

    [Fact]
    public async Task IsEnabledAsync_NullFlagName_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.IsEnabledAsync(null!));
    }

    [Fact]
    public async Task IsEnabledAsync_WhitespaceFlagName_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.IsEnabledAsync("   "));
    }

    #endregion

    #region GetEnabledFeaturesAsync Tests

    [Fact]
    public async Task GetEnabledFeaturesAsync_NoFlags_ReturnsEmptyList()
    {
        // Act
        var result = await _service.GetEnabledFeaturesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetEnabledFeaturesAsync_SingleEnabledFlag_ReturnsCorrectList()
    {
        // Arrange
        _service.EnableFlag("FEATURE_X");

        // Act
        var result = await _service.GetEnabledFeaturesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains("FEATURE_X", result);
    }

    [Fact]
    public async Task GetEnabledFeaturesAsync_MultipleEnabledFlags_ReturnsAllEnabled()
    {
        // Arrange
        _service.EnableFlag("FEATURE_1");
        _service.EnableFlag("FEATURE_2");
        _service.EnableFlag("FEATURE_3");

        // Act
        var result = await _service.GetEnabledFeaturesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Contains("FEATURE_1", result);
        Assert.Contains("FEATURE_2", result);
        Assert.Contains("FEATURE_3", result);
    }

    [Fact]
    public async Task GetEnabledFeaturesAsync_MixedStates_ReturnsOnlyEnabled()
    {
        // Arrange
        _service.EnableFlag("ENABLED_1");
        _service.EnableFlag("ENABLED_2");
        _service.DisableFlag("DISABLED_1");
        _service.DisableFlag("DISABLED_2");

        // Act
        var result = await _service.GetEnabledFeaturesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains("ENABLED_1", result);
        Assert.Contains("ENABLED_2", result);
        Assert.DoesNotContain("DISABLED_1", result);
        Assert.DoesNotContain("DISABLED_2", result);
    }

    [Fact]
    public async Task GetEnabledFeaturesAsync_WithCancellation_ReturnsCancelledTask()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(
            () => _service.GetEnabledFeaturesAsync(cts.Token));
    }

    [Fact]
    public async Task GetEnabledFeaturesAsync_ReturnsReadOnlyList()
    {
        // Arrange
        _service.EnableFlag("FEATURE");

        // Act
        var result = await _service.GetEnabledFeaturesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IReadOnlyList<string>>(result);
    }

    #endregion

    #region EnableFlag and DisableFlag Tests

    [Fact]
    public async Task EnableFlag_SetsFlag_IsEnabledReturnsTrue()
    {
        // Act
        _service.EnableFlag("NEW_FEATURE");

        // Assert
        var result = await _service.IsEnabledAsync("NEW_FEATURE");
        Assert.True(result);
    }

    [Fact]
    public async Task DisableFlag_ClearsFlag_IsEnabledReturnsFalse()
    {
        // Arrange
        _service.EnableFlag("ACTIVE_FEATURE");

        // Act
        _service.DisableFlag("ACTIVE_FEATURE");

        // Assert
        var result = await _service.IsEnabledAsync("ACTIVE_FEATURE");
        Assert.False(result);
    }

    [Fact]
    public void EnableFlag_EmptyFlagName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.EnableFlag(""));
    }

    [Fact]
    public void EnableFlag_NullFlagName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.EnableFlag(null!));
    }

    [Fact]
    public void DisableFlag_EmptyFlagName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.DisableFlag(""));
    }

    [Fact]
    public void DisableFlag_NullFlagName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.DisableFlag(null!));
    }

    [Fact]
    public async Task EnableFlag_OverwritesPreviousDisabled_ResetsToEnabled()
    {
        // Arrange
        _service.EnableFlag("TOGGLE");
        _service.DisableFlag("TOGGLE");

        // Act
        _service.EnableFlag("TOGGLE");

        // Assert
        var result = await _service.IsEnabledAsync("TOGGLE");
        Assert.True(result);
    }

    #endregion

    #region Concurrent Operations Tests

    [Fact]
    public async Task ConcurrentEnableDisable_MaintainsThreadSafety()
    {
        // Arrange
        const int threadCount = 10;
        const int operationsPerThread = 100;
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < threadCount; i++)
        {
            var threadId = i;
            tasks.Add(Task.Run(async () =>
            {
                for (int j = 0; j < operationsPerThread; j++)
                {
                    string flagName = $"FLAG_{threadId}_{j % 5}"; // 5 unique flags per thread
                    if (j % 2 == 0)
                        _service.EnableFlag(flagName);
                    else
                        _service.DisableFlag(flagName);

                    // Verify state is consistent
                    var isEnabled = await _service.IsEnabledAsync(flagName);
                    var enabled = await _service.GetEnabledFeaturesAsync();
                    Assert.NotNull(enabled);
                }
            }));
        }

        // Assert
        await Task.WhenAll(tasks);
        var finalEnabled = await _service.GetEnabledFeaturesAsync();
        Assert.NotNull(finalEnabled);
    }

    [Fact]
    public async Task ConcurrentReadWrite_DoesNotThrow()
    {
        // Arrange
        _service.EnableFlag("SHARED_FLAG");
        const int readerCount = 5;
        const int writerCount = 5;
        const int operationsPerTask = 50;
        var tasks = new List<Task>();

        // Act - Writers
        for (int i = 0; i < writerCount; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < operationsPerTask; j++)
                {
                    if (j % 2 == 0)
                        _service.EnableFlag("SHARED_FLAG");
                    else
                        _service.DisableFlag("SHARED_FLAG");
                }
            }));
        }

        // Act - Readers
        for (int i = 0; i < readerCount; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                for (int j = 0; j < operationsPerTask; j++)
                {
                    await _service.IsEnabledAsync("SHARED_FLAG");
                    await _service.GetEnabledFeaturesAsync();
                }
            }));
        }

        // Assert
        await Task.WhenAll(tasks);
        Assert.NotNull(await _service.GetEnabledFeaturesAsync());
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task CompleteWorkflow_EnableDisableQuery_Works()
    {
        // Arrange & Act
        _service.EnableFlag("BETA_UI");
        _service.EnableFlag("ADVANCED_SEARCH");
        _service.DisableFlag("LEGACY_API");

        var betaEnabled = await _service.IsEnabledAsync("BETA_UI");
        var searchEnabled = await _service.IsEnabledAsync("ADVANCED_SEARCH");
        var legacyEnabled = await _service.IsEnabledAsync("LEGACY_API");
        var unknownEnabled = await _service.IsEnabledAsync("UNKNOWN");
        var allEnabled = await _service.GetEnabledFeaturesAsync();

        // Assert
        Assert.True(betaEnabled);
        Assert.True(searchEnabled);
        Assert.False(legacyEnabled);
        Assert.False(unknownEnabled);
        Assert.Equal(2, allEnabled.Count);
        Assert.Contains("BETA_UI", allEnabled);
        Assert.Contains("ADVANCED_SEARCH", allEnabled);
    }

    [Fact]
    public async Task FlagNameCase_IsCaseSensitive()
    {
        // Arrange
        _service.EnableFlag("Feature");

        // Act
        var exact = await _service.IsEnabledAsync("Feature");
        var lower = await _service.IsEnabledAsync("feature");
        var upper = await _service.IsEnabledAsync("FEATURE");

        // Assert
        Assert.True(exact);
        Assert.False(lower);
        Assert.False(upper);
    }

    #endregion
}

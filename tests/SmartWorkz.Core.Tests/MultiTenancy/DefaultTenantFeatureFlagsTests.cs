using SmartWorkz.Core.Shared.MultiTenancy;

namespace SmartWorkz.Core.Tests.MultiTenancy;

/// <summary>
/// Tests for DefaultTenantFeatureFlags - tenant-scoped feature flag provider.
/// </summary>
public class DefaultTenantFeatureFlagsTests : IDisposable
{
    private readonly DefaultTenantFeatureFlags _featureFlags;

    public DefaultTenantFeatureFlagsTests()
    {
        _featureFlags = new DefaultTenantFeatureFlags();
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
        var tenantId = "tenant-1";
        var flagName = "PAYMENTS";
        _featureFlags.EnableFlag(tenantId, flagName);

        // Act
        var result = await _featureFlags.IsEnabledAsync(tenantId, flagName);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsEnabledAsync_DisabledFlag_ReturnsFalse()
    {
        // Arrange
        var tenantId = "tenant-1";
        var flagName = "ADVANCED_ANALYTICS";

        // Act
        var result = await _featureFlags.IsEnabledAsync(tenantId, flagName);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsEnabledAsync_UnknownTenant_ReturnsFalse()
    {
        // Arrange
        var tenantId = "unknown-tenant";
        var flagName = "ANY_FLAG";

        // Act
        var result = await _featureFlags.IsEnabledAsync(tenantId, flagName);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsEnabledAsync_WithCancellationToken_Succeeds()
    {
        // Arrange
        var tenantId = "tenant-1";
        var flagName = "TEST_FLAG";
        _featureFlags.EnableFlag(tenantId, flagName);
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _featureFlags.IsEnabledAsync(tenantId, flagName, cts.Token);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region GetEnabledFeaturesAsync Tests

    [Fact]
    public async Task GetEnabledFeaturesAsync_ReturnsCorrectSet()
    {
        // Arrange
        var tenantId = "tenant-1";
        var flags = new[] { "PAYMENTS", "ADVANCED_ANALYTICS", "API_V2" };
        foreach (var flag in flags)
        {
            _featureFlags.EnableFlag(tenantId, flag);
        }

        // Act
        var result = await _featureFlags.GetEnabledFeaturesAsync(tenantId);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains("PAYMENTS", result);
        Assert.Contains("ADVANCED_ANALYTICS", result);
        Assert.Contains("API_V2", result);
    }

    [Fact]
    public async Task GetEnabledFeaturesAsync_UnknownTenant_ReturnsEmptyList()
    {
        // Arrange
        var tenantId = "unknown-tenant";

        // Act
        var result = await _featureFlags.GetEnabledFeaturesAsync(tenantId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetEnabledFeaturesAsync_NoEnabledFlags_ReturnsEmptyList()
    {
        // Arrange
        var tenantId = "tenant-with-no-flags";

        // Act
        var result = await _featureFlags.GetEnabledFeaturesAsync(tenantId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetEnabledFeaturesAsync_WithCancellationToken_Succeeds()
    {
        // Arrange
        var tenantId = "tenant-1";
        _featureFlags.EnableFlag(tenantId, "FLAG1");
        _featureFlags.EnableFlag(tenantId, "FLAG2");
        using var cts = new CancellationTokenSource();

        // Act
        var result = await _featureFlags.GetEnabledFeaturesAsync(tenantId, cts.Token);

        // Assert
        Assert.Equal(2, result.Count);
    }

    #endregion

    #region EnableFlag and DisableFlag Tests

    [Fact]
    public async Task EnableFlag_CreatesNewTenantIfNeeded()
    {
        // Arrange
        var tenantId = "new-tenant";
        var flagName = "NEW_FLAG";

        // Act
        _featureFlags.EnableFlag(tenantId, flagName);

        // Assert
        var result = await _featureFlags.IsEnabledAsync(tenantId, flagName);
        Assert.True(result);
    }

    [Fact]
    public async Task EnableFlag_IsIdempotent()
    {
        // Arrange
        var tenantId = "tenant-1";
        var flagName = "IDEMPOTENT_FLAG";

        // Act
        _featureFlags.EnableFlag(tenantId, flagName);
        _featureFlags.EnableFlag(tenantId, flagName);
        _featureFlags.EnableFlag(tenantId, flagName);

        // Assert
        var result = await _featureFlags.IsEnabledAsync(tenantId, flagName);
        Assert.True(result);
    }

    [Fact]
    public async Task DisableFlag_RemovesFlag()
    {
        // Arrange
        var tenantId = "tenant-1";
        var flagName = "REMOVABLE_FLAG";
        _featureFlags.EnableFlag(tenantId, flagName);
        var enabledResult = await _featureFlags.IsEnabledAsync(tenantId, flagName);
        Assert.True(enabledResult);

        // Act
        _featureFlags.DisableFlag(tenantId, flagName);

        // Assert
        var disabledResult = await _featureFlags.IsEnabledAsync(tenantId, flagName);
        Assert.False(disabledResult);
    }

    [Fact]
    public async Task DisableFlag_IsIdempotent()
    {
        // Arrange
        var tenantId = "tenant-1";
        var flagName = "DISABLE_IDEMPOTENT";
        _featureFlags.EnableFlag(tenantId, flagName);

        // Act
        _featureFlags.DisableFlag(tenantId, flagName);
        _featureFlags.DisableFlag(tenantId, flagName);
        _featureFlags.DisableFlag(tenantId, flagName);

        // Assert
        var result = await _featureFlags.IsEnabledAsync(tenantId, flagName);
        Assert.False(result);
    }

    #endregion

    #region Tenant Isolation Tests

    [Fact]
    public async Task DifferentTenants_HaveIndependentFlagSets()
    {
        // Arrange
        var tenant1 = "tenant-1";
        var tenant2 = "tenant-2";

        _featureFlags.EnableFlag(tenant1, "PAYMENTS");
        _featureFlags.EnableFlag(tenant1, "ANALYTICS");
        _featureFlags.EnableFlag(tenant2, "PAYMENTS");

        // Act
        var tenant1Enabled = await _featureFlags.IsEnabledAsync(tenant1, "ANALYTICS");
        var tenant2Enabled = await _featureFlags.IsEnabledAsync(tenant2, "ANALYTICS");
        var tenant1PaymentsEnabled = await _featureFlags.IsEnabledAsync(tenant1, "PAYMENTS");
        var tenant2PaymentsEnabled = await _featureFlags.IsEnabledAsync(tenant2, "PAYMENTS");

        // Assert
        Assert.True(tenant1Enabled);
        Assert.False(tenant2Enabled);
        Assert.True(tenant1PaymentsEnabled);
        Assert.True(tenant2PaymentsEnabled);
    }

    [Fact]
    public async Task DifferentTenants_GetEnabledFeaturesAsync_ReturnIndependentLists()
    {
        // Arrange
        var tenant1 = "tenant-1";
        var tenant2 = "tenant-2";

        _featureFlags.EnableFlag(tenant1, "FLAG_A");
        _featureFlags.EnableFlag(tenant1, "FLAG_B");
        _featureFlags.EnableFlag(tenant2, "FLAG_A");
        _featureFlags.EnableFlag(tenant2, "FLAG_C");

        // Act
        var tenant1Features = await _featureFlags.GetEnabledFeaturesAsync(tenant1);
        var tenant2Features = await _featureFlags.GetEnabledFeaturesAsync(tenant2);

        // Assert
        Assert.Equal(2, tenant1Features.Count);
        Assert.Contains("FLAG_A", tenant1Features);
        Assert.Contains("FLAG_B", tenant1Features);

        Assert.Equal(2, tenant2Features.Count);
        Assert.Contains("FLAG_A", tenant2Features);
        Assert.Contains("FLAG_C", tenant2Features);
    }

    #endregion

    #region Concurrent Operations Tests

    [Fact]
    public async Task ConcurrentFlagUpdates_DontCorruptState()
    {
        // Arrange
        var tenantId = "concurrent-tenant";
        var flags = new[] { "FLAG1", "FLAG2", "FLAG3", "FLAG4", "FLAG5" };
        var tasks = new List<Task>();

        // Act
        foreach (var flag in flags)
        {
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() => _featureFlags.EnableFlag(tenantId, flag)));
            }
        }

        await Task.WhenAll(tasks);

        // Assert
        var enabledFeatures = await _featureFlags.GetEnabledFeaturesAsync(tenantId);
        Assert.Equal(5, enabledFeatures.Count);
        foreach (var flag in flags)
        {
            Assert.Contains(flag, enabledFeatures);
        }
    }

    [Fact]
    public async Task ConcurrentMixedOperations_MaintainCorrectState()
    {
        // Arrange
        var tenantId = "mixed-concurrent";
        _featureFlags.EnableFlag(tenantId, "FLAG1");
        _featureFlags.EnableFlag(tenantId, "FLAG2");

        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(() => _featureFlags.EnableFlag(tenantId, "FLAG1")));
            tasks.Add(Task.Run(() => _featureFlags.DisableFlag(tenantId, "FLAG2")));
            tasks.Add(Task.Run(async () => await _featureFlags.IsEnabledAsync(tenantId, "FLAG1")));
        }

        await Task.WhenAll(tasks);

        // Assert
        var result1 = await _featureFlags.IsEnabledAsync(tenantId, "FLAG1");
        var result2 = await _featureFlags.IsEnabledAsync(tenantId, "FLAG2");
        Assert.True(result1);
        Assert.False(result2);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task EmptyTenantId_Succeeds()
    {
        // Arrange
        var tenantId = "";
        var flagName = "FLAG";

        // Act
        _featureFlags.EnableFlag(tenantId, flagName);

        // Assert
        var result = await _featureFlags.IsEnabledAsync(tenantId, flagName);
        Assert.True(result);
    }

    [Fact]
    public async Task EmptyFlagName_Succeeds()
    {
        // Arrange
        var tenantId = "tenant-1";
        var flagName = "";

        // Act
        _featureFlags.EnableFlag(tenantId, flagName);

        // Assert
        var result = await _featureFlags.IsEnabledAsync(tenantId, flagName);
        Assert.True(result);
    }

    [Fact]
    public async Task SpecialCharactersInFlagName_Succeeds()
    {
        // Arrange
        var tenantId = "tenant-1";
        var flagName = "FLAG-WITH_SPECIAL.CHARS@#$";

        // Act
        _featureFlags.EnableFlag(tenantId, flagName);
        var result = await _featureFlags.IsEnabledAsync(tenantId, flagName);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ManyFlagsPerTenant_Succeeds()
    {
        // Arrange
        var tenantId = "tenant-with-many-flags";
        var flagCount = 1000;

        // Act
        for (int i = 0; i < flagCount; i++)
        {
            _featureFlags.EnableFlag(tenantId, $"FLAG_{i}");
        }

        var features = await _featureFlags.GetEnabledFeaturesAsync(tenantId);

        // Assert
        Assert.Equal(flagCount, features.Count);
    }

    #endregion
}

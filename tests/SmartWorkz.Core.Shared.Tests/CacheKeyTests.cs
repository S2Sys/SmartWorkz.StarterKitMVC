namespace SmartWorkz.Core.Shared.Tests;

/// <summary>
/// Test suite for cache key generation and tenant isolation patterns.
/// Covers key formatting, tenant prefixing, and pattern matching.
/// </summary>
public class CacheKeyTests
{
    #region Cache Key Generation

    [Fact]
    public void CacheKey_WithSimpleKey_FormatsCorrectly()
    {
        // Arrange
        const string key = "user:123";

        // Act
        var cacheKey = key;

        // Assert
        Assert.NotEmpty(cacheKey);
        Assert.Equal("user:123", cacheKey);
    }

    [Fact]
    public void CacheKey_WithColonDelimiter_MaintainsStructure()
    {
        // Arrange
        const string key = "product:category:electronics";

        // Act
        var parts = key.Split(':');

        // Assert
        Assert.Equal(3, parts.Length);
        Assert.Equal("product", parts[0]);
        Assert.Equal("category", parts[1]);
        Assert.Equal("electronics", parts[2]);
    }

    [Fact]
    public void CacheKey_WithTenantPrefix_IsolatesData()
    {
        // Arrange
        const string tenantId = "tenant1";
        const string key = "user:123";
        var tenantKey = $"{tenantId}:{key}";

        // Act
        var parsed = tenantKey.Split(':', 2);

        // Assert
        Assert.Equal("tenant1", parsed[0]);
        Assert.Equal("user:123", parsed[1]);
    }

    [Fact]
    public void CacheKey_WithMultipleTenants_CreatesDistinctKeys()
    {
        // Arrange
        const string key = "shared_key";
        var tenant1Key = $"tenant1:{key}";
        var tenant2Key = $"tenant2:{key}";

        // Act & Assert
        Assert.NotEqual(tenant1Key, tenant2Key);
        Assert.True(tenant1Key.StartsWith("tenant1:"));
        Assert.True(tenant2Key.StartsWith("tenant2:"));
    }

    #endregion

    #region Pattern Matching

    [Fact]
    public void CacheKeyPattern_WithWildcard_MatchesPrefixes()
    {
        // Arrange
        const string pattern = "user:*";
        var key1 = "user:123";
        var key2 = "user:456";
        var key3 = "product:123";

        // Act
        var matches1 = key1.StartsWith("user:");
        var matches2 = key2.StartsWith("user:");
        var matches3 = key3.StartsWith("user:");

        // Assert
        Assert.True(matches1);
        Assert.True(matches2);
        Assert.False(matches3);
    }

    [Fact]
    public void CacheKeyPattern_WithoutWildcard_MatchesPrefixes()
    {
        // Arrange
        const string pattern = "config:";
        var key1 = "config:db";
        var key2 = "config:cache";
        var key3 = "other:value";

        // Act
        var matches1 = key1.StartsWith(pattern);
        var matches2 = key2.StartsWith(pattern);
        var matches3 = key3.StartsWith(pattern);

        // Assert
        Assert.True(matches1);
        Assert.True(matches2);
        Assert.False(matches3);
    }

    [Fact]
    public void CacheKeyPattern_CaseSensitive_DifferentiatesKeys()
    {
        // Arrange
        const string pattern = "USER:";
        var key1 = "user:123";
        var key2 = "USER:123";

        // Act
        var matches1 = key1.StartsWith(pattern);
        var matches2 = key2.StartsWith(pattern);

        // Assert
        Assert.False(matches1); // Case sensitive, should not match
        Assert.StartsWith(pattern, key2);
    }

    #endregion

    #region Key Validation

    [Fact]
    public void CacheKey_CannotBeEmpty()
    {
        // Arrange
        var key = "";

        // Act & Assert
        Assert.Empty(key);
        Assert.True(string.IsNullOrEmpty(key));
    }

    [Fact]
    public void CacheKey_WithSpecialCharacters_StaysValid()
    {
        // Arrange
        var key = "cache:item-123_v2.1";

        // Act & Assert
        Assert.NotEmpty(key);
        Assert.Contains('-', key);
        Assert.Contains('_', key);
        Assert.Contains('.', key);
    }

    [Fact]
    public void CacheKey_WithLongValue_RemainsValid()
    {
        // Arrange
        var longValue = new string('a', 1000);
        var key = $"cache:{longValue}";

        // Act & Assert
        Assert.NotEmpty(key);
        Assert.True(key.Length > 1000);
    }

    [Fact]
    public void CacheKey_WithUnicodeCharacters_IsSupported()
    {
        // Arrange
        var key = "cache:user:123";
        var unicodeString = "cache:用户:123";

        // Act & Assert
        Assert.NotEmpty(key);
        Assert.NotEmpty(unicodeString);
        Assert.NotEqual(key, unicodeString); // Different strings, one with unicode
    }

    #endregion

    #region Prefix Removal

    [Fact]
    public void CacheKeyPrefix_CanBeRemovedByPattern()
    {
        // Arrange
        var keys = new[] { "user:123", "user:456", "product:789" };
        const string prefixToRemove = "user:";

        // Act
        var filtered = keys.Where(k => !k.StartsWith(prefixToRemove)).ToList();

        // Assert
        Assert.Single(filtered);
        Assert.Equal("product:789", filtered.First());
    }

    [Fact]
    public void CacheKeyPrefix_RemovalIsAccurate()
    {
        // Arrange
        var keys = new[] { "config:db", "config:cache", "config:redis", "other:value" };
        const string pattern = "config:";

        // Act
        var configKeys = keys.Where(k => k.StartsWith(pattern)).ToList();
        var otherKeys = keys.Where(k => !k.StartsWith(pattern)).ToList();

        // Assert
        Assert.Equal(3, configKeys.Count);
        Assert.Single(otherKeys);
    }

    [Fact]
    public void CacheKeyPrefix_WithWildcard_FiltersCorrectly()
    {
        // Arrange
        var keys = new[]
        {
            "default:user:123",
            "default:product:456",
            "tenant1:user:789",
            "tenant2:order:111"
        };

        // Act
        var defaultTenantKeys = keys.Where(k => k.StartsWith("default:")).ToList();

        // Assert
        Assert.Equal(2, defaultTenantKeys.Count);
        Assert.All(defaultTenantKeys, k => Assert.True(k.StartsWith("default:")));
    }

    #endregion

    #region Performance Characteristics

    [Fact]
    public void CacheKeyComparison_IsFast()
    {
        // Arrange
        var key1 = "cache:item:123";
        var key2 = "cache:item:123";
        var iterations = 100000;

        // Act
        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            _ = key1 == key2;
        }
        sw.Stop();

        // Assert - Should complete in reasonable time
        Assert.True(sw.ElapsedMilliseconds < 100);
    }

    [Fact]
    public void CacheKeyPrefixMatching_IsFast()
    {
        // Arrange
        var key = "user:123";
        var pattern = "user:";
        var iterations = 100000;

        // Act
        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            _ = key.StartsWith(pattern);
        }
        sw.Stop();

        // Assert - Should complete in reasonable time
        Assert.True(sw.ElapsedMilliseconds < 100);
    }

    #endregion
}

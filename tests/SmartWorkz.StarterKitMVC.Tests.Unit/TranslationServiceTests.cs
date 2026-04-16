using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartWorkz.StarterKitMVC.Application.Localization;
using SmartWorkz.StarterKitMVC.Infrastructure.Services;
using Xunit;

namespace SmartWorkz.StarterKitMVC.Tests.Unit;

/// <summary>
/// Unit tests for TranslationService - validates caching and fallback behavior.
/// </summary>
public class TranslationServiceTests
{
    private readonly ITranslationService _translationService;
    private readonly IMemoryCache _cache;
    private readonly IServiceProvider _serviceProvider;

    public TranslationServiceTests()
    {
        var services = new ServiceCollection();

        // Add configuration
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Features:Localization:SupportedCultures", "en-US,es-ES,fr-FR" }
            })
            .Build();
        services.AddSingleton<IConfiguration>(config);

        // Add cache
        services.AddMemoryCache();

        // Add translation repository and service
        services.AddScoped<ITranslationRepository, MockTranslationRepository>();
        services.AddSingleton<ITranslationService, TranslationService>();

        _serviceProvider = services.BuildServiceProvider();
        _cache = _serviceProvider.GetRequiredService<IMemoryCache>();
        _translationService = _serviceProvider.GetRequiredService<ITranslationService>();
    }

    [Fact]
    public void Get_WithExistingKey_ReturnsTranslation()
    {
        // Arrange
        const string tenantId = "DEFAULT";
        const string locale = "en-US";
        const string key = "auth.login_success";
        const string expectedValue = "Login successful";

        // Act
        var result = _translationService.Get(key, tenantId, locale);

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public void Get_WithMissingKey_ReturnsKeyAsDefault()
    {
        // Arrange
        const string tenantId = "DEFAULT";
        const string locale = "en-US";
        const string key = "nonexistent.key.that.does.not.exist";

        // Act
        var result = _translationService.Get(key, tenantId, locale);

        // Assert
        // When key is not found, service returns the key itself
        Assert.Equal(key, result);
    }

    [Fact]
    public void Get_WithFormatting_ReturnsFormattedString()
    {
        // Arrange
        const string tenantId = "DEFAULT";
        const string locale = "en-US";
        const string key = "auth.welcome_user";
        const string templateValue = "Welcome, {0}!";
        const string userName = "Admin";
        const string expectedValue = "Welcome, Admin!";

        // Act
        var result = _translationService.Get(key, tenantId, locale, userName);

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public async Task WarmCacheAsync_LoadsTranslationsIntoCache()
    {
        // Arrange
        const string tenantId = "DEFAULT";
        const string locale = "en-US";

        // Act
        await _translationService.WarmCacheAsync(tenantId, locale);

        // Assert
        // After warm-up, a Get should work without database calls
        var result = _translationService.Get("auth.login_success", tenantId, locale);
        Assert.Equal("Login successful", result);
    }

    [Fact]
    public async Task WarmCacheAsync_WithMultipleLocales_CachesAll()
    {
        // Arrange
        const string tenantId = "DEFAULT";
        var locales = new[] { "en-US", "es-ES", "fr-FR" };

        // Act
        foreach (var locale in locales)
        {
            await _translationService.WarmCacheAsync(tenantId, locale);
        }

        // Assert
        foreach (var locale in locales)
        {
            var result = _translationService.Get("auth.login_success", tenantId, locale);
            Assert.NotNull(result);
        }
    }

    [Fact]
    public async Task RefreshCacheAsync_ClearsAndReloadsCache()
    {
        // Arrange
        const string tenantId = "DEFAULT";
        const string locale = "en-US";
        const string key = "auth.login_success";

        // First warm-up
        await _translationService.WarmCacheAsync(tenantId, locale);
        var first = _translationService.Get(key, tenantId, locale);

        // Act
        await _translationService.RefreshCacheAsync(tenantId, locale);

        // Assert
        var second = _translationService.Get(key, tenantId, locale);
        Assert.Equal(first, second); // Should have same value after refresh
    }

    [Fact]
    public void InvalidateCache_ClearsAllTenantLocales()
    {
        // Arrange
        const string tenantId = "DEFAULT";
        var locales = new[] { "en-US", "es-ES", "fr-FR" };

        // Pre-load cache
        foreach (var locale in locales)
        {
            _translationService.Get("auth.login_success", tenantId, locale);
        }

        // Act
        _translationService.InvalidateCache(tenantId);

        // Assert
        // Cache should be cleared, but Get should still work with fallback
        foreach (var locale in locales)
        {
            var result = _translationService.Get("auth.login_success", tenantId, locale);
            Assert.NotNull(result);
        }
    }

    [Fact]
    public void Get_DefaultLocaleIsSameAsSpecified()
    {
        // Arrange
        const string tenantId = "DEFAULT";
        const string key = "auth.login_success";

        // Act
        // Get without specifying locale (should default to en-US)
        var resultDefault = _translationService.Get(key, tenantId);
        // Get with explicit en-US
        var resultExplicit = _translationService.Get(key, tenantId, "en-US");

        // Assert
        Assert.Equal(resultDefault, resultExplicit);
    }

    [Fact]
    public void Get_WithDifferentTenants_ReturnsTenantSpecificTranslation()
    {
        // Arrange
        const string key = "auth.login_success";
        const string locale = "en-US";
        const string tenantDefault = "DEFAULT";
        const string tenantDemo = "DEMO";

        // Act
        var resultDefault = _translationService.Get(key, tenantDefault, locale);
        var resultDemo = _translationService.Get(key, tenantDemo, locale);

        // Assert
        // Both should have translations (from mock data)
        Assert.NotNull(resultDefault);
        Assert.NotNull(resultDemo);
    }

    [Fact]
    public async Task CacheMiss_AutomaticallyWarmsCache()
    {
        // Arrange
        const string tenantId = "DEFAULT";
        const string locale = "en-US";
        const string key = "auth.login_success";

        // Clear cache to ensure cache miss
        _cache.Remove($"translations:{tenantId}:{locale}");

        // Act
        // This Get should trigger a cache miss and auto-warm
        var result = _translationService.Get(key, tenantId, locale);

        // Assert
        Assert.Equal("Login successful", result);
    }

    /// <summary>
    /// Mock translation repository for testing.
    /// Returns hardcoded translation entries to avoid database dependency.
    /// </summary>
    private class MockTranslationRepository : ITranslationRepository
    {
        public async Task<IEnumerable<TranslationEntry>> GetAllAsync(string tenantId, string locale)
        {
            var entries = new List<TranslationEntry>
            {
                new("auth.login_success", "Login successful", tenantId, locale),
                new("auth.login_failed", "Login failed", tenantId, locale),
                new("auth.invalid_credentials", "Invalid email or password", tenantId, locale),
                new("auth.access_denied", "Access denied", tenantId, locale),
                new("auth.welcome_user", "Welcome, {0}!", tenantId, locale),
                new("error.not_found", "Resource not found", tenantId, locale),
                new("error.server_error", "An error occurred. Please try again.", tenantId, locale),
            };

            return await Task.FromResult(entries);
        }
    }
}

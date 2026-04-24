namespace SmartWorkz.Mobile.Tests.Security;

using System;
using System.Threading.Tasks;
using Xunit;

/// <summary>
/// Unit tests for platform-specific SecureTokenStore implementations.
/// Tests secure token storage across Android and iOS platforms.
/// </summary>
public class SecureTokenStoreTests
{
    private const string TestTokenKey = "test_access_token";
    private const string TestTokenValue = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...test_value";
    private const string AnotherTokenKey = "test_refresh_token";
    private const string AnotherTokenValue = "refresh_token_value_here";

#if __ANDROID__
    private readonly SmartWorkz.Mobile.SecureTokenStore _sut;

    public SecureTokenStoreTests()
    {
        var context = Microsoft.Maui.Controls.MauiApplication.Current;
        if (context != null)
        {
            _sut = new SmartWorkz.Mobile.SecureTokenStore(context);
        }
    }

    [Fact]
    public async Task SaveTokenAsync_ValidToken_StoresSuccessfully()
    {
        // Arrange
        await _sut.DeleteTokenAsync(TestTokenKey);

        // Act
        var result = await _sut.SaveTokenAsync(TestTokenKey, TestTokenValue);

        // Assert
        Assert.True(result);

        // Cleanup
        await _sut.DeleteTokenAsync(TestTokenKey);
    }

    [Fact]
    public async Task GetTokenAsync_SavedToken_ReturnsOriginalValue()
    {
        // Arrange
        await _sut.SaveTokenAsync(TestTokenKey, TestTokenValue);

        // Act
        var retrieved = await _sut.GetTokenAsync(TestTokenKey);

        // Assert
        Assert.Equal(TestTokenValue, retrieved);

        // Cleanup
        await _sut.DeleteTokenAsync(TestTokenKey);
    }

    [Fact]
    public async Task GetTokenAsync_NonExistentToken_ReturnsNull()
    {
        // Arrange
        var nonExistentKey = "nonexistent_" + Guid.NewGuid().ToString();

        // Act
        var retrieved = await _sut.GetTokenAsync(nonExistentKey);

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task DeleteTokenAsync_ExistingToken_Removes()
    {
        // Arrange
        await _sut.SaveTokenAsync(TestTokenKey, TestTokenValue);

        // Act
        var deleted = await _sut.DeleteTokenAsync(TestTokenKey);

        // Assert
        Assert.True(deleted);
        var retrieved = await _sut.GetTokenAsync(TestTokenKey);
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task TokenExistsAsync_SavedToken_ReturnsTrue()
    {
        // Arrange
        await _sut.SaveTokenAsync(TestTokenKey, TestTokenValue);

        // Act
        var exists = await _sut.TokenExistsAsync(TestTokenKey);

        // Assert
        Assert.True(exists);

        // Cleanup
        await _sut.DeleteTokenAsync(TestTokenKey);
    }

    [Fact]
    public async Task TokenExistsAsync_NonExistentToken_ReturnsFalse()
    {
        // Arrange
        var nonExistentKey = "nonexistent_" + Guid.NewGuid().ToString();

        // Act
        var exists = await _sut.TokenExistsAsync(nonExistentKey);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task ClearAllTokensAsync_MultipleTokens_RemovesAll()
    {
        // Arrange
        await _sut.SaveTokenAsync(TestTokenKey, TestTokenValue);
        await _sut.SaveTokenAsync(AnotherTokenKey, AnotherTokenValue);

        // Act
        var cleared = await _sut.ClearAllTokensAsync();

        // Assert
        Assert.True(cleared);
        var exists1 = await _sut.TokenExistsAsync(TestTokenKey);
        var exists2 = await _sut.TokenExistsAsync(AnotherTokenKey);
        Assert.False(exists1);
        Assert.False(exists2);
    }
#else
    /// <summary>
    /// Placeholder test for non-Android platforms.
    /// SecureTokenStore tests are only available on Android platform.
    /// </summary>
    [Fact(Skip = "SecureTokenStore tests are only available on Android platform")]
    public void Placeholder_NonAndroidPlatform()
    {
        // This test is skipped on non-Android platforms
    }
#endif
}

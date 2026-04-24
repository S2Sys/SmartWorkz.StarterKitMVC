#if __IOS__
namespace SmartWorkz.Mobile.Tests.iOS;

using Xunit;
using SmartWorkz.Shared.Security.TokenStorage;

/// <summary>
/// Tests for iOS Keychain-based secure token storage.
/// Verifies token save, retrieve, delete, and clear operations.
/// </summary>
public class KeychainTokenStoreTests
{
    private readonly ISecureTokenStore _sut;
    private const string TestTokenKey = "test_access_token";
    private const string TestTokenValue = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";

    public KeychainTokenStoreTests()
    {
        _sut = new KeychainTokenStore();
        // Clear test tokens before test
        _ = _sut.DeleteTokenAsync(TestTokenKey).Result;
    }

    [Fact]
    public async Task SaveToken_ValidToken_StoresSuccessfully()
    {
        // Act
        var result = await _sut.SaveTokenAsync(TestTokenKey, TestTokenValue);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetToken_SavedToken_ReturnsOriginalValue()
    {
        // Arrange
        await _sut.SaveTokenAsync(TestTokenKey, TestTokenValue);

        // Act
        var retrieved = await _sut.GetTokenAsync(TestTokenKey);

        // Assert
        Assert.Equal(TestTokenValue, retrieved);
    }

    [Fact]
    public async Task GetToken_NonExistentToken_ReturnsNull()
    {
        // Act
        var retrieved = await _sut.GetTokenAsync("nonexistent_key");

        // Assert
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task DeleteToken_ExistingToken_Removes()
    {
        // Arrange
        await _sut.SaveTokenAsync(TestTokenKey, TestTokenValue);

        // Act
        var deleted = await _sut.DeleteTokenAsync(TestTokenKey);
        var retrieved = await _sut.GetTokenAsync(TestTokenKey);

        // Assert
        Assert.True(deleted);
        Assert.Null(retrieved);
    }

    [Fact]
    public async Task TokenExists_SavedToken_ReturnsTrue()
    {
        // Arrange
        await _sut.SaveTokenAsync(TestTokenKey, TestTokenValue);

        // Act
        var exists = await _sut.TokenExistsAsync(TestTokenKey);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task TokenExists_NonExistentToken_ReturnsFalse()
    {
        // Act
        var exists = await _sut.TokenExistsAsync("nonexistent");

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task ClearAllTokens_MultipleTokens_RemovesAll()
    {
        // Arrange
        await _sut.SaveTokenAsync("token1", "value1");
        await _sut.SaveTokenAsync("token2", "value2");

        // Act
        var cleared = await _sut.ClearAllTokensAsync();
        var exists1 = await _sut.TokenExistsAsync("token1");
        var exists2 = await _sut.TokenExistsAsync("token2");

        // Assert
        Assert.True(cleared);
        Assert.False(exists1);
        Assert.False(exists2);
    }
}
#endif

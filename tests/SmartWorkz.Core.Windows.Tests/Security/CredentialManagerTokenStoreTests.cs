using SmartWorkz.Core.Windows.Security;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SmartWorkz.Core.Windows.Tests.Security
{
    /// <summary>
    /// Unit tests for CredentialManagerTokenStore Windows implementation.
    /// Tests secure token storage using Windows Credential Manager with DPAPI encryption.
    /// </summary>
    public class CredentialManagerTokenStoreTests
    {
        private readonly CredentialManagerTokenStore _sut;
        private const string TestTokenKey = "test_access_token";
        private const string TestTokenValue = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...test_token_value";
        private const string AnotherTokenKey = "test_refresh_token";
        private const string AnotherTokenValue = "refresh_token_value_here";

        public CredentialManagerTokenStoreTests()
        {
            _sut = new CredentialManagerTokenStore();
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
        public async Task DeleteTokenAsync_NonExistentToken_ReturnsTrue()
        {
            // Arrange
            var nonExistentKey = "nonexistent_" + Guid.NewGuid().ToString();

            // Act
            var deleted = await _sut.DeleteTokenAsync(nonExistentKey);

            // Assert - should return true since credential not found (error 1168) is acceptable
            Assert.True(deleted);
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
        public async Task ClearAllTokensAsync_MultipleTokens_ReturnsTrue()
        {
            // Arrange
            await _sut.SaveTokenAsync(TestTokenKey, TestTokenValue);
            await _sut.SaveTokenAsync(AnotherTokenKey, AnotherTokenValue);

            // Act - ClearAllTokensAsync is a no-op for security reasons (no enumeration API)
            var cleared = await _sut.ClearAllTokensAsync();

            // Assert - should return true (no-op)
            Assert.True(cleared);

            // Cleanup - manually delete since ClearAllTokensAsync is no-op
            await _sut.DeleteTokenAsync(TestTokenKey);
            await _sut.DeleteTokenAsync(AnotherTokenKey);
        }

        [Fact]
        public async Task SaveAndRetrieveMultipleTokens_StoresAndRetrievesEachIndividually()
        {
            // Arrange
            var token1Key = "token1";
            var token1Value = "value1_" + Guid.NewGuid().ToString();
            var token2Key = "token2";
            var token2Value = "value2_" + Guid.NewGuid().ToString();

            // Clean up before test
            await _sut.DeleteTokenAsync(token1Key);
            await _sut.DeleteTokenAsync(token2Key);

            // Act
            var save1Result = await _sut.SaveTokenAsync(token1Key, token1Value);
            var save2Result = await _sut.SaveTokenAsync(token2Key, token2Value);

            var retrieved1 = await _sut.GetTokenAsync(token1Key);
            var retrieved2 = await _sut.GetTokenAsync(token2Key);

            // Assert
            Assert.True(save1Result);
            Assert.True(save2Result);
            Assert.Equal(token1Value, retrieved1);
            Assert.Equal(token2Value, retrieved2);

            // Cleanup
            await _sut.DeleteTokenAsync(token1Key);
            await _sut.DeleteTokenAsync(token2Key);
        }
    }
}

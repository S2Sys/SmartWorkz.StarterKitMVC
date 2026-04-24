using System;
using System.Threading.Tasks;

namespace SmartWorkz.Shared.Security.TokenStorage;

/// <summary>
/// Platform-agnostic secure token storage abstraction.
/// Implementations use platform-specific secure vaults (Keychain, Android Keystore, Credential Manager).
/// </summary>
public interface ISecureTokenStore
{
    /// <summary>
    /// Stores a token securely in the platform vault.
    /// </summary>
    /// <param name="tokenKey">Identifier for the token (e.g., "access_token", "refresh_token")</param>
    /// <param name="token">Token value to store</param>
    /// <returns>True if successfully stored, false otherwise</returns>
    Task<bool> SaveTokenAsync(string tokenKey, string token);

    /// <summary>
    /// Retrieves a token from the platform vault.
    /// </summary>
    /// <param name="tokenKey">Identifier for the token</param>
    /// <returns>Token value or null if not found</returns>
    Task<string?> GetTokenAsync(string tokenKey);

    /// <summary>
    /// Removes a token from the platform vault.
    /// </summary>
    /// <param name="tokenKey">Identifier for the token</param>
    /// <returns>True if successfully removed or not found, false if error</returns>
    Task<bool> DeleteTokenAsync(string tokenKey);

    /// <summary>
    /// Checks if a token exists in the vault.
    /// </summary>
    /// <param name="tokenKey">Identifier for the token</param>
    /// <returns>True if token exists, false otherwise</returns>
    Task<bool> TokenExistsAsync(string tokenKey);

    /// <summary>
    /// Removes all stored tokens.
    /// </summary>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> ClearAllTokensAsync();
}

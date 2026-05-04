#if __IOS__
namespace SmartWorkz.Mobile;

using Foundation;
using Security;
using SmartWorkz.Shared.Security.TokenStorage;
using System;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// iOS-specific secure token storage using Keychain Services with optional SecureEnclave support.
/// Implements ISecureTokenStore to provide platform-agnostic token storage across all platforms.
/// Tokens are encrypted and stored in the device's secure Keychain with accessibility controls.
/// </summary>
public class KeychainTokenStore : ISecureTokenStore
{
    private const string ServiceName = "com.smartworkz.ios";
    private const string AccessGroup = "com.smartworkz.ios.shared";
    private readonly bool _useSecureEnclave = true;

    /// <summary>
    /// Stores a token securely in iOS Keychain.
    /// Tokens are stored with kSecAttrAccessibleWhenUnlockedThisDeviceOnly protection.
    /// </summary>
    public Task<bool> SaveTokenAsync(string tokenKey, string token)
    {
        return Task.Run(() =>
        {
            try
            {
                var tokenData = Encoding.UTF8.GetBytes(token);
                var query = new NSMutableDictionary
                {
                    { SecItem.Class, SecClass.GenericPassword },
                    { SecItem.AttrService, new NSString(ServiceName) },
                    { SecItem.AttrAccount, new NSString(tokenKey) },
                    { SecItem.ValueData, NSData.FromArray(tokenData) },
                    { SecItem.AttrAccessible, SecAccessible.WhenUnlockedThisDeviceOnly },
                    { SecItem.AttrSynchronizable, new NSNumber(false) }
                };

                // Delete existing token first
                SecKeyChain.Remove(query);

                // Add new token
                var status = SecKeyChain.Add(query);

                return status == SecStatusCode.Success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving token to Keychain: {ex.Message}");
                return false;
            }
        });
    }

    /// <summary>
    /// Retrieves a token from iOS Keychain.
    /// Returns null if token is not found.
    /// </summary>
    public Task<string?> GetTokenAsync(string tokenKey)
    {
        return Task.Run(() =>
        {
            try
            {
                var query = new NSMutableDictionary
                {
                    { SecItem.Class, SecClass.GenericPassword },
                    { SecItem.AttrService, new NSString(ServiceName) },
                    { SecItem.AttrAccount, new NSString(tokenKey) },
                    { SecItem.ReturnData, new NSNumber(true) },
                    { SecItem.MatchLimit, SecMatchLimit.One }
                };

                NSObject? result = null;
                var status = SecKeyChain.QueryAsData(query, out var data);

                if (status == SecStatusCode.Success && data != null)
                {
                    var token = Encoding.UTF8.GetString(data.ToArray());
                    return token;
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving token from Keychain: {ex.Message}");
                return null;
            }
        });
    }

    /// <summary>
    /// Deletes a token from iOS Keychain.
    /// Returns true if successfully deleted or not found.
    /// </summary>
    public Task<bool> DeleteTokenAsync(string tokenKey)
    {
        return Task.Run(() =>
        {
            try
            {
                var query = new NSMutableDictionary
                {
                    { SecItem.Class, SecClass.GenericPassword },
                    { SecItem.AttrService, new NSString(ServiceName) },
                    { SecItem.AttrAccount, new NSString(tokenKey) }
                };

                var status = SecKeyChain.Remove(query);

                return status == SecStatusCode.Success || status == SecStatusCode.ItemNotFound;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting token from Keychain: {ex.Message}");
                return false;
            }
        });
    }

    /// <summary>
    /// Checks if a token exists in iOS Keychain.
    /// </summary>
    public Task<bool> TokenExistsAsync(string tokenKey)
    {
        return Task.Run(() =>
        {
            try
            {
                var query = new NSMutableDictionary
                {
                    { SecItem.Class, SecClass.GenericPassword },
                    { SecItem.AttrService, new NSString(ServiceName) },
                    { SecItem.AttrAccount, new NSString(tokenKey) },
                    { SecItem.ReturnData, new NSNumber(false) },
                    { SecItem.MatchLimit, SecMatchLimit.One }
                };

                var status = SecKeyChain.QueryAsRecord(query, out _);

                return status == SecStatusCode.Success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking token existence in Keychain: {ex.Message}");
                return false;
            }
        });
    }

    /// <summary>
    /// Removes all tokens stored by this application in iOS Keychain.
    /// </summary>
    public Task<bool> ClearAllTokensAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                var query = new NSMutableDictionary
                {
                    { SecItem.Class, SecClass.GenericPassword },
                    { SecItem.AttrService, new NSString(ServiceName) }
                };

                var status = SecKeyChain.Remove(query);

                return status == SecStatusCode.Success || status == SecStatusCode.ItemNotFound;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing all tokens from Keychain: {ex.Message}");
                return false;
            }
        });
    }
}
#endif

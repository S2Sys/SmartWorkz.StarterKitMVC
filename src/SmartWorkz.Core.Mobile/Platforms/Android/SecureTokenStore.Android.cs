#if __ANDROID__
namespace SmartWorkz.Mobile;

using Android.Content;
using AndroidX.Security.Crypto;
using SmartWorkz.Shared.Security.TokenStorage;
using System;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Android-specific secure token storage using EncryptedSharedPreferences with AES-256-GCM encryption.
/// Implements ISecureTokenStore to provide platform-agnostic token storage across all platforms.
/// Tokens are encrypted using Android's built-in Keystore and stored in SharedPreferences.
/// </summary>
public class SecureTokenStore : ISecureTokenStore
{
    private const string PreferencesFileName = "smartworkz_tokens";
    private readonly Context _context;
    private EncryptedSharedPreferences? _encryptedPrefs;

    public SecureTokenStore(Context context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Lazily initializes EncryptedSharedPreferences with MasterKey using AES256_GCM.
    /// </summary>
    private EncryptedSharedPreferences GetEncryptedPreferences()
    {
        if (_encryptedPrefs != null)
            return _encryptedPrefs;

        try
        {
            // Create a MasterKey using AES-256-GCM
            var masterKey = new MasterKey.Builder(_context)
                .SetKeyScheme(MasterKey.KeyScheme.Aes256Gcm)
                .Build();

            // Create EncryptedSharedPreferences with AES256_SIV for keys and AES256_GCM for values
            _encryptedPrefs = EncryptedSharedPreferences.Create(
                _context,
                PreferencesFileName,
                masterKey,
                EncryptedSharedPreferences.PrefKeyEncryptionScheme.Aes256Siv,
                EncryptedSharedPreferences.PrefValueEncryptionScheme.Aes256Gcm
            );

            return _encryptedPrefs;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing EncryptedSharedPreferences: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Stores a token securely in EncryptedSharedPreferences.
    /// </summary>
    public Task<bool> SaveTokenAsync(string tokenKey, string token)
    {
        return Task.Run(() =>
        {
            try
            {
                var prefs = GetEncryptedPreferences();
                var editor = prefs.Edit();
                editor.PutString(tokenKey, token);
                var result = editor.Commit();
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving token: {ex.Message}");
                return false;
            }
        });
    }

    /// <summary>
    /// Retrieves a token from EncryptedSharedPreferences.
    /// Returns null if token is not found.
    /// </summary>
    public Task<string?> GetTokenAsync(string tokenKey)
    {
        return Task.Run(() =>
        {
            try
            {
                var prefs = GetEncryptedPreferences();
                var token = prefs.GetString(tokenKey, null);
                return token;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error retrieving token: {ex.Message}");
                return null;
            }
        });
    }

    /// <summary>
    /// Removes a token from EncryptedSharedPreferences.
    /// </summary>
    public Task<bool> DeleteTokenAsync(string tokenKey)
    {
        return Task.Run(() =>
        {
            try
            {
                var prefs = GetEncryptedPreferences();
                var editor = prefs.Edit();
                editor.Remove(tokenKey);
                var result = editor.Commit();
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting token: {ex.Message}");
                return false;
            }
        });
    }

    /// <summary>
    /// Checks if a token exists in EncryptedSharedPreferences.
    /// </summary>
    public Task<bool> TokenExistsAsync(string tokenKey)
    {
        return Task.Run(() =>
        {
            try
            {
                var prefs = GetEncryptedPreferences();
                return prefs.Contains(tokenKey);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error checking token existence: {ex.Message}");
                return false;
            }
        });
    }

    /// <summary>
    /// Removes all tokens stored in EncryptedSharedPreferences.
    /// </summary>
    public Task<bool> ClearAllTokensAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                var prefs = GetEncryptedPreferences();
                var editor = prefs.Edit();
                editor.Clear();
                var result = editor.Commit();
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error clearing all tokens: {ex.Message}");
                return false;
            }
        });
    }
}
#endif

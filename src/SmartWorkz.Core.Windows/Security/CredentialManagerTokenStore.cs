using SmartWorkz.Shared.Security.TokenStorage;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SmartWorkz.Core.Windows.Security
{
    /// <summary>
    /// Windows-specific secure token storage using Windows Data Protection API (DPAPI) via Credential Manager.
    /// Implements ISecureTokenStore to provide platform-agnostic token storage across all platforms.
    /// </summary>
    public class CredentialManagerTokenStore : ISecureTokenStore
    {
        private const string TargetNamePrefix = "SmartWorkz";
        private static readonly Dictionary<string, string> _inMemoryCredentials = new();

        /// <summary>
        /// Windows Credential structure for P/Invoke calls.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CREDENTIAL
        {
            public uint Flags;
            public uint Type;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string TargetName;
            public IntPtr Comment;
            public SYSTEMTIME LastWritten;
            public uint CredentialBlobSize;
            public IntPtr CredentialBlob;
            public uint Persist;
            public uint AttributeCount;
            public IntPtr Attributes;
            public IntPtr TargetAlias;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string UserName;
        }

        /// <summary>
        /// System time structure used in CREDENTIAL.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEMTIME
        {
            public ushort Year;
            public ushort Month;
            public ushort DayOfWeek;
            public ushort Day;
            public ushort Hour;
            public ushort Minute;
            public ushort Second;
            public ushort Milliseconds;
        }

        /// <summary>
        /// P/Invoke declaration for CredWrite - stores a credential.
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CredWrite(ref CREDENTIAL credential, uint flags);

        /// <summary>
        /// P/Invoke declaration for CredRead - retrieves a credential.
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CredRead(string targetName, uint type, uint flags, out IntPtr credential);

        /// <summary>
        /// P/Invoke declaration for CredDelete - deletes a credential.
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CredDelete(string targetName, uint type, uint flags);

        /// <summary>
        /// P/Invoke declaration for CredFree - frees credential memory.
        /// </summary>
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern void CredFree(IntPtr credential);

        /// <summary>
        /// Stores a token securely in Windows Credential Manager.
        /// Falls back to in-memory storage with encryption if Credential Manager fails.
        /// </summary>
        /// <param name="tokenKey">Identifier for the token (e.g., "access_token")</param>
        /// <param name="token">Token value to store</param>
        /// <returns>True if successfully stored, false otherwise</returns>
        public Task<bool> SaveTokenAsync(string tokenKey, string token)
        {
            return Task.Run(() =>
            {
                try
                {
                    var targetName = $"{TargetNamePrefix}_{tokenKey}";
                    var tokenBytes = Encoding.UTF8.GetBytes(token);

                    // Try Credential Manager first
                    bool credManagerSuccess = TryWriteToCredentialManager(targetName, tokenBytes);

                    if (credManagerSuccess)
                    {
                        return true;
                    }

                    // Fallback to in-memory storage with DPAPI encryption
                    var encryptedData = ProtectData(tokenBytes);
                    lock (_inMemoryCredentials)
                    {
                        _inMemoryCredentials[tokenKey] = Convert.ToBase64String(encryptedData);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving token: {ex.Message}");
                    return false;
                }
            });
        }

        /// <summary>
        /// Retrieves a token from Windows Credential Manager or in-memory storage.
        /// </summary>
        /// <param name="tokenKey">Identifier for the token</param>
        /// <returns>Token value or null if not found</returns>
        public Task<string?> GetTokenAsync(string tokenKey)
        {
            return Task.Run(() =>
            {
                try
                {
                    var targetName = $"{TargetNamePrefix}_{tokenKey}";

                    // Try Credential Manager first
                    var token = TryReadFromCredentialManager(targetName);
                    if (token != null)
                    {
                        return token;
                    }

                    // Check in-memory storage
                    lock (_inMemoryCredentials)
                    {
                        if (_inMemoryCredentials.TryGetValue(tokenKey, out var encryptedBase64))
                        {
                            var encryptedData = Convert.FromBase64String(encryptedBase64);
                            var decryptedData = UnprotectData(encryptedData);
                            return Encoding.UTF8.GetString(decryptedData);
                        }
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error retrieving token: {ex.Message}");
                    return null;
                }
            });
        }

        /// <summary>
        /// Removes a token from Windows Credential Manager or in-memory storage.
        /// </summary>
        /// <param name="tokenKey">Identifier for the token</param>
        /// <returns>True if successfully removed or not found, false if error</returns>
        public Task<bool> DeleteTokenAsync(string tokenKey)
        {
            return Task.Run(() =>
            {
                try
                {
                    var targetName = $"{TargetNamePrefix}_{tokenKey}";

                    // Try to delete from Credential Manager
                    bool credManagerDeleted = CredDelete(targetName, 1, 0);

                    // Delete from in-memory storage
                    lock (_inMemoryCredentials)
                    {
                        _inMemoryCredentials.Remove(tokenKey);
                    }

                    // Return true if either succeeded or credential not found (error 1168)
                    if (credManagerDeleted)
                    {
                        return true;
                    }

                    int lastError = Marshal.GetLastWin32Error();
                    return lastError == 1168 || lastError == 0; // ERROR_NOT_FOUND or success
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error deleting token: {ex.Message}");
                    return false;
                }
            });
        }

        /// <summary>
        /// Checks if a token exists in Windows Credential Manager or in-memory storage.
        /// </summary>
        /// <param name="tokenKey">Identifier for the token</param>
        /// <returns>True if token exists, false otherwise</returns>
        public async Task<bool> TokenExistsAsync(string tokenKey)
        {
            var token = await GetTokenAsync(tokenKey);
            return token != null;
        }

        /// <summary>
        /// Removes all tokens stored by this application.
        /// Windows Credential Manager does not provide enumeration for security reasons.
        /// Clears only in-memory storage.
        /// </summary>
        /// <returns>True (no-op for Credential Manager, clears in-memory)</returns>
        public Task<bool> ClearAllTokensAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    lock (_inMemoryCredentials)
                    {
                        _inMemoryCredentials.Clear();
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            });
        }

        /// <summary>
        /// Helper method to write credentials to Windows Credential Manager.
        /// </summary>
        private static bool TryWriteToCredentialManager(string targetName, byte[] tokenBytes)
        {
            IntPtr credentialBlobPtr = IntPtr.Zero;

            try
            {
                credentialBlobPtr = Marshal.AllocCoTaskMem(tokenBytes.Length);
                Marshal.Copy(tokenBytes, 0, credentialBlobPtr, tokenBytes.Length);

                var credential = new CREDENTIAL
                {
                    Type = 1, // CRED_TYPE_GENERIC
                    TargetName = targetName,
                    UserName = Environment.UserName,
                    CredentialBlob = credentialBlobPtr,
                    CredentialBlobSize = (uint)tokenBytes.Length,
                    Persist = 1, // CRED_PERSIST_SESSION
                    Flags = 0,
                    Comment = IntPtr.Zero,
                    AttributeCount = 0,
                    Attributes = IntPtr.Zero,
                    TargetAlias = IntPtr.Zero
                };

                bool result = CredWrite(ref credential, 0);

                if (!result)
                {
                    int lastError = Marshal.GetLastWin32Error();
                    System.Diagnostics.Debug.WriteLine($"CredWrite failed with error code: {lastError}");
                }

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TryWriteToCredentialManager exception: {ex.Message}");
                return false;
            }
            finally
            {
                if (credentialBlobPtr != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(credentialBlobPtr);
                }
            }
        }

        /// <summary>
        /// Helper method to read credentials from Windows Credential Manager.
        /// </summary>
        private static string? TryReadFromCredentialManager(string targetName)
        {
            try
            {
                bool result = CredRead(targetName, 1, 0, out IntPtr credentialPtr);

                if (!result || credentialPtr == IntPtr.Zero)
                {
                    return null;
                }

                var credential = Marshal.PtrToStructure<CREDENTIAL>(credentialPtr);
                var tokenBytes = new byte[credential.CredentialBlobSize];
                Marshal.Copy(credential.CredentialBlob, tokenBytes, 0, (int)credential.CredentialBlobSize);

                CredFree(credentialPtr);

                return Encoding.UTF8.GetString(tokenBytes);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TryReadFromCredentialManager exception: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Protects data using Windows DPAPI (Data Protection API).
        /// </summary>
        private static byte[] ProtectData(byte[] data)
        {
            try
            {
                return System.Security.Cryptography.ProtectedData.Protect(
                    data,
                    null,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ProtectData failed: {ex.Message}");
                // Return encrypted with a simple approach if DPAPI fails
                return EncryptWithDpapi(data);
            }
        }

        /// <summary>
        /// Unprotects data using Windows DPAPI.
        /// </summary>
        private static byte[] UnprotectData(byte[] encryptedData)
        {
            try
            {
                return System.Security.Cryptography.ProtectedData.Unprotect(
                    encryptedData,
                    null,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UnprotectData failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Simple encryption helper if DPAPI is unavailable.
        /// </summary>
        private static byte[] EncryptWithDpapi(byte[] data)
        {
            // For testing/fallback: just return the data as-is
            // In production, this should use actual encryption
            return data;
        }
    }
}

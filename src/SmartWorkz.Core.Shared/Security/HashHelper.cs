namespace SmartWorkz.Core.Shared.Security;

using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Provides utilities for cryptographic hash operations (SHA256 and MD5).
/// </summary>
public static class HashHelper
{
    /// <summary>
    /// Computes the SHA256 hash of a string and returns it as a hexadecimal string.
    /// </summary>
    public static Result<string> Sha256(string text)
    {
        try
        {
            if (string.IsNullOrEmpty(text))
                return Result.Fail<string>("Error.EmptyText", "Text cannot be null or empty");

            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
            return Result.Ok(Convert.ToHexString(hash));
        }
        catch (Exception ex)
        {
            return Result.Fail<string>("Error.Sha256", ex.Message);
        }
    }

    /// <summary>
    /// Computes the SHA256 hash of a byte array and returns the hash as a byte array.
    /// </summary>
    public static Result<byte[]> Sha256Bytes(byte[] data)
    {
        try
        {
            if (data == null || data.Length == 0)
                return Result.Fail<byte[]>("Error.EmptyData", "Data cannot be null or empty");

            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(data);
            return Result.Ok(hash);
        }
        catch (Exception ex)
        {
            return Result.Fail<byte[]>("Error.Sha256Bytes", ex.Message);
        }
    }

    /// <summary>
    /// Computes the MD5 hash of a string and returns it as a hexadecimal string.
    /// Note: MD5 is cryptographically broken; use SHA256 for security-critical applications.
    /// </summary>
    public static Result<string> Md5(string text)
    {
        try
        {
            if (string.IsNullOrEmpty(text))
                return Result.Fail<string>("Error.EmptyText", "Text cannot be null or empty");

#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(text));
            return Result.Ok(Convert.ToHexString(hash));
#pragma warning restore CA5351
        }
        catch (Exception ex)
        {
            return Result.Fail<string>("Error.Md5", ex.Message);
        }
    }

    /// <summary>
    /// Verifies that a text matches its SHA256 hash.
    /// </summary>
    public static Result<bool> VerifyHash(string text, string hash)
    {
        try
        {
            if (string.IsNullOrEmpty(text))
                return Result.Fail<bool>("Error.EmptyText", "Text cannot be null or empty");

            if (string.IsNullOrEmpty(hash))
                return Result.Fail<bool>("Error.EmptyHash", "Hash cannot be null or empty");

            var computedHashResult = Sha256(text);
            if (!computedHashResult.Succeeded)
                return Result.Fail<bool>(computedHashResult.MessageKey ?? "Error.VerifyHash");

            bool matches = computedHashResult.Data?.Equals(hash, StringComparison.OrdinalIgnoreCase) ?? false;
            return Result.Ok(matches);
        }
        catch (Exception ex)
        {
            return Result.Fail<bool>("Error.VerifyHash", ex.Message);
        }
    }
}

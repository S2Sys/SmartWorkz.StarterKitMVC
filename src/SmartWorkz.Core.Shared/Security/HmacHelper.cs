namespace SmartWorkz.Shared;

using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Provides HMAC-SHA256/SHA512 message signing and verification for API requests and webhook verification.
/// Implements constant-time comparison to prevent timing attacks.
/// </summary>
public sealed class HmacHelper
{
    /// <summary>
    /// Signs a message using HMAC with the specified algorithm and returns a Base64-encoded hex digest.
    /// </summary>
    /// <param name="message">The message to sign.</param>
    /// <param name="secret">The secret key for signing.</param>
    /// <param name="algorithm">The HMAC algorithm to use (default: SHA256).</param>
    /// <returns>A Result containing the Base64-encoded signature or an error.</returns>
    public static Result<string> Sign(
        string message,
        string secret,
        HmacAlgorithm algorithm = HmacAlgorithm.SHA256)
    {
        if (string.IsNullOrEmpty(message))
            return Result.Fail<string>("HmacHelper.InvalidMessage", "Message cannot be null or empty");

        if (string.IsNullOrEmpty(secret))
            return Result.Fail<string>("HmacHelper.InvalidSecret", "Secret cannot be null or empty");

        try
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var secretBytes = Encoding.UTF8.GetBytes(secret);

            var signatureBytes = SignBytes(messageBytes, secretBytes, algorithm);
            var base64Signature = Convert.ToBase64String(signatureBytes);

            return Result.Ok(base64Signature);
        }
        catch (Exception ex)
        {
            return Result.Fail<string>("HmacHelper.SigningFailed", $"Failed to sign message: {ex.Message}");
        }
    }

    /// <summary>
    /// Signs a message using HMAC with the specified algorithm and returns the raw byte digest.
    /// </summary>
    /// <param name="message">The message bytes to sign.</param>
    /// <param name="secret">The secret key for signing.</param>
    /// <param name="algorithm">The HMAC algorithm to use (default: SHA256).</param>
    /// <returns>A Result containing the byte signature or an error.</returns>
    public static Result<byte[]> SignBytes(
        byte[] message,
        string secret,
        HmacAlgorithm algorithm = HmacAlgorithm.SHA256)
    {
        if (message == null || message.Length == 0)
            return Result.Fail<byte[]>("HmacHelper.InvalidMessage", "Message cannot be null or empty");

        if (string.IsNullOrEmpty(secret))
            return Result.Fail<byte[]>("HmacHelper.InvalidSecret", "Secret cannot be null or empty");

        try
        {
            var secretBytes = Encoding.UTF8.GetBytes(secret);
            var signatureBytes = SignBytes(message, secretBytes, algorithm);

            return Result.Ok(signatureBytes);
        }
        catch (Exception ex)
        {
            return Result.Fail<byte[]>("HmacHelper.SigningFailed", $"Failed to sign message: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifies a message signature using HMAC with constant-time comparison to prevent timing attacks.
    /// </summary>
    /// <param name="message">The original message that was signed.</param>
    /// <param name="signature">The Base64-encoded signature to verify.</param>
    /// <param name="secret">The secret key used for signing.</param>
    /// <param name="algorithm">The HMAC algorithm to use (default: SHA256).</param>
    /// <returns>A Result containing true if the signature is valid, false if not, or an error.</returns>
    public static Result<bool> Verify(
        string message,
        string signature,
        string secret,
        HmacAlgorithm algorithm = HmacAlgorithm.SHA256)
    {
        if (string.IsNullOrEmpty(message))
            return Result.Fail<bool>("HmacHelper.InvalidMessage", "Message cannot be null or empty");

        if (string.IsNullOrEmpty(signature))
            return Result.Fail<bool>("HmacHelper.InvalidSignature", "Signature cannot be null or empty");

        if (string.IsNullOrEmpty(secret))
            return Result.Fail<bool>("HmacHelper.InvalidSecret", "Secret cannot be null or empty");

        try
        {
            var signatureBytes = Convert.FromBase64String(signature);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            return VerifyBytes(messageBytes, signatureBytes, secret, algorithm);
        }
        catch (FormatException)
        {
            return Result.Fail<bool>("HmacHelper.InvalidSignatureFormat", "Signature is not a valid Base64 string");
        }
        catch (Exception ex)
        {
            return Result.Fail<bool>("HmacHelper.VerificationFailed", $"Failed to verify signature: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifies a message signature using HMAC with raw byte inputs and constant-time comparison.
    /// </summary>
    /// <param name="message">The original message bytes that were signed.</param>
    /// <param name="signature">The byte signature to verify.</param>
    /// <param name="secret">The secret key used for signing.</param>
    /// <param name="algorithm">The HMAC algorithm to use (default: SHA256).</param>
    /// <returns>A Result containing true if the signature is valid, false if not, or an error.</returns>
    public static Result<bool> VerifyBytes(
        byte[] message,
        byte[] signature,
        string secret,
        HmacAlgorithm algorithm = HmacAlgorithm.SHA256)
    {
        if (message == null || message.Length == 0)
            return Result.Fail<bool>("HmacHelper.InvalidMessage", "Message cannot be null or empty");

        if (signature == null || signature.Length == 0)
            return Result.Fail<bool>("HmacHelper.InvalidSignature", "Signature cannot be null or empty");

        if (string.IsNullOrEmpty(secret))
            return Result.Fail<bool>("HmacHelper.InvalidSecret", "Secret cannot be null or empty");

        try
        {
            var secretBytes = Encoding.UTF8.GetBytes(secret);
            var computedSignature = SignBytes(message, secretBytes, algorithm);

            // Use constant-time comparison to prevent timing attacks
            var isValid = CryptographicOperations.FixedTimeEquals(computedSignature, signature);

            return Result.Ok(isValid);
        }
        catch (Exception ex)
        {
            return Result.Fail<bool>("HmacHelper.VerificationFailed", $"Failed to verify signature: {ex.Message}");
        }
    }

    /// <summary>
    /// Internal method to compute HMAC signature from raw bytes.
    /// </summary>
    private static byte[] SignBytes(byte[] message, byte[] secret, HmacAlgorithm algorithm)
    {
        using var hmac = CreateHmac(secret, algorithm);
        return hmac.ComputeHash(message);
    }

    /// <summary>
    /// Creates the appropriate HMAC instance based on the algorithm.
    /// </summary>
    private static HMAC CreateHmac(byte[] secret, HmacAlgorithm algorithm)
    {
        return algorithm switch
        {
            HmacAlgorithm.SHA256 => new HMACSHA256(secret),
            HmacAlgorithm.SHA512 => new HMACSHA512(secret),
            _ => throw new ArgumentException($"Unsupported HMAC algorithm: {algorithm}", nameof(algorithm))
        };
    }
}

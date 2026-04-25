using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SmartWorkz.Core.Shared.Webhooks;

/// <summary>
/// Handles HMAC-SHA256 signing and verification of webhook payloads.
/// </summary>
public static class WebhookSignature
{
    /// <summary>
    /// Algorithm used for signing: HMAC-SHA256.
    /// </summary>
    public const string Algorithm = "sha256";

    /// <summary>
    /// Sign a webhook payload using the provided secret key.
    /// </summary>
    /// <param name="payload">The webhook payload to sign.</param>
    /// <param name="secretKey">The webhook secret key.</param>
    /// <returns>HMAC-SHA256 signature in hex format.</returns>
    public static string Sign(WebhookPayload payload, string secretKey)
    {
        var json = JsonSerializer.Serialize(payload.Event, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        return Sign(json, secretKey);
    }

    /// <summary>
    /// Sign a JSON string using the provided secret key.
    /// </summary>
    public static string Sign(string json, string secretKey)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(json));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    /// <summary>
    /// Verify a webhook payload signature.
    /// </summary>
    /// <param name="payload">The webhook payload.</param>
    /// <param name="signature">The signature to verify.</param>
    /// <param name="secretKey">The webhook secret key.</param>
    /// <returns>True if signature is valid, false otherwise.</returns>
    public static bool Verify(WebhookPayload payload, string signature, string secretKey)
    {
        var expectedSignature = Sign(payload, secretKey);
        return signature.Equals(expectedSignature, StringComparison.OrdinalIgnoreCase);
    }
}

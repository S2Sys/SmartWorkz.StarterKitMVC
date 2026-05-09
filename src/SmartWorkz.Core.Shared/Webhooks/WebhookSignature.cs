using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SmartWorkz.Core.Shared.Webhooks.Models;

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
    /// Sign a webhook event using the provided secret key.
    /// </summary>
    /// <param name="webhookEvent">The webhook event to sign.</param>
    /// <param name="secretKey">The webhook secret key.</param>
    /// <returns>HMAC-SHA256 signature in hex format.</returns>
    public static string Sign(WebhookEvent webhookEvent, string secretKey)
    {
        var json = JsonSerializer.Serialize(webhookEvent, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        return Sign(json, secretKey);
    }

    /// <summary>
    /// Sign a webhook payload using the provided secret key.
    /// </summary>
    /// <param name="payload">The webhook payload to sign.</param>
    /// <param name="secretKey">The webhook secret key.</param>
    /// <returns>HMAC-SHA256 signature in hex format.</returns>
    public static string Sign(WebhookPayload payload, string secretKey)
    {
        if (payload == null)
            throw new ArgumentNullException(nameof(payload));

        var options = new JsonSerializerOptions { PropertyNamingPolicy = null };
        var json = JsonSerializer.Serialize(payload.Event, options);
        return Sign(json, secretKey);
    }

    /// <summary>
    /// Sign a webhook event using the provided secret key.
    /// </summary>
    public static string Sign(WebhookEvent @event, string secretKey)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));
        if (secretKey == null)
            throw new ArgumentNullException(nameof(secretKey));

        var options = new JsonSerializerOptions { PropertyNamingPolicy = null };
        var json = JsonSerializer.Serialize(@event, options);
        return Sign(json, secretKey);
    }

    /// <summary>
    /// Sign a JSON string using the provided secret key.
    /// </summary>
    public static string Sign(string json, string secretKey)
    {
        if (json == null)
            throw new ArgumentNullException(nameof(json));
        if (secretKey == null)
            throw new ArgumentNullException(nameof(secretKey));

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
        if (payload == null)
            throw new ArgumentNullException(nameof(payload));
        if (signature == null)
            throw new ArgumentNullException(nameof(signature));
        if (secretKey == null)
            throw new ArgumentNullException(nameof(secretKey));

        var expectedSignature = Sign(payload, secretKey);
        return ConstantTimeEquals(signature, expectedSignature);
    }

    /// <summary>
    /// Verify a webhook event signature.
    /// </summary>
    public static bool Verify(WebhookEvent @event, string signature, string secretKey)
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));
        if (signature == null)
            throw new ArgumentNullException(nameof(signature));
        if (secretKey == null)
            throw new ArgumentNullException(nameof(secretKey));

        var expectedSignature = Sign(@event, secretKey);
        return ConstantTimeEquals(signature, expectedSignature);
    }

    private static bool ConstantTimeEquals(string a, string b)
    {
        if (a == null || b == null)
            return a == b;

        if (a.Length != b.Length)
            return false;

        int result = 0;
        for (int i = 0; i < a.Length; i++)
            result |= a[i] ^ b[i];

        return result == 0;
    }
}

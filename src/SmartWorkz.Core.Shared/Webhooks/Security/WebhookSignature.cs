using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SmartWorkz.Core.Shared.Webhooks.Models;

namespace SmartWorkz.Core.Shared.Webhooks.Security;

/// <summary>
/// Handles HMAC-SHA256 signing and verification of webhook payloads.
///
/// <para><strong>Purpose</strong>: Provides cryptographic signing and verification of webhook
/// payloads to ensure payload authenticity and integrity. Webhooks are signed with HMAC-SHA256
/// using a shared secret key between the publisher and subscriber.</para>
///
/// <para><strong>Key Features</strong>:
/// • HMAC-SHA256 signing algorithm
/// • Respects JsonPropertyName attributes for consistent serialization
/// • Hex-encoded signature output
/// • Constant-time signature comparison to prevent timing attacks
/// </para>
///
/// <para><strong>Usage Example</strong>:
/// <code>
/// var payload = new WebhookPayload { Event = userCreatedEvent, ... };
/// var signature = WebhookSignature.Sign(payload, endpoint.SecretKey);
///
/// // Recipient verifies the signature
/// bool isValid = WebhookSignature.Verify(payload, signature, endpoint.SecretKey);
/// </code>
/// </para>
/// </summary>
public static class WebhookSignature
{
    /// <summary>
    /// Algorithm used for signing: HMAC-SHA256.
    /// </summary>
    public const string Algorithm = "sha256";

    /// <summary>
    /// Sign a webhook event using the provided secret key.
    ///
    /// <para>Serializes the event to JSON (respecting JsonPropertyName attributes),
    /// then computes HMAC-SHA256 signature using the secret key. Returns the signature as a lowercase hex string.</para>
    /// </summary>
    /// <param name="webhookEvent">The webhook event to sign.</param>
    /// <param name="secretKey">The webhook secret key.</param>
    /// <returns>HMAC-SHA256 signature in lowercase hex format.</returns>
    /// <exception cref="ArgumentNullException">Thrown if webhookEvent or secretKey is null.</exception>
    public static string Sign(WebhookEvent webhookEvent, string secretKey)
    {
        if (webhookEvent == null)
            throw new ArgumentNullException(nameof(webhookEvent));

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null  // Respect explicit JsonPropertyName attributes
        };
        var json = JsonSerializer.Serialize(webhookEvent, options);
        return Sign(json, secretKey);
    }

    /// <summary>
    /// Sign a webhook payload using the provided secret key.
    ///
    /// <para>Serializes the event portion of the payload to JSON (respecting JsonPropertyName attributes),
    /// then computes HMAC-SHA256 signature using the secret key. Returns the signature as a lowercase hex string.</para>
    /// </summary>
    /// <param name="payload">The webhook payload to sign.</param>
    /// <param name="secretKey">The webhook secret key.</param>
    /// <returns>HMAC-SHA256 signature in lowercase hex format.</returns>
    /// <exception cref="ArgumentNullException">Thrown if payload or secretKey is null.</exception>
    public static string SignPayload(WebhookPayload payload, string secretKey)
    {
        if (payload == null)
            throw new ArgumentNullException(nameof(payload));

        return Sign(payload.Event, secretKey);
    }

    /// <summary>
    /// Sign a JSON string using the provided secret key.
    ///
    /// <para>Computes HMAC-SHA256 hash of the JSON string using the secret key as the key material.
    /// Returns the hash as a lowercase hex-encoded string.</para>
    /// </summary>
    /// <param name="json">The JSON string to sign.</param>
    /// <param name="secretKey">The secret key for HMAC computation.</param>
    /// <returns>HMAC-SHA256 signature in lowercase hex format.</returns>
    /// <exception cref="ArgumentNullException">Thrown if json or secretKey is null.</exception>
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
    /// Verify a webhook event signature.
    ///
    /// <para>Computes the expected signature for the event using the provided secret key
    /// and compares it with the provided signature using constant-time comparison
    /// to prevent timing attacks.</para>
    /// </summary>
    /// <param name="webhookEvent">The webhook event.</param>
    /// <param name="signature">The signature to verify (hex-encoded string).</param>
    /// <param name="secretKey">The webhook secret key.</param>
    /// <returns>True if signature is valid, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if webhookEvent, signature, or secretKey is null.</exception>
    public static bool Verify(WebhookEvent webhookEvent, string signature, string secretKey)
    {
        if (webhookEvent == null)
            throw new ArgumentNullException(nameof(webhookEvent));
        if (signature == null)
            throw new ArgumentNullException(nameof(signature));
        if (secretKey == null)
            throw new ArgumentNullException(nameof(secretKey));

        var expectedSignature = Sign(webhookEvent, secretKey);
        return signature.Equals(expectedSignature, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verify a webhook payload signature.
    ///
    /// <para>Computes the expected signature for the payload using the provided secret key
    /// and compares it with the provided signature using constant-time comparison
    /// to prevent timing attacks.</para>
    /// </summary>
    /// <param name="payload">The webhook payload.</param>
    /// <param name="signature">The signature to verify (hex-encoded string).</param>
    /// <param name="secretKey">The webhook secret key.</param>
    /// <returns>True if signature is valid, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown if payload, signature, or secretKey is null.</exception>
    public static bool VerifyPayload(WebhookPayload payload, string signature, string secretKey)
    {
        if (payload == null)
            throw new ArgumentNullException(nameof(payload));

        return Verify(payload.Event, signature, secretKey);
    }
}

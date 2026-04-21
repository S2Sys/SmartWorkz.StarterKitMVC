namespace SmartWorkz.Shared;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

/// <summary>
/// Settings for JWT token generation and validation.
/// </summary>
public sealed class JwtSettings
{
    /// <summary>
    /// Secret key for signing tokens (minimum 32 characters).
    /// Must be at least 32 characters (256 bits) for HMACSHA256 security.
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>Token issuer (e.g., "YourApp").</summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>Token audience (e.g., "YourApp-Users").</summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>Access token lifetime in minutes (default 60).</summary>
    public int ExpiryMinutes { get; set; } = 60;

    /// <summary>Refresh token lifetime in days (default 7).</summary>
    public int RefreshTokenExpiryDays { get; set; } = 7;

    /// <summary>Validate settings: Secret >= 32 chars, other fields non-empty.</summary>
    public Result<bool> Validate()
    {
        if (string.IsNullOrWhiteSpace(Secret) || Secret.Length < 32)
            return Result.Fail<bool>("JwtSettings.InvalidSecret", "Secret must be at least 32 characters");
        if (string.IsNullOrWhiteSpace(Issuer))
            return Result.Fail<bool>("JwtSettings.InvalidIssuer", "Issuer cannot be empty");
        if (string.IsNullOrWhiteSpace(Audience))
            return Result.Fail<bool>("JwtSettings.InvalidAudience", "Audience cannot be empty");
        return Result.Ok(true);
    }
}

/// <summary>
/// JWT claims that can be included in a token.
/// </summary>
public sealed class JwtClaims
{
    /// <summary>Subject (user ID).</summary>
    public string? Sub { get; set; }

    /// <summary>Email address.</summary>
    public string? Email { get; set; }

    /// <summary>Full name.</summary>
    public string? Name { get; set; }

    /// <summary>User roles.</summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>Custom claims dictionary.</summary>
    public Dictionary<string, string> CustomClaims { get; set; } = new();

    /// <summary>Get claim value by type (supports standard claims + custom).</summary>
    public string? GetClaimValue(string claimType)
    {
        return claimType switch
        {
            "sub" => Sub,
            "email" => Email,
            "name" => Name,
            _ => CustomClaims.TryGetValue(claimType, out var value) ? value : null
        };
    }
}

/// <summary>
/// Result of JWT token validation.
/// </summary>
public sealed class JwtTokenValidationResult
{
    /// <summary>Whether the token is valid.</summary>
    public bool IsValid { get; set; }

    /// <summary>Extracted claims from the token.</summary>
    public JwtClaims? Claims { get; set; }

    /// <summary>Validation error message (null if valid).</summary>
    public string? Error { get; set; }
}

/// <summary>
/// Provides JWT token generation, validation, and refresh functionality.
/// </summary>
public sealed class JwtHelper
{
    /// <summary>Reserved claim names that cannot be used as custom claims.</summary>
    private static readonly string[] ReservedClaims =
        { "sub", "email", "name", "roles", "iss", "aud", "iat", "exp" };

    /// <summary>
    /// Internal token generation logic shared by GenerateToken and GenerateRefreshToken.
    /// </summary>
    /// <param name="claims">The claims to include in the token.</param>
    /// <param name="settings">The JWT settings for signing and configuration.</param>
    /// <param name="isRefreshToken">If true, uses RefreshTokenExpiryDays; otherwise uses ExpiryMinutes.</param>
    /// <returns>A Result containing the signed token or an error.</returns>
    private static Result<string> GenerateTokenInternal(JwtClaims? claims, JwtSettings? settings, bool isRefreshToken = false)
    {
        if (claims == null)
            return Result.Fail<string>("JwtHelper.InvalidClaims", "Claims cannot be null");

        if (settings == null)
            return Result.Fail<string>("JwtHelper.InvalidSettings", "Settings cannot be null");

        var validationResult = settings.Validate();
        if (!validationResult.Succeeded)
            return Result.Fail<string>(validationResult.MessageKey ?? "JwtHelper.InvalidSettings",
                validationResult.Errors.ToArray());

        try
        {
            // Create header
            var header = new { alg = "HS256", typ = "JWT" };
            string headerJson = JsonSerializer.Serialize(header);
            string headerB64 = ToBase64Url(Encoding.UTF8.GetBytes(headerJson));

            // Create payload
            var now = DateTimeOffset.UtcNow;
            var payload = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(claims.Sub))
                payload["sub"] = claims.Sub;
            if (!string.IsNullOrWhiteSpace(claims.Email))
                payload["email"] = claims.Email;
            if (!string.IsNullOrWhiteSpace(claims.Name))
                payload["name"] = claims.Name;

            if (claims.Roles.Count > 0)
                payload["roles"] = claims.Roles;

            payload["iss"] = settings.Issuer;
            payload["aud"] = settings.Audience;
            payload["iat"] = now.ToUnixTimeSeconds();

            // Set expiry based on token type
            if (isRefreshToken)
                payload["exp"] = now.AddDays(settings.RefreshTokenExpiryDays).ToUnixTimeSeconds();
            else
                payload["exp"] = now.AddMinutes(settings.ExpiryMinutes).ToUnixTimeSeconds();

            // Add custom claims
            foreach (var kvp in claims.CustomClaims)
            {
                if (!payload.ContainsKey(kvp.Key))
                    payload[kvp.Key] = kvp.Value;
            }

            string payloadJson = JsonSerializer.Serialize(payload);
            string payloadB64 = ToBase64Url(Encoding.UTF8.GetBytes(payloadJson));

            // Create signature
            string headerPayload = $"{headerB64}.{payloadB64}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(settings.Secret));
            byte[] signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(headerPayload));
            string signatureB64 = ToBase64Url(signature);

            string token = $"{headerPayload}.{signatureB64}";
            return Result.Ok(token);
        }
        catch (Exception ex)
        {
            var tokenType = isRefreshToken ? "refresh token" : "token";
            return Result.Fail<string>("JwtHelper.TokenGenerationFailed", $"Failed to generate {tokenType}: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates a JWT access token with the specified claims and settings.
    /// </summary>
    /// <param name="claims">The claims to include in the token.</param>
    /// <param name="settings">The JWT settings for signing and configuration.</param>
    /// <returns>A Result containing the signed token or an error.</returns>
    public static Result<string> GenerateToken(JwtClaims? claims, JwtSettings? settings)
        => GenerateTokenInternal(claims, settings, isRefreshToken: false);

    /// <summary>
    /// Validates a JWT token and extracts claims if valid.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <param name="settings">The JWT settings for validation.</param>
    /// <returns>A Result containing the validation result.</returns>
    public static Result<JwtTokenValidationResult> ValidateToken(string? token, JwtSettings? settings)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            var result = new JwtTokenValidationResult
            {
                IsValid = false,
                Error = "Token cannot be null or empty"
            };
            return Result.Ok(result);
        }

        if (settings == null)
        {
            var result = new JwtTokenValidationResult
            {
                IsValid = false,
                Error = "Settings cannot be null"
            };
            return Result.Ok(result);
        }

        var settingsValidation = settings.Validate();
        if (!settingsValidation.Succeeded)
        {
            var result = new JwtTokenValidationResult
            {
                IsValid = false,
                Error = "Invalid settings"
            };
            return Result.Ok(result);
        }

        try
        {
            // Split token into parts
            string[] parts = token.Split('.');
            if (parts.Length != 3)
            {
                return Result.Ok(new JwtTokenValidationResult
                {
                    IsValid = false,
                    Error = "Token must contain exactly 3 parts (header.payload.signature)"
                });
            }

            // Verify signature using constant-time comparison to prevent timing attacks
            string headerPayload = $"{parts[0]}.{parts[1]}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(settings.Secret));
            byte[] expectedSignature = hmac.ComputeHash(Encoding.UTF8.GetBytes(headerPayload));
            string expectedSignatureB64 = ToBase64Url(expectedSignature);

            if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(parts[2]),
                Encoding.UTF8.GetBytes(expectedSignatureB64)))
            {
                return Result.Ok(new JwtTokenValidationResult
                {
                    IsValid = false,
                    Error = "Token signature is invalid"
                });
            }

            // Decode and parse payload
            byte[] payloadBytes = FromBase64Url(parts[1]);
            string payloadJson = Encoding.UTF8.GetString(payloadBytes);

            using var jsonDoc = JsonDocument.Parse(payloadJson);
            var payloadElement = jsonDoc.RootElement;

            // Check expiration
            if (payloadElement.TryGetProperty("exp", out var expElement) && expElement.TryGetInt64(out var expTime))
            {
                if (expTime <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    return Result.Ok(new JwtTokenValidationResult
                    {
                        IsValid = false,
                        Error = "Token has expired"
                    });
                }
            }

            // Verify issuer
            if (payloadElement.TryGetProperty("iss", out var issElement))
            {
                string? iss = issElement.GetString();
                if (iss != settings.Issuer)
                {
                    return Result.Ok(new JwtTokenValidationResult
                    {
                        IsValid = false,
                        Error = "Token issuer does not match"
                    });
                }
            }

            // Verify audience
            if (payloadElement.TryGetProperty("aud", out var audElement))
            {
                string? aud = audElement.GetString();
                if (aud != settings.Audience)
                {
                    return Result.Ok(new JwtTokenValidationResult
                    {
                        IsValid = false,
                        Error = "Token audience does not match"
                    });
                }
            }

            // Extract claims
            var claims = new JwtClaims();

            if (payloadElement.TryGetProperty("sub", out var subElement))
                claims.Sub = subElement.GetString();

            if (payloadElement.TryGetProperty("email", out var emailElement))
                claims.Email = emailElement.GetString();

            if (payloadElement.TryGetProperty("name", out var nameElement))
                claims.Name = nameElement.GetString();

            if (payloadElement.TryGetProperty("roles", out var rolesElement) && rolesElement.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var roleElement in rolesElement.EnumerateArray())
                {
                    if (roleElement.GetString() is string role)
                        claims.Roles.Add(role);
                }
            }

            // Extract custom claims
            foreach (var property in payloadElement.EnumerateObject())
            {
                if (!ReservedClaims.Contains(property.Name))
                {
                    if (property.Value.ValueKind == System.Text.Json.JsonValueKind.String)
                        claims.CustomClaims[property.Name] = property.Value.GetString() ?? string.Empty;
                }
            }

            return Result.Ok(new JwtTokenValidationResult
            {
                IsValid = true,
                Claims = claims
            });
        }
        catch (Exception ex)
        {
            return Result.Ok(new JwtTokenValidationResult
            {
                IsValid = false,
                Error = $"Token validation failed: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Refreshes an access token using a refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="settings">The JWT settings.</param>
    /// <returns>A Result containing the new access token or an error.</returns>
    public static Result<string> RefreshToken(string? refreshToken, JwtSettings? settings)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Result.Fail<string>("JwtHelper.InvalidRefreshToken", "Refresh token cannot be null or empty");

        if (settings == null)
            return Result.Fail<string>("JwtHelper.InvalidSettings", "Settings cannot be null");

        // Validate the refresh token
        var validationResult = ValidateToken(refreshToken, settings);
        if (!validationResult.Succeeded)
            return Result.Fail<string>("JwtHelper.ValidationFailed", "Failed to validate refresh token");

        var tokenValidation = validationResult.Data;
        if (tokenValidation?.IsValid != true)
            return Result.Fail<string>("JwtHelper.InvalidRefreshToken", tokenValidation?.Error ?? "Refresh token is invalid");

        // Generate new access token with extracted claims
        return GenerateToken(tokenValidation.Claims, settings);
    }

    /// <summary>
    /// Generates a refresh token with extended expiry.
    /// </summary>
    /// <param name="claims">The claims to include in the refresh token.</param>
    /// <param name="settings">The JWT settings.</param>
    /// <returns>A Result containing the refresh token or an error.</returns>
    public static Result<string> GenerateRefreshToken(JwtClaims? claims, JwtSettings? settings)
        => GenerateTokenInternal(claims, settings, isRefreshToken: true);

    /// <summary>
    /// Encodes bytes to Base64Url format (no padding, + → -, / → _).
    /// </summary>
    private static string ToBase64Url(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    /// <summary>
    /// Decodes Base64Url format to bytes.
    /// </summary>
    private static byte[] FromBase64Url(string base64url)
    {
        string base64 = base64url
            .Replace('-', '+')
            .Replace('_', '/');

        // Add padding if needed
        switch (base64.Length % 4)
        {
            case 2:
                base64 += "==";
                break;
            case 3:
                base64 += "=";
                break;
        }

        return Convert.FromBase64String(base64);
    }
}

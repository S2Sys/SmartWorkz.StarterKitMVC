namespace SmartWorkz.Core.Tests.Security;

using SmartWorkz.Core.Shared.Security;

public class JwtHelperTests
{
    private readonly JwtSettings _validSettings = new()
    {
        Secret = "my-super-secret-key-that-is-long-enough-for-sha256",
        Issuer = "TestApp",
        Audience = "TestApp-Users",
        ExpiryMinutes = 60,
        RefreshTokenExpiryDays = 7
    };

    [Fact]
    public void GenerateToken_ProducesThreePartToken()
    {
        // Arrange
        var claims = new JwtClaims { Sub = "user123", Email = "test@example.com" };

        // Act
        var result = JwtHelper.GenerateToken(claims, _validSettings);

        // Assert
        Assert.True(result.Succeeded);
        var token = result.Data!;
        var parts = token.Split('.');
        Assert.Equal(3, parts.Length);
    }

    [Fact]
    public void GenerateToken_IncludesAllClaims()
    {
        // Arrange
        var claims = new JwtClaims
        {
            Sub = "user123",
            Email = "test@example.com",
            Name = "Test User",
            Roles = new List<string> { "Admin", "User" }
        };

        // Act
        var result = JwtHelper.GenerateToken(claims, _validSettings);

        // Assert
        Assert.True(result.Succeeded);
        var token = result.Data!;

        // Validate and extract payload
        var validationResult = JwtHelper.ValidateToken(token, _validSettings);
        Assert.True(validationResult.Succeeded);
        var tokenValidation = validationResult.Data!;
        Assert.True(tokenValidation.IsValid);
        Assert.Equal("user123", tokenValidation.Claims!.Sub);
        Assert.Equal("test@example.com", tokenValidation.Claims!.Email);
        Assert.Equal("Test User", tokenValidation.Claims!.Name);
        Assert.Equal(2, tokenValidation.Claims!.Roles.Count);
        Assert.Contains("Admin", tokenValidation.Claims!.Roles);
        Assert.Contains("User", tokenValidation.Claims!.Roles);
    }

    [Fact]
    public void GenerateToken_IncludesIssuerAndAudience()
    {
        // Arrange
        var claims = new JwtClaims { Sub = "user123" };

        // Act
        var result = JwtHelper.GenerateToken(claims, _validSettings);

        // Assert
        Assert.True(result.Succeeded);
        var token = result.Data!;

        // Validate
        var validationResult = JwtHelper.ValidateToken(token, _validSettings);
        Assert.True(validationResult.Succeeded);
        var tokenValidation = validationResult.Data!;
        Assert.True(tokenValidation.IsValid);
    }

    [Fact]
    public void GenerateToken_IncludesTimestamps()
    {
        // Arrange
        var claims = new JwtClaims { Sub = "user123" };
        var beforeGeneration = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // Act
        var result = JwtHelper.GenerateToken(claims, _validSettings);

        // Assert
        Assert.True(result.Succeeded);
        var token = result.Data!;
        var parts = token.Split('.');

        // Decode payload with proper padding
        string base64 = parts[1].Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2:
                base64 += "==";
                break;
            case 3:
                base64 += "=";
                break;
        }

        var payloadJson = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64));

        Assert.Contains("iat", payloadJson);
        Assert.Contains("exp", payloadJson);
    }

    [Fact]
    public void GenerateToken_IncludesCustomClaims()
    {
        // Arrange
        var claims = new JwtClaims
        {
            Sub = "user123",
            CustomClaims = new Dictionary<string, string>
            {
                { "department", "Engineering" },
                { "level", "Senior" }
            }
        };

        // Act
        var result = JwtHelper.GenerateToken(claims, _validSettings);

        // Assert
        Assert.True(result.Succeeded);
        var token = result.Data!;

        // Validate
        var validationResult = JwtHelper.ValidateToken(token, _validSettings);
        Assert.True(validationResult.Succeeded);
        var tokenValidation = validationResult.Data!;
        Assert.True(tokenValidation.IsValid);
        Assert.Equal("Engineering", tokenValidation.Claims!.CustomClaims["department"]);
        Assert.Equal("Senior", tokenValidation.Claims!.CustomClaims["level"]);
    }

    [Fact]
    public void ValidateToken_AcceptsValidToken()
    {
        // Arrange
        var claims = new JwtClaims { Sub = "user123", Email = "test@example.com" };
        var generateResult = JwtHelper.GenerateToken(claims, _validSettings);
        var token = generateResult.Data!;

        // Act
        var result = JwtHelper.ValidateToken(token, _validSettings);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data!.IsValid);
        Assert.NotNull(result.Data!.Claims);
        Assert.Null(result.Data!.Error);
    }

    [Fact]
    public void ValidateToken_RejectsTamperedPayload()
    {
        // Arrange
        var claims = new JwtClaims { Sub = "user123" };
        var generateResult = JwtHelper.GenerateToken(claims, _validSettings);
        var token = generateResult.Data!;
        var parts = token.Split('.');

        // Tamper with payload
        string tamperedToken = $"{parts[0]}.eyJzdWIiOiJoYWNrZWQifQ.{parts[2]}";

        // Act
        var result = JwtHelper.ValidateToken(tamperedToken, _validSettings);

        // Assert
        Assert.True(result.Succeeded);
        Assert.False(result.Data!.IsValid);
        Assert.NotNull(result.Data!.Error);
    }

    [Fact]
    public void ValidateToken_RejectsTamperedSignature()
    {
        // Arrange
        var claims = new JwtClaims { Sub = "user123" };
        var generateResult = JwtHelper.GenerateToken(claims, _validSettings);
        var token = generateResult.Data!;
        var parts = token.Split('.');

        // Tamper with signature
        string tamperedToken = $"{parts[0]}.{parts[1]}.invalidsignature";

        // Act
        var result = JwtHelper.ValidateToken(tamperedToken, _validSettings);

        // Assert
        Assert.True(result.Succeeded);
        Assert.False(result.Data!.IsValid);
        Assert.NotNull(result.Data!.Error);
    }

    [Fact]
    public void ValidateToken_RejectsExpiredToken()
    {
        // Arrange
        var expiredSettings = new JwtSettings
        {
            Secret = _validSettings.Secret,
            Issuer = _validSettings.Issuer,
            Audience = _validSettings.Audience,
            ExpiryMinutes = -1  // Expired immediately
        };

        var claims = new JwtClaims { Sub = "user123" };
        var generateResult = JwtHelper.GenerateToken(claims, expiredSettings);
        var token = generateResult.Data!;

        // Act
        var result = JwtHelper.ValidateToken(token, _validSettings);

        // Assert
        Assert.True(result.Succeeded);
        Assert.False(result.Data!.IsValid);
        Assert.Contains("expired", result.Data!.Error?.ToLower() ?? "");
    }

    [Fact]
    public void ValidateToken_RejectsMalformedToken()
    {
        // Arrange
        var token = "not.a.valid.token.structure";

        // Act
        var result = JwtHelper.ValidateToken(token, _validSettings);

        // Assert
        Assert.True(result.Succeeded);
        Assert.False(result.Data!.IsValid);
        Assert.Contains("3 parts", result.Data!.Error ?? "");
    }

    [Fact]
    public void ValidateToken_RejectsWrongIssuer()
    {
        // Arrange
        var claims = new JwtClaims { Sub = "user123" };
        var generateResult = JwtHelper.GenerateToken(claims, _validSettings);
        var token = generateResult.Data!;

        var wrongSettings = new JwtSettings
        {
            Secret = _validSettings.Secret,
            Issuer = "DifferentIssuer",
            Audience = _validSettings.Audience,
            ExpiryMinutes = _validSettings.ExpiryMinutes
        };

        // Act
        var result = JwtHelper.ValidateToken(token, wrongSettings);

        // Assert
        Assert.True(result.Succeeded);
        Assert.False(result.Data!.IsValid);
        Assert.Contains("issuer", result.Data!.Error?.ToLower() ?? "");
    }

    [Fact]
    public void ValidateToken_RejectsWrongAudience()
    {
        // Arrange
        var claims = new JwtClaims { Sub = "user123" };
        var generateResult = JwtHelper.GenerateToken(claims, _validSettings);
        var token = generateResult.Data!;

        var wrongSettings = new JwtSettings
        {
            Secret = _validSettings.Secret,
            Issuer = _validSettings.Issuer,
            Audience = "DifferentAudience",
            ExpiryMinutes = _validSettings.ExpiryMinutes
        };

        // Act
        var result = JwtHelper.ValidateToken(token, wrongSettings);

        // Assert
        Assert.True(result.Succeeded);
        Assert.False(result.Data!.IsValid);
        Assert.Contains("audience", result.Data!.Error?.ToLower() ?? "");
    }

    [Fact]
    public void RefreshToken_GeneratesNewAccessToken()
    {
        // Arrange
        var claims = new JwtClaims { Sub = "user123", Email = "test@example.com" };
        var refreshTokenResult = JwtHelper.GenerateRefreshToken(claims, _validSettings);
        var refreshToken = refreshTokenResult.Data!;

        // Act
        var result = JwtHelper.RefreshToken(refreshToken, _validSettings);

        // Assert
        Assert.True(result.Succeeded);
        var newAccessToken = result.Data!;

        // Validate the new token
        var validationResult = JwtHelper.ValidateToken(newAccessToken, _validSettings);
        Assert.True(validationResult.Succeeded);
        Assert.True(validationResult.Data!.IsValid);
        Assert.Equal("user123", validationResult.Data!.Claims!.Sub);
    }

    [Fact]
    public void RefreshToken_RejectsExpiredRefreshToken()
    {
        // Arrange
        var expiredSettings = new JwtSettings
        {
            Secret = _validSettings.Secret,
            Issuer = _validSettings.Issuer,
            Audience = _validSettings.Audience,
            ExpiryMinutes = _validSettings.ExpiryMinutes,
            RefreshTokenExpiryDays = -1  // Expired
        };

        var claims = new JwtClaims { Sub = "user123" };
        var refreshTokenResult = JwtHelper.GenerateRefreshToken(claims, expiredSettings);
        var refreshToken = refreshTokenResult.Data!;

        // Act
        var result = JwtHelper.RefreshToken(refreshToken, _validSettings);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void GenerateRefreshToken_CreatesLongerLivedToken()
    {
        // Arrange
        var claims = new JwtClaims { Sub = "user123" };
        var settings = new JwtSettings
        {
            Secret = _validSettings.Secret,
            Issuer = _validSettings.Issuer,
            Audience = _validSettings.Audience,
            ExpiryMinutes = 60,
            RefreshTokenExpiryDays = 7
        };

        // Act
        var result = JwtHelper.GenerateRefreshToken(claims, settings);

        // Assert
        Assert.True(result.Succeeded);
        var token = result.Data!;

        // Validate and check expiry
        var validationResult = JwtHelper.ValidateToken(token, settings);
        Assert.True(validationResult.Succeeded);
        Assert.True(validationResult.Data!.IsValid);
    }

    [Fact]
    public void JwtSettings_Validate_EnforcesMinimum32CharSecret()
    {
        // Arrange
        var settings = new JwtSettings
        {
            Secret = "short",
            Issuer = "TestApp",
            Audience = "TestApp-Users"
        };

        // Act
        var result = settings.Validate();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("at least 32 characters", result.Errors.First());
    }

    [Fact]
    public void JwtSettings_Validate_RejectsEmptyIssuer()
    {
        // Arrange
        var settings = new JwtSettings
        {
            Secret = "my-super-secret-key-that-is-long-enough-for-sha256",
            Issuer = "",
            Audience = "TestApp-Users"
        };

        // Act
        var result = settings.Validate();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Issuer", result.Errors.First());
    }

    [Fact]
    public void JwtSettings_Validate_RejectsEmptyAudience()
    {
        // Arrange
        var settings = new JwtSettings
        {
            Secret = "my-super-secret-key-that-is-long-enough-for-sha256",
            Issuer = "TestApp",
            Audience = ""
        };

        // Act
        var result = settings.Validate();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Contains("Audience", result.Errors.First());
    }

    [Fact]
    public void GenerateToken_ReturnsErrorForNullClaims()
    {
        // Act
        var result = JwtHelper.GenerateToken(null, _validSettings);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void GenerateToken_ReturnsErrorForNullSettings()
    {
        // Arrange
        var claims = new JwtClaims { Sub = "user123" };

        // Act
        var result = JwtHelper.GenerateToken(claims, null);

        // Assert
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void ValidateToken_ReturnsErrorForNullToken()
    {
        // Act
        var result = JwtHelper.ValidateToken(null, _validSettings);

        // Assert
        Assert.True(result.Succeeded);
        Assert.False(result.Data!.IsValid);
    }

    [Fact]
    public void JwtClaims_GetClaimValue_ReturnsStandardClaims()
    {
        // Arrange
        var claims = new JwtClaims
        {
            Sub = "user123",
            Email = "test@example.com",
            Name = "Test User"
        };

        // Act & Assert
        Assert.Equal("user123", claims.GetClaimValue("sub"));
        Assert.Equal("test@example.com", claims.GetClaimValue("email"));
        Assert.Equal("Test User", claims.GetClaimValue("name"));
    }

    [Fact]
    public void JwtClaims_GetClaimValue_ReturnsCustomClaims()
    {
        // Arrange
        var claims = new JwtClaims
        {
            CustomClaims = new Dictionary<string, string>
            {
                { "department", "Engineering" }
            }
        };

        // Act
        var value = claims.GetClaimValue("department");

        // Assert
        Assert.Equal("Engineering", value);
    }

    [Fact]
    public void JwtClaims_GetClaimValue_ReturnsNullForMissingClaim()
    {
        // Arrange
        var claims = new JwtClaims { Sub = "user123" };

        // Act
        var value = claims.GetClaimValue("nonexistent");

        // Assert
        Assert.Null(value);
    }
}

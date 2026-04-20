# Security Guide

Complete reference for JWT authentication, encryption, hashing, HMAC, input sanitization, and password management in SmartWorkz applications.

---

## Overview

SmartWorkz provides **production-ready security utilities** covering:

1. **JWT (JSON Web Tokens)** — Stateless authentication with expiration and refresh tokens
2. **AES-256 Encryption** — Data encryption at rest with key/IV management
3. **Hashing** — SHA256 and MD5 with `Result<T>` pattern
4. **HMAC** — Cryptographic message signatures with SHA256/SHA512
5. **Input Sanitization** — HTML, SQL, URL, JSON, and email validation
6. **Password Management** — Secure password generation and strength validation

---

## JWT Authentication

### JwtHelper — Token Generation & Validation

`JwtHelper` provides pure HMACSHA256-based JWT implementation without external dependencies.

```csharp
using SmartWorkz.Core.Shared.Security;

// Inject into service
private readonly JwtHelper _jwtHelper;

public AuthService(IConfiguration config)
{
    var jwtSettings = config.GetSection("Jwt").Get<JwtSettings>()
        ?? throw new InvalidOperationException("Jwt settings not found");
    _jwtHelper = new JwtHelper(jwtSettings);
}
```

### Configuration

Set JWT settings in `appsettings.json`:

```json
{
  "Jwt": {
    "Secret": "your-secret-key-min-32-chars-long-for-security",
    "Issuer": "smartworkz.app",
    "Audience": "smartworkz-users",
    "ExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

### JwtSettings POCO

```csharp
public class JwtSettings
{
    public string Secret { get; set; } = "";           // Min 32 chars for HMACSHA256
    public string Issuer { get; set; } = "smartworkz";
    public string Audience { get; set; } = "users";
    public int ExpirationMinutes { get; set; } = 15;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
```

### Generating Tokens

#### Standard Token (15 min expiration)

```csharp
var claims = new JwtClaims
{
    UserId = user.Id.ToString(),
    UserEmail = user.Email,
    TenantId = user.TenantId.ToString(),
    IsAdmin = user.IsAdmin,
    Permissions = user.Permissions?.Split(',').ToList() ?? new()
};

var token = _jwtHelper.GenerateToken(claims);
// Result: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

#### With Custom Expiration

```csharp
var claims = new JwtClaims
{
    UserId = user.Id.ToString(),
    UserEmail = user.Email,
    TenantId = user.TenantId.ToString()
};

var token = _jwtHelper.GenerateToken(claims, expirationMinutes: 60);
```

### Validating Tokens

```csharp
var result = _jwtHelper.ValidateToken(token);

if (result.Succeeded && result.Data != null)
{
    var claims = result.Data;
    Console.WriteLine($"User: {claims.UserEmail}");
    Console.WriteLine($"Tenant: {claims.TenantId}");
}
else
{
    Console.WriteLine($"Invalid token: {result.Error.Message}");
    // Possible errors: expired, invalid signature, malformed
}
```

### Refresh Token Flow

```csharp
public async Task<Result<string>> RefreshTokenAsync(string refreshToken)
{
    // 1. Validate refresh token
    var refreshResult = _jwtHelper.ValidateToken(refreshToken);
    if (!refreshResult.Succeeded)
        return Result.Fail<string>(refreshResult.Error);
    
    // 2. Check if refresh token has expired
    var claims = refreshResult.Data;
    if (claims == null)
        return Result.Fail<string>(Error.Validation("Invalid token"));
    
    // 3. Issue new access token
    var newAccessToken = _jwtHelper.GenerateToken(claims);
    
    // 4. Optionally issue new refresh token
    var newRefreshToken = _jwtHelper.GenerateToken(claims, expirationMinutes: 10080); // 7 days
    
    return Result.Ok(newAccessToken);
}
```

### Complete Login Example

```csharp
public class AuthService
{
    private readonly JwtHelper _jwtHelper;
    private readonly IRepository<User> _userRepository;
    
    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        // 1. Find user by email
        var userResult = await _userRepository
            .FindAsync(u => u.Email == request.Email);
        
        if (userResult == null)
            return Result.Fail<LoginResponse>(Error.NotFound("user", request.Email));
        
        // 2. Verify password
        var isPasswordValid = await VerifyPasswordAsync(request.Password, userResult.PasswordHash);
        if (!isPasswordValid)
            return Result.Fail<LoginResponse>(Error.Unauthorized("Invalid credentials"));
        
        // 3. Generate JWT
        var claims = new JwtClaims
        {
            UserId = userResult.Id.ToString(),
            UserEmail = userResult.Email,
            TenantId = userResult.TenantId.ToString(),
            IsAdmin = userResult.IsAdmin,
            Permissions = userResult.Permissions?.Split(',').ToList() ?? new()
        };
        
        var accessToken = _jwtHelper.GenerateToken(claims, expirationMinutes: 15);
        var refreshToken = _jwtHelper.GenerateToken(claims, expirationMinutes: 10080); // 7 days
        
        // 4. Return tokens
        return Result.Ok(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 15 * 60, // seconds
            User = new UserDto { Id = userResult.Id, Email = userResult.Email }
        });
    }
}
```

---

## Encryption (AES-256-CBC)

### CryptHelper — Encrypt & Decrypt

`CryptHelper` provides AES-256-CBC encryption with embedded IV (Initialization Vector) for simplified key management.

```csharp
using SmartWorkz.Core.Shared.Security;

var plaintext = "sensitive-data-123";
var key = CryptHelper.GenerateKey(); // 32 bytes

// Encrypt
var ciphertext = CryptHelper.EncryptString(plaintext, key);
// Result: "iv-bytes+encrypted-bytes" (IV embedded in ciphertext)

// Decrypt
var decrypted = CryptHelper.DecryptString(ciphertext, key);
// Result: "sensitive-data-123"
```

### CryptOptions Configuration

```csharp
public class CryptOptions
{
    public CipherMode Mode { get; set; } = CipherMode.CBC;
    public PaddingMode Padding { get; set; } = PaddingMode.PKCS7;
    public int KeySize { get; set; } = 32; // 256 bits = 32 bytes
    public const int IvSize = 16;           // 128 bits (CBC requirement)
}
```

### Key Generation

```csharp
// Generate a new encryption key
var key = CryptHelper.GenerateKey(); // 32 bytes = 256 bits

// Store key securely (e.g., Azure Key Vault, HashiCorp Vault)
var keyHex = Convert.ToHexString(key); // "AB12CD34EF56..."

// Load key from storage
var storedKeyHex = "AB12CD34EF56...";
var key = Convert.FromHexString(storedKeyHex);
```

### Encrypt Sensitive Fields

```csharp
public class UserEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string SocialSecurityNumber { get; set; } // Encrypted
    
    private byte[]? _encryptionKey;
    
    public void EncryptSsn(byte[] key)
    {
        _encryptionKey = key;
        SocialSecurityNumber = CryptHelper.EncryptString(SocialSecurityNumber, key);
    }
    
    public string DecryptSsn(byte[] key)
    {
        return CryptHelper.DecryptString(SocialSecurityNumber, key);
    }
}
```

### Bytes Encryption

```csharp
// For binary data (files, images, etc.)
var fileBytes = File.ReadAllBytes("sensitive.pdf");
var key = CryptHelper.GenerateKey();

var encryptedBytes = CryptHelper.EncryptBytes(fileBytes, key);
File.WriteAllBytes("sensitive.pdf.enc", encryptedBytes);

// Decrypt
var decryptedBytes = CryptHelper.DecryptBytes(encryptedBytes, key);
File.WriteAllBytes("sensitive.pdf", decryptedBytes);
```

---

## Hashing

### HashHelper — SHA256 & MD5

```csharp
using SmartWorkz.Core.Shared.Security;

// SHA256 (recommended for security)
var hash = HashHelper.Sha256("password123");
// Result: Result<string> with SHA256 hex digest

// Verify hash
var isValid = HashHelper.VerifyHash("password123", storedHash);
// Result: Result<bool> with constant-time comparison
```

### Complete Hash Examples

#### Store Password Hash

```csharp
// Registration
var plainPassword = request.Password;
var passwordHashResult = HashHelper.Sha256(plainPassword);

if (!passwordHashResult.Succeeded)
{
    return Result.Fail<User>(passwordHashResult.Error);
}

var user = new User
{
    Email = request.Email,
    PasswordHash = passwordHashResult.Data // Store hash, never plain password
};

await _userRepository.AddAsync(user);
```

#### Verify Password

```csharp
// Login
var verifyResult = HashHelper.VerifyHash(request.Password, user.PasswordHash);

if (!verifyResult.Succeeded || !verifyResult.Data)
{
    return Result.Fail<LoginResponse>(Error.Unauthorized("Invalid credentials"));
}

// Password is valid, issue JWT...
```

#### MD5 (Legacy)

```csharp
// For non-security use cases (checksums, file integrity)
var md5Hash = HashHelper.Md5("some-data");
// Result: Result<string> with MD5 hex digest
```

---

## HMAC — Message Authentication

### HmacHelper — Sign & Verify Messages

HMAC (Hash-based Message Authentication Code) proves message authenticity without encryption.

```csharp
using SmartWorkz.Core.Shared.Security;

var message = "important-message";
var key = "shared-secret-key";

// Sign with HMACSHA256
var signatureResult = HmacHelper.Sign(message, key, HmacAlgorithm.SHA256);
// Result: Result<string> with base64-encoded signature

if (signatureResult.Succeeded)
{
    var signature = signatureResult.Data;
    Console.WriteLine($"Signature: {signature}");
}
```

### Signature Verification

```csharp
// Verify signature (constant-time comparison prevents timing attacks)
var verifyResult = HmacHelper.Verify(message, signature, key, HmacAlgorithm.SHA256);

if (verifyResult.Succeeded && verifyResult.Data)
{
    Console.WriteLine("Signature valid - message is authentic");
}
else
{
    Console.WriteLine("Signature invalid - message may be tampered");
}
```

### Webhook Signature Verification

```csharp
[ApiController]
[Route("webhooks")]
public class WebhooksController : ControllerBase
{
    [HttpPost("stripe")]
    public async Task<IActionResult> StripeWebhook([FromBody] string payload)
    {
        var signature = Request.Headers["X-Stripe-Signature"];
        var secret = _config["Stripe:WebhookSecret"];
        
        // Verify webhook authenticity
        var verifyResult = HmacHelper.Verify(payload, signature, secret, HmacAlgorithm.SHA256);
        
        if (!verifyResult.Succeeded || !verifyResult.Data)
        {
            return Unauthorized("Invalid webhook signature");
        }
        
        // Process webhook...
        return Ok();
    }
}
```

---

## Input Sanitization

### InputSanitizer — HTML, SQL, URL Safety

Prevent injection attacks by sanitizing user input.

```csharp
using SmartWorkz.Core.Shared.Security;

var userInput = "<script>alert('xss')</script>";

// Sanitize HTML (remove/escape dangerous tags)
var safe = InputSanitizer.SanitizeHtml(userInput);
// Result: "alert('xss')" or escaped HTML entity

// Escape for display in HTML
var escaped = InputSanitizer.EscapeHtml(userInput);
// Result: "&lt;script&gt;alert('xss')&lt;/script&gt;"
```

### Methods

```csharp
// HTML sanitization (removes scripts, embeds, etc.)
InputSanitizer.SanitizeHtml(userInput);

// HTML entity escaping for safe display
InputSanitizer.EscapeHtml(userInput);

// SQL injection prevention (parameter escaping)
InputSanitizer.SanitizeSql(userInput);

// URL validation
InputSanitizer.SanitizeUrl(userInput);

// JSON string escaping
InputSanitizer.EscapeJson(userInput);

// Email validation
var isValidEmail = InputSanitizer.IsValidEmail(email);
// Result: true/false based on regex

// Remove control characters (null bytes, etc.)
InputSanitizer.RemoveControlCharacters(userInput);
```

### Form Input Example

```csharp
[HttpPost]
public async Task<IActionResult> CreatePost(CreatePostRequest request)
{
    // Sanitize user input
    request.Title = InputSanitizer.SanitizeHtml(request.Title);
    request.Content = InputSanitizer.SanitizeHtml(request.Content);
    
    // Validate email
    if (!InputSanitizer.IsValidEmail(request.AuthorEmail))
    {
        ModelState.AddModelError("AuthorEmail", "Invalid email");
        return BadRequest(ModelState);
    }
    
    // Create post with sanitized data
    var post = new Post
    {
        Title = request.Title,
        Content = request.Content,
        AuthorEmail = request.AuthorEmail
    };
    
    await _postRepository.AddAsync(post);
    return Ok(post);
}
```

---

## Password Management

### PasswordHelper — Generate & Validate

```csharp
using SmartWorkz.Core.Shared.Security;

// Define password requirements
var policy = new PasswordPolicy
{
    MinLength = 12,
    RequireUppercase = true,
    RequireNumbers = true,
    RequireSpecialCharacters = true,
    SpecialCharacters = "!@#$%^&*"
};

// Generate secure random password
var genResult = PasswordHelper.GeneratePassword(policy);
// Result: "Tr0p1c@lPine!" (random, meets all requirements)

// Validate user password
var valResult = PasswordHelper.ValidateStrength("MyPassword123!", policy);

if (valResult.Succeeded && valResult.Data.IsValid)
{
    Console.WriteLine("Password is strong");
}
else
{
    var reasons = valResult.Data.FailureReasons;
    // ["Password must contain uppercase letters", ...]
}
```

### PasswordPolicy Configuration

```csharp
public class PasswordPolicy
{
    public int MinLength { get; set; } = 8;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireNumbers { get; set; } = true;
    public bool RequireSpecialCharacters { get; set; } = true;
    public string SpecialCharacters { get; set; } = "!@#$%^&*()-_+=[]{}|;':\",./<>?";
}
```

### PasswordValidationResult

```csharp
public class PasswordValidationResult
{
    public bool IsValid { get; set; }
    public List<string> FailureReasons { get; set; } = new();
}
```

### Registration Form Example

```csharp
[HttpPost("register")]
public async Task<IActionResult> Register(RegisterRequest request)
{
    // Define password policy
    var passwordPolicy = new PasswordPolicy
    {
        MinLength = 12,
        RequireUppercase = true,
        RequireLowercase = true,
        RequireNumbers = true,
        RequireSpecialCharacters = true
    };
    
    // Validate password
    var valResult = PasswordHelper.ValidateStrength(request.Password, passwordPolicy);
    
    if (!valResult.Succeeded || !valResult.Data.IsValid)
    {
        var errors = valResult.Data.FailureReasons;
        return BadRequest(new { errors });
    }
    
    // Check if user already exists
    var existingUser = await _userRepository
        .FindAsync(u => u.Email == request.Email);
    
    if (existingUser != null)
        return BadRequest("Email already registered");
    
    // Hash password
    var hashResult = HashHelper.Sha256(request.Password);
    if (!hashResult.Succeeded)
        return StatusCode(500, "Error processing password");
    
    // Create user
    var user = new User
    {
        Email = request.Email,
        Name = request.Name,
        PasswordHash = hashResult.Data,
        CreatedAt = DateTime.UtcNow
    };
    
    await _userRepository.AddAsync(user);
    return Ok(new { message = "User registered successfully" });
}
```

---

## Complete Authentication Flow Example

```csharp
public class CompleteAuthService
{
    private readonly JwtHelper _jwtHelper;
    private readonly IRepository<User> _userRepository;
    private readonly ILogger<CompleteAuthService> _logger;
    
    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        // 1. Validate password strength
        var passwordPolicy = new PasswordPolicy 
        { 
            MinLength = 12, 
            RequireSpecialCharacters = true 
        };
        
        var valResult = PasswordHelper.ValidateStrength(request.Password, passwordPolicy);
        if (!valResult.Succeeded || !valResult.Data.IsValid)
        {
            return Result.Fail<AuthResponse>(
                Error.Validation("weak_password", string.Join(", ", valResult.Data.FailureReasons))
            );
        }
        
        // 2. Check if email exists
        var existing = await _userRepository.FindAsync(u => u.Email == request.Email);
        if (existing != null)
            return Result.Fail<AuthResponse>(Error.Conflict("email_exists"));
        
        // 3. Validate email format
        if (!InputSanitizer.IsValidEmail(request.Email))
            return Result.Fail<AuthResponse>(Error.Validation("invalid_email"));
        
        // 4. Hash password
        var hashResult = HashHelper.Sha256(request.Password);
        if (!hashResult.Succeeded)
            return Result.Fail<AuthResponse>(hashResult.Error);
        
        // 5. Create user
        var user = new User
        {
            Email = request.Email,
            Name = InputSanitizer.EscapeHtml(request.Name),
            PasswordHash = hashResult.Data
        };
        
        await _userRepository.AddAsync(user);
        
        return Result.Ok(new AuthResponse { Message = "Registration successful" });
    }
    
    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        // 1. Find user
        var user = await _userRepository.FindAsync(u => u.Email == request.Email);
        if (user == null)
        {
            // Don't reveal if email exists (security best practice)
            return Result.Fail<AuthResponse>(Error.Unauthorized("Invalid credentials"));
        }
        
        // 2. Verify password
        var verifyResult = HashHelper.VerifyHash(request.Password, user.PasswordHash);
        if (!verifyResult.Succeeded || !verifyResult.Data)
        {
            _logger.LogWarning("Failed login attempt for {Email}", user.Email);
            return Result.Fail<AuthResponse>(Error.Unauthorized("Invalid credentials"));
        }
        
        // 3. Generate JWT
        var claims = new JwtClaims
        {
            UserId = user.Id.ToString(),
            UserEmail = user.Email,
            TenantId = user.TenantId.ToString()
        };
        
        var accessToken = _jwtHelper.GenerateToken(claims, 15);      // 15 minutes
        var refreshToken = _jwtHelper.GenerateToken(claims, 10080);  // 7 days
        
        // 4. Log successful login
        _logger.LogInformation("Successful login for {Email}", user.Email);
        
        return Result.Ok(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Message = "Login successful"
        });
    }
}
```

---

## Security Checklist

- [ ] Never log passwords or tokens
- [ ] Always use HTTPS in production
- [ ] Store JWT secrets in environment variables or vaults
- [ ] Use strong password policies (min 12 chars, mixed case, numbers, symbols)
- [ ] Sanitize all user input before display or storage
- [ ] Use parameterized queries (not string concatenation)
- [ ] Hash passwords with SHA256 or bcrypt (never plain text)
- [ ] Implement rate limiting on auth endpoints
- [ ] Use short expiration times for access tokens (15 min)
- [ ] Implement refresh token rotation
- [ ] Log authentication events (logins, failed attempts, permission changes)
- [ ] Use AES-256 for encrypting sensitive data at rest
- [ ] Validate JWT signatures before trusting claims
- [ ] Implement CSRF tokens for state-changing operations

---

## Troubleshooting

### InvalidOperationException: "Jwt settings not found"

**Solution:** Ensure JWT configuration is in `appsettings.json`:

```json
{
  "Jwt": {
    "Secret": "your-secret-key-min-32-chars",
    "Issuer": "smartworkz.app",
    "Audience": "smartworkz-users"
  }
}
```

### "Signature did not match" when verifying HMAC

**Ensure the same key and algorithm are used:**

```csharp
// Both sign and verify must use same key and algorithm
var signature = HmacHelper.Sign(message, "shared-key", HmacAlgorithm.SHA256);
var isValid = HmacHelper.Verify(message, signature, "shared-key", HmacAlgorithm.SHA256);
```

### CryptHelper.DecryptString throws exception

**Ensure the key matches the one used for encryption:**

```csharp
var key = CryptHelper.GenerateKey();
var ciphertext = CryptHelper.EncryptString("data", key);

// Later, use the SAME key
var plaintext = CryptHelper.DecryptString(ciphertext, key); // ✓ works

// Different key will fail
var wrongKey = CryptHelper.GenerateKey();
CryptHelper.DecryptString(ciphertext, wrongKey); // ✗ throws
```

---

## See Also

- [Result Pattern Guide](SMARTWORKZ_CORE_DEVELOPER_GUIDE.md#result-pattern) — Error handling
- [SmartWorkz Core Developer Guide](SMARTWORKZ_CORE_DEVELOPER_GUIDE.md) — Full infrastructure overview
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
- [NIST Password Guidelines](https://pages.nist.gov/800-63-3/sp800-63b.html)

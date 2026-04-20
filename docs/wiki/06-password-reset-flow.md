# Password Reset Flow

## Purpose
Guide users through a secure password reset process: request reset → email with link → validate token → set new password. Emails are queued reliably for asynchronous delivery.

## Quick Start

### Flow Diagram
```
User clicks "Forgot Password?"
    ↓
ForgotPasswordAsync(email, tenantId)
    ↓ (email exists + not revealed)
Generate PasswordResetToken (2h expiry)
    ↓
Queue EmailQueue record (Pending status)
    ↓
User clicks link in email → ResetPasswordAsync(email, token, newPassword)
    ↓
Token validated (not expired, not used)
    ↓
Password hashed, token marked used, redirect to login
```

## How It Works

### 1. Forgot Password (AuthService.ForgotPasswordAsync)

```csharp
public async Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request)
{
    var user = await _userRepository.GetByEmailAsync(request.Email, request.TenantId);
    
    // Always return Ok — don't reveal whether email exists (timing attack prevention)
    if (user == null) return Result.Ok();
    
    // Invalidate previous tokens for this user
    await _userRepository.InvalidatePreviousPasswordResetTokensAsync(user.UserId);
    
    // Create new reset token with 2-hour expiry
    var token = new PasswordResetToken
    {
        UserId = user.UserId,
        Token = _tokenService.GenerateRefreshToken(), // Secure random token
        TenantId = user.TenantId,
        ExpiresAt = DateTime.UtcNow.AddHours(2),
        CreatedAt = DateTime.UtcNow
    };
    
    await _userRepository.CreatePasswordResetTokenAsync(token);
    
    // Queue email for delivery
    var resetLink = $"{_configuration["App:BaseUrl"]}/account/reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token.Token)}";
    var emailQueue = new EmailQueue
    {
        ToEmail = user.Email,
        Subject = "Reset Your Password",
        Body = $"Click the link below to reset your password:\n\n{resetLink}\n\nThis link expires in 2 hours.",
        IsHtml = false,
        Status = "Pending",
        CreatedAt = DateTime.UtcNow,
        SendAttempts = 0,
        TenantId = user.TenantId
    };
    
    await _emailQueueRepository.EnqueueAsync(emailQueue);
    return Result.Ok();
}
```

**Key Points:**
- **Always returns `Ok()`** — don't reveal whether email exists (prevents user enumeration attacks)
- **One active token per user** — previous tokens are invalidated
- **2-hour expiry** — balances security with UX (long enough for user to check email)
- **Email queued, not sent** — asynchronous delivery via background job reads `EmailQueue` table

### 2. Reset Password (AuthService.ResetPasswordAsync)

```csharp
public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
{
    var user = await _userRepository.GetByEmailAsync(request.Email, request.TenantId);
    
    if (user == null)
        return Result.Fail(MessageKeys.Auth.PasswordResetInvalid);
    
    // Verify token exists, hasn't expired, and hasn't been used
    var token = await _userRepository.GetPasswordResetTokenAsync(user.UserId, request.Token, request.TenantId);
    
    if (token == null)
        return Result.Fail(MessageKeys.Auth.PasswordResetInvalid);
    
    // Update password and security stamp
    user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
    user.SecurityStamp = Guid.NewGuid().ToString(); // Invalidate existing sessions
    user.UpdatedAt = DateTime.UtcNow;
    
    await _userRepository.UpsertUserAsync(user);
    
    // Mark token as used
    await _userRepository.UsePasswordResetTokenAsync(token.PasswordResetTokenId);
    
    return Result.Ok();
}
```

**Key Points:**
- **Token validated** — repository checks expiry and used status
- **Security stamp updated** — logs out user from all devices
- **Generic error** — same message for invalid token and missing user (prevents enumeration)

## Email Queue Integration

The password reset flow relies on `IEmailQueueRepository` to queue emails in the database:

```csharp
var emailQueue = new EmailQueue
{
    ToEmail = recipient,
    Subject = subject,
    Body = body,
    IsHtml = false,
    Status = "Pending",      // Background job processes these
    CreatedAt = DateTime.UtcNow,
    SendAttempts = 0,        // Retries tracked here
    TenantId = user.TenantId // Multi-tenant support
};

await _emailQueueRepository.EnqueueAsync(emailQueue);
```

A background job (not shown here, implemented separately) runs periodically to:
1. Query `Shared.EmailQueue` for `Status = 'Pending'` records
2. Send via SMTP
3. Update `Status = 'Sent'` (or increment `SendAttempts` on failure)

## Testing Password Reset

### Scenario 1: Valid Reset
1. Register user (e.g., `john@example.com`)
2. Click "Forgot Password?" → enter email
3. Check database: `Shared.PasswordResetTokens` has new record with 2h expiry
4. Check database: `Shared.EmailQueue` has Pending record with reset link
5. Extract token from email body → navigate to reset page
6. Submit new password → logged out, can log in with new password

### Scenario 2: Expired Token
1. Create reset token, wait > 2 hours (or manually set `ExpiresAt` to past)
2. Try to reset → "Invalid or expired reset link"
3. Database: token still exists (for audit), but `ExpiresAt < UtcNow`

### Scenario 3: Reused Token
1. Reset password once → token marked used
2. Try same link again → "Invalid or expired reset link"

### Scenario 4: Non-Existent Email
1. Click "Forgot Password?" → enter non-existent email
2. Page shows "Check your email…" (even though email doesn't exist)
3. Database: no token created, no email queued
4. Prevents user enumeration

## Customization

### Change Token Expiry
In `AuthService.ForgotPasswordAsync`:
```csharp
ExpiresAt = DateTime.UtcNow.AddHours(4), // 4 hours instead of 2
```

### Customize Email Subject/Body
In `AuthService.ForgotPasswordAsync`:
```csharp
var emailQueue = new EmailQueue
{
    Subject = "Reset Your Account Password",
    Body = $"Hi {user.DisplayName},\n\nClick here to reset: {resetLink}\n\nExpires at: {DateTime.UtcNow.AddHours(2):yyyy-MM-dd HH:mm} UTC",
    // ...
};
```

### Use HTML Email
Change `IsHtml` and body format:
```csharp
var emailQueue = new EmailQueue
{
    Subject = "Reset Your Account Password",
    Body = $"<p>Click <a href='{resetLink}'>here</a> to reset your password.</p>",
    IsHtml = true,
    // ...
};
```

## Common Mistakes

❌ **Revealing whether email exists:** "Email not found" leaks users.  
✅ Always return `Ok()` for both cases.

❌ **Storing plain-text tokens in database:**  
✅ Use `GenerateRefreshToken()` which creates a cryptographically secure random string.

❌ **Long token expiry (> 24h):**  
✅ Use 2 hours (enough for user to check email, small window for brute force).

❌ **Not invalidating old tokens:**  
✅ Call `InvalidatePreviousPasswordResetTokensAsync()` to prevent multiple concurrent resets.

❌ **Forgetting to mark token used:**  
✅ Call `UsePasswordResetTokenAsync()` after successful reset to prevent replay.

## See Also
- [Translation System](01-translation-system.md) — for error messages like `PasswordResetInvalid`
- [Result Pattern](04-result-pattern.md) — understanding `Result` return type
- [AuthService](../../src/SmartWorkz.StarterKitMVC.Infrastructure/Services/AuthService.cs) — full implementation

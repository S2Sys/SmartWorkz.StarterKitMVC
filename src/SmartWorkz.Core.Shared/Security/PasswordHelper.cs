namespace SmartWorkz.Core.Shared.Security;

using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Provides secure password generation and validation using cryptographically secure random number generation.
/// </summary>
public sealed class PasswordHelper
{
    private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
    private const string DigitChars = "0123456789";
    private const string SpecialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

    /// <summary>
    /// Generates a cryptographically secure random password.
    /// </summary>
    /// <param name="length">Length of the password (8-128, default 12).</param>
    /// <param name="includeSpecialChars">Whether to include special characters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A Result containing the generated password or an error.</returns>
    public static ValueTask<Result<string>> GeneratePassword(
        int length = 12,
        bool includeSpecialChars = true,
        CancellationToken cancellationToken = default)
    {
        if (length < 8 || length > 128)
            return new ValueTask<Result<string>>(Result.Fail<string>("Password.InvalidLength", "Password length must be between 8 and 128 characters"));

        var chars = new StringBuilder();
        chars.Append(UppercaseChars);
        chars.Append(LowercaseChars);
        chars.Append(DigitChars);
        if (includeSpecialChars)
            chars.Append(SpecialChars);

        var charset = chars.ToString();
        var password = new char[length];

        // Ensure we have at least one character of each required type
        password[0] = GetRandomChar(UppercaseChars);
        password[1] = GetRandomChar(LowercaseChars);
        password[2] = GetRandomChar(DigitChars);

        int nextIndex = 3;
        if (includeSpecialChars)
        {
            password[nextIndex++] = GetRandomChar(SpecialChars);
        }

        // Fill the rest with random characters from the full charset
        for (int i = nextIndex; i < length; i++)
        {
            password[i] = GetRandomChar(charset);
        }

        // Shuffle the password
        password = Shuffle(password);

        return new ValueTask<Result<string>>(Result.Ok(new string(password)));
    }

    /// <summary>
    /// Validates the strength of a password against a policy.
    /// </summary>
    /// <param name="password">The password to validate.</param>
    /// <param name="policy">The policy to validate against (uses default if null).</param>
    /// <returns>A Result containing the validation result.</returns>
    public static Result<PasswordValidationResult> ValidateStrength(
        string? password,
        PasswordPolicy? policy = null)
    {
        if (string.IsNullOrEmpty(password))
            return Result.Fail<PasswordValidationResult>("Password.Required", "Password is required");

        policy ??= new PasswordPolicy();

        var failedRequirements = new List<string>();

        if (!CheckPasswordLength(password, policy))
            failedRequirements.Add($"Password must be at least {policy.MinLength} characters");

        if (policy.RequireUppercase && !CheckUppercase(password, policy))
            failedRequirements.Add("Password must contain at least one uppercase letter");

        if (policy.RequireLowercase && !CheckLowercase(password, policy))
            failedRequirements.Add("Password must contain at least one lowercase letter");

        if (policy.RequireNumbers && !CheckNumbers(password, policy))
            failedRequirements.Add("Password must contain at least one digit");

        if (policy.RequireSpecialChars && !CheckSpecialChars(password, policy))
            failedRequirements.Add($"Password must contain at least one special character from: {policy.AllowedSpecialChars}");

        var validationResult = new PasswordValidationResult
        {
            IsValid = failedRequirements.Count == 0,
            FailedRequirements = failedRequirements
        };

        return Result.Ok(validationResult);
    }

    /// <summary>
    /// Gets a random character from the specified character set using cryptographic randomness.
    /// </summary>
    private static char GetRandomChar(string charset)
    {
        using var rng = RandomNumberGenerator.Create();
        byte[] buffer = new byte[1];
        int index;
        do
        {
            rng.GetBytes(buffer);
            index = buffer[0] % charset.Length;
        } while (buffer[0] - index > 255 - charset.Length); // Avoid bias

        return charset[index];
    }

    /// <summary>
    /// Performs Fisher-Yates shuffle on the character array.
    /// </summary>
    private static char[] Shuffle(char[] chars)
    {
        var shuffled = (char[])chars.Clone();
        using var rng = RandomNumberGenerator.Create();

        for (int i = shuffled.Length - 1; i > 0; i--)
        {
            byte[] buffer = new byte[4];
            rng.GetBytes(buffer);
            int randomIndex = Math.Abs(BitConverter.ToInt32(buffer, 0)) % (i + 1);

            (shuffled[i], shuffled[randomIndex]) = (shuffled[randomIndex], shuffled[i]);
        }

        return shuffled;
    }

    /// <summary>Checks if password meets minimum length requirement.</summary>
    private static bool CheckPasswordLength(string password, PasswordPolicy policy)
        => password.Length >= policy.MinLength && password.Length <= policy.MaxLength;

    /// <summary>Checks if password contains at least one uppercase letter.</summary>
    private static bool CheckUppercase(string password, PasswordPolicy policy)
        => !policy.RequireUppercase || password.Any(char.IsUpper);

    /// <summary>Checks if password contains at least one lowercase letter.</summary>
    private static bool CheckLowercase(string password, PasswordPolicy policy)
        => !policy.RequireLowercase || password.Any(char.IsLower);

    /// <summary>Checks if password contains at least one digit.</summary>
    private static bool CheckNumbers(string password, PasswordPolicy policy)
        => !policy.RequireNumbers || password.Any(char.IsDigit);

    /// <summary>Checks if password contains at least one special character.</summary>
    private static bool CheckSpecialChars(string password, PasswordPolicy policy)
        => !policy.RequireSpecialChars || password.Any(c => policy.AllowedSpecialChars.Contains(c));
}

/// <summary>Policy for password validation requirements.</summary>
public sealed class PasswordPolicy
{
    /// <summary>Minimum password length (1-128).</summary>
    public int MinLength { get; set; } = 8;

    /// <summary>Maximum password length (1-128).</summary>
    public int MaxLength { get; set; } = 128;

    /// <summary>Whether to require at least one uppercase letter.</summary>
    public bool RequireUppercase { get; set; } = true;

    /// <summary>Whether to require at least one lowercase letter.</summary>
    public bool RequireLowercase { get; set; } = true;

    /// <summary>Whether to require at least one digit.</summary>
    public bool RequireNumbers { get; set; } = true;

    /// <summary>Whether to require at least one special character.</summary>
    public bool RequireSpecialChars { get; set; } = false;

    /// <summary>Allowed special characters for validation.</summary>
    public string AllowedSpecialChars { get; set; } = "!@#$%^&*()_+-=[]{}|;:,.<>?";
}

/// <summary>Result of password validation against a policy.</summary>
public sealed class PasswordValidationResult
{
    /// <summary>Whether the password is valid according to the policy.</summary>
    public bool IsValid { get; set; }

    /// <summary>List of human-readable error messages for failed requirements.</summary>
    public List<string> FailedRequirements { get; set; } = new();
}

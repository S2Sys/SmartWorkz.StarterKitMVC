using System;
using System.Security.Cryptography;
using System.Text;

// Quick hash generator for test passwords
// Usage: dotnet run -- "TestPassword123!"

class Program
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    static void Main(string[] args)
    {
        string password = args.Length > 0 ? args[0] : "TestPassword123!";

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            Algorithm,
            HashSize
        );

        string passwordHash = $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";

        Console.WriteLine($"Password: {password}");
        Console.WriteLine($"Hash: {passwordHash}");
        Console.WriteLine();
        Console.WriteLine("SQL INSERT for all test users with this password:");
        Console.WriteLine($"'{passwordHash}'");
    }
}

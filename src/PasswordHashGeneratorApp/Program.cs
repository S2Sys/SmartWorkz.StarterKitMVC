using System;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main()
    {
        const int SaltSize = 16;
        const int HashSize = 32;
        const int Iterations = 100000;
        var Algorithm = HashAlgorithmName.SHA256;

        string password = "Enter321";

        // Generate random salt
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        // PBKDF2-SHA256
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            Algorithm,
            HashSize
        );

        // Format: salt.hash (Base64)
        string saltB64 = Convert.ToBase64String(salt);
        string hashB64 = Convert.ToBase64String(hash);
        string passwordHash = $"{saltB64}.{hashB64}";

        Console.WriteLine("=============================================================");
        Console.WriteLine("REAL PBKDF2-SHA256 PASSWORD HASH");
        Console.WriteLine("=============================================================");
        Console.WriteLine("");
        Console.WriteLine($"Password: {password}");
        Console.WriteLine($"Algorithm: PBKDF2-SHA256");
        Console.WriteLine($"Iterations: 100000");
        Console.WriteLine("");
        Console.WriteLine($"Salt (B64): {saltB64}");
        Console.WriteLine($"Salt Length: {saltB64.Length} chars");
        Console.WriteLine("");
        Console.WriteLine($"Hash (B64): {hashB64}");
        Console.WriteLine($"Hash Length: {hashB64.Length} chars");
        Console.WriteLine("");
        Console.WriteLine($"Full Hash: {passwordHash}");
        Console.WriteLine($"Total Length: {passwordHash.Length} chars ✓");
        Console.WriteLine("");
        Console.WriteLine("=============================================================");
        Console.WriteLine("SQL UPDATE COMMAND:");
        Console.WriteLine("=============================================================");
        Console.WriteLine("");
        Console.WriteLine($"UPDATE Auth.Users");
        Console.WriteLine($"SET PasswordHash = '{passwordHash}'");
        Console.WriteLine($"WHERE Email IN (");
        Console.WriteLine($"    'admin@smartworkz.test',");
        Console.WriteLine($"    'manager@smartworkz.test',");
        Console.WriteLine($"    'staff@smartworkz.test',");
        Console.WriteLine($"    'customer@smartworkz.test'");
        Console.WriteLine($");");
        Console.WriteLine("");
        Console.WriteLine("=============================================================");
        Console.WriteLine("Copy the hash above and run the SQL UPDATE command");
        Console.WriteLine("=============================================================");
    }
}

#!/usr/bin/env dotnet-script
using System;
using System.Security.Cryptography;
using System.Text;

// ============================================
// Generate PBKDF2-SHA256 Hash for TestPassword123!
// ============================================

const int SaltSize = 16;
const int HashSize = 32;
const int Iterations = 100_000;
var Algorithm = HashAlgorithmName.SHA256;

string password = "TestPassword123!";

// Generate random salt
var salt = RandomNumberGenerator.GetBytes(SaltSize);

// Derive hash using PBKDF2
var hash = Rfc2898DeriveBytes.Pbkdf2(
    Encoding.UTF8.GetBytes(password),
    salt,
    Iterations,
    Algorithm,
    HashSize
);

// Format: salt.hash (Base64 encoded)
string saltB64 = Convert.ToBase64String(salt);
string hashB64 = Convert.ToBase64String(hash);
string passwordHash = $"{saltB64}.{hashB64}";

Console.WriteLine("============================================================");
Console.WriteLine("PBKDF2-SHA256 Password Hash Generator");
Console.WriteLine("============================================================");
Console.WriteLine("");
Console.WriteLine($"Password: {password}");
Console.WriteLine($"Algorithm: PBKDF2-SHA256");
Console.WriteLine($"Iterations: {Iterations:N0}");
Console.WriteLine($"Salt Size: {SaltSize} bytes");
Console.WriteLine($"Hash Size: {HashSize} bytes");
Console.WriteLine("");
Console.WriteLine("Generated Hash:");
Console.WriteLine("");
Console.WriteLine(passwordHash);
Console.WriteLine("");
Console.WriteLine("============================================================");
Console.WriteLine("SQL Command to Update All Test Users:");
Console.WriteLine("============================================================");
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
Console.WriteLine("============================================================");
Console.WriteLine("Or individual INSERT in seed script:");
Console.WriteLine("============================================================");
Console.WriteLine("");
Console.WriteLine($"DECLARE @PasswordHash NVARCHAR(MAX) = '{passwordHash}'");
Console.WriteLine("");
Console.WriteLine("INSERT INTO Auth.Users (...) VALUES");
Console.WriteLine("    (..., @PasswordHash, ...),");
Console.WriteLine("    (..., @PasswordHash, ...),");
Console.WriteLine("    (..., @PasswordHash, ...),");
Console.WriteLine("    (..., @PasswordHash, ...);");

#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Generate PBKDF2 password hash for test users
.DESCRIPTION
    Creates correct password hash using PBKDF2-SHA256 with 100,000 iterations
    Matches the format used by PasswordHasher in the application
#>

param(
    [string]$Password = "TestPassword123!"
)

# Import cryptography
$salt = [byte[]]::new(16)
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($salt)

$pbkdf2 = New-Object System.Security.Cryptography.Rfc2898DeriveBytes(
    $Password,
    $salt,
    100000,
    [System.Security.Cryptography.HashAlgorithmName]::SHA256
)

$hash = $pbkdf2.GetBytes(32)

# Format: salt.hash (Base64 encoded)
$saltB64 = [Convert]::ToBase64String($salt)
$hashB64 = [Convert]::ToBase64String($hash)
$passwordHash = "$saltB64.$hashB64"

Write-Host "Password: $Password"
Write-Host "Hash: $passwordHash"
Write-Host ""
Write-Host "Use this in SQL INSERT:"
Write-Host "'$passwordHash'"

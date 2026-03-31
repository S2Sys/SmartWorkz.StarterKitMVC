#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Generate PBKDF2 password hashes matching PasswordHasher implementation
.DESCRIPTION
    Creates PBKDF2-SHA256 password hashes with 100,000 iterations
    Format: salt.hash (Base64 encoded, matching C# PasswordHasher)
    Used to seed test users in database
.EXAMPLE
    .\GeneratePasswordHashes.ps1 "TestPassword123!" | Set-Clipboard
#>

param(
    [string]$Password = "TestPassword123!",
    [int]$Count = 1
)

# ============================================
# Configuration (must match PasswordHasher.cs)
# ============================================
$SaltSize = 16
$HashSize = 32
$Iterations = 100000
$Algorithm = [System.Security.Cryptography.HashAlgorithmName]::SHA256

Write-Host "============================================================"
Write-Host "[INFO] Generating PBKDF2-SHA256 Password Hashes"
Write-Host "============================================================"
Write-Host ""
Write-Host "Configuration:"
Write-Host "  Password: $Password"
Write-Host "  Algorithm: PBKDF2-SHA256"
Write-Host "  Iterations: $Iterations"
Write-Host "  Salt Size: $SaltSize bytes"
Write-Host "  Hash Size: $HashSize bytes"
Write-Host ""

# ============================================
# Generate hashes
# ============================================
$hashes = @()

for ($i = 0; $i -lt $Count; $i++) {
    # Generate random salt
    $salt = [byte[]]::new($SaltSize)
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $rng.GetBytes($salt)

    # Derive hash using PBKDF2
    $pbkdf2 = New-Object System.Security.Cryptography.Rfc2898DeriveBytes(
        $Password,
        $salt,
        $Iterations,
        $Algorithm
    )

    $hash = $pbkdf2.GetBytes($HashSize)

    # Format: salt.hash (Base64 encoded)
    $saltB64 = [Convert]::ToBase64String($salt)
    $hashB64 = [Convert]::ToBase64String($hash)
    $passwordHash = "$saltB64.$hashB64"

    $hashes += $passwordHash

    Write-Host "[Hash $($i+1)]"
    Write-Host "  Salt (B64): $saltB64"
    Write-Host "  Hash (B64): $hashB64"
    Write-Host "  Complete: $passwordHash"
    Write-Host ""
}

# ============================================
# Output results
# ============================================
Write-Host "============================================================"
Write-Host "[OUTPUT] Use these hashes in SQL INSERT:"
Write-Host "============================================================"
Write-Host ""

for ($i = 0; $i -lt $hashes.Count; $i++) {
    Write-Host "DECLARE @PasswordHash$($i+1) NVARCHAR(MAX) = '$($hashes[$i])'"
}

Write-Host ""
Write-Host "[COPY] All hashes (for easy copy-paste):"
Write-Host ""

$hashes | ForEach-Object {
    Write-Host "'$_',"
}

Write-Host ""
Write-Host "[INFO] Replace PLACEHOLDER_HASH values in 008_SeedTestUsers.sql with actual hashes"
Write-Host "[INFO] All test users can use the same hash if password is identical"

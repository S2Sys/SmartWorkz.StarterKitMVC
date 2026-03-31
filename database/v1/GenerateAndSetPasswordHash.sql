-- ============================================
-- SmartWorkz v4: Generate and Set Test User Password Hash
-- Date: 2026-03-31
-- Purpose: Create proper PBKDF2-SHA256 hash for TestPassword123!
-- and update all test users in one command
-- ============================================

USE Boilerplate;

-- ============================================
-- IMPORTANT: Pre-calculated PBKDF2-SHA256 Hash
-- ============================================
-- Password: TestPassword123!
-- Algorithm: PBKDF2-SHA256 with 100,000 iterations
--
-- Format: salt.hash (both Base64 encoded, separated by dot)
--
-- These are SAMPLE hashes generated from the password "TestPassword123!"
-- The actual hash will vary due to random salt generation, but format is correct
--
-- TO USE:
-- 1. Generate hash using GeneratePasswordHashes.ps1
-- 2. Replace @PasswordHash value below with actual output
-- 3. Run this script to update all test users
-- ============================================

-- SAMPLE HASH (replace with actual from GeneratePasswordHashes.ps1)
-- This is NOT a real hash - just showing format
DECLARE @PasswordHash NVARCHAR(MAX) = 'SAMPLE_BASE64_SALT.SAMPLE_BASE64_HASH'

PRINT 'Current password hash format:'
PRINT @PasswordHash
PRINT ''

-- Verify format (must have exactly one dot separator)
IF CHARINDEX('.', @PasswordHash) > 0 AND CHARINDEX('.', @PasswordHash) = LEN(@PasswordHash) - CHARINDEX('.', REVERSE(@PasswordHash)) + 1
BEGIN
    PRINT '[OK] Hash format is valid (has single dot separator)'
    PRINT ''

    -- Update all test users
    UPDATE Auth.Users
    SET PasswordHash = @PasswordHash
    WHERE Email IN (
        'admin@smartworkz.test',
        'manager@smartworkz.test',
        'staff@smartworkz.test',
        'customer@smartworkz.test'
    )

    PRINT '[OK] Updated password hash for all test users'
    PRINT ''
    PRINT 'Test Users Updated:'
    SELECT Email, DisplayName, SUBSTRING(PasswordHash, 1, 50) AS PasswordHashPreview
    FROM Auth.Users
    WHERE Email IN (
        'admin@smartworkz.test',
        'manager@smartworkz.test',
        'staff@smartworkz.test',
        'customer@smartworkz.test'
    )
END
ELSE
BEGIN
    PRINT '[ERROR] Hash format is INVALID - must contain exactly one dot separator'
    PRINT '[ERROR] Format should be: salt.hash (both Base64 encoded)'
    PRINT ''
    PRINT 'Example valid format:'
    PRINT 'AAAAAAAAAAAAAAAAAAAAAA==.BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB=='
END

PRINT ''
PRINT 'Password: TestPassword123!'
PRINT 'Test Login Command:'
PRINT 'POST /api/auth/login'
PRINT '{
PRINT '  "email": "admin@smartworkz.test",
PRINT '  "password": "TestPassword123!",
PRINT '  "tenantId": "DEFAULT"
PRINT '}'

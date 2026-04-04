-- ============================================
-- Update Test User Passwords
-- Password: TestPassword123!
-- Hash: k23Gu+N1T4pqRO1hJHpuzw==.iiB/92EnS507sbn/96mQi6ZDMobfcsU6SVFN2sdLc2w=
-- ============================================

USE Boilerplate;

-- Update test users with correct password hash
UPDATE Auth.Users
SET PasswordHash = 'k23Gu+N1T4pqRO1hJHpuzw==.iiB/92EnS507sbn/96mQi6ZDMobfcsU6SVFN2sdLc2w='
WHERE NormalizedEmail IN (
    'ADMIN@SMARTWORKZ.TEST',
    'MANAGER@SMARTWORKZ.TEST',
    'STAFF@SMARTWORKZ.TEST',
    'CUSTOMER@SMARTWORKZ.TEST'
);

PRINT 'Updated ' + CAST(@@ROWCOUNT AS VARCHAR) + ' test users with correct password hash.';

-- Verify the update
SELECT 'Email' AS Field, Email AS Value FROM Auth.Users
WHERE NormalizedEmail = 'ADMIN@SMARTWORKZ.TEST'
UNION ALL
SELECT 'PasswordHash', PasswordHash FROM Auth.Users
WHERE NormalizedEmail = 'ADMIN@SMARTWORKZ.TEST';

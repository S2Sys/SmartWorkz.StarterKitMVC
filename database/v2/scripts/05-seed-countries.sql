-- ============================================
-- V2: Seed Countries (Global, IDs 51-100)
-- ============================================
-- Purpose: Seed global country lookups
-- Scope: IsGlobalScope = 1, TenantId = NULL

SET NOCOUNT ON

DECLARE @Now DATETIME2 = GETUTCDATE()

INSERT INTO LoV.LovItems (IntId, Id, CategoryKey, Key, DisplayName, TenantId, IsGlobalScope, IsActive, CreatedAt, CreatedBy, SortOrder)
VALUES
    (51, NEWID(), 'countries', 'US', 'United States', NULL, 1, 1, @Now, 'system', 1),
    (52, NEWID(), 'countries', 'CA', 'Canada', NULL, 1, 1, @Now, 'system', 2),
    (53, NEWID(), 'countries', 'MX', 'Mexico', NULL, 1, 1, @Now, 'system', 3),
    (54, NEWID(), 'countries', 'GB', 'United Kingdom', NULL, 1, 1, @Now, 'system', 4),
    (55, NEWID(), 'countries', 'FR', 'France', NULL, 1, 1, @Now, 'system', 5),
    (56, NEWID(), 'countries', 'DE', 'Germany', NULL, 1, 1, @Now, 'system', 6),
    (57, NEWID(), 'countries', 'IT', 'Italy', NULL, 1, 1, @Now, 'system', 7),
    (58, NEWID(), 'countries', 'ES', 'Spain', NULL, 1, 1, @Now, 'system', 8),
    (59, NEWID(), 'countries', 'JP', 'Japan', NULL, 1, 1, @Now, 'system', 9),
    (60, NEWID(), 'countries', 'CN', 'China', NULL, 1, 1, @Now, 'system', 10),
    (61, NEWID(), 'countries', 'IN', 'India', NULL, 1, 1, @Now, 'system', 11),
    (62, NEWID(), 'countries', 'BR', 'Brazil', NULL, 1, 1, @Now, 'system', 12),
    (63, NEWID(), 'countries', 'AU', 'Australia', NULL, 1, 1, @Now, 'system', 13),
    (64, NEWID(), 'countries', 'ZA', 'South Africa', NULL, 1, 1, @Now, 'system', 14),
    (65, NEWID(), 'countries', 'SG', 'Singapore', NULL, 1, 1, @Now, 'system', 15)

PRINT @@ROWCOUNT & ' country records inserted.'

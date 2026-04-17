-- ============================================
-- V2: Seed TimeZones (Global, IDs 1-50)
-- ============================================
-- Purpose: Seed global timezone lookups
-- Scope: IsGlobalScope = 1, TenantId = NULL

SET NOCOUNT ON

DECLARE @Now DATETIME2 = GETUTCDATE()

-- Truncate existing timezones (optional - uncomment to reset)
-- DELETE FROM LoV.LovItems WHERE CategoryKey = 'timezones'

INSERT INTO LoV.LovItems (IntId, Id, CategoryKey, Key, DisplayName, TenantId, IsGlobalScope, IsActive, CreatedAt, CreatedBy, SortOrder, Metadata)
VALUES
    (1, NEWID(), 'timezones', 'America/New_York', 'Eastern Time (US & Canada)', NULL, 1, 1, @Now, 'system', 1, '{"standardName":"EST","offsetHours":-5}'),
    (2, NEWID(), 'timezones', 'America/Chicago', 'Central Time (US & Canada)', NULL, 1, 1, @Now, 'system', 2, '{"standardName":"CST","offsetHours":-6}'),
    (3, NEWID(), 'timezones', 'America/Denver', 'Mountain Time (US & Canada)', NULL, 1, 1, @Now, 'system', 3, '{"standardName":"MST","offsetHours":-7}'),
    (4, NEWID(), 'timezones', 'America/Los_Angeles', 'Pacific Time (US & Canada)', NULL, 1, 1, @Now, 'system', 4, '{"standardName":"PST","offsetHours":-8}'),
    (5, NEWID(), 'timezones', 'America/Anchorage', 'Alaska', NULL, 1, 1, @Now, 'system', 5, '{"standardName":"AKST","offsetHours":-9}'),
    (6, NEWID(), 'timezones', 'Pacific/Honolulu', 'Hawaii', NULL, 1, 1, @Now, 'system', 6, '{"standardName":"HST","offsetHours":-10}'),
    (7, NEWID(), 'timezones', 'Europe/London', 'GMT (UK)', NULL, 1, 1, @Now, 'system', 7, '{"standardName":"GMT","offsetHours":0}'),
    (8, NEWID(), 'timezones', 'Europe/Paris', 'Central European Time', NULL, 1, 1, @Now, 'system', 8, '{"standardName":"CET","offsetHours":1}'),
    (9, NEWID(), 'timezones', 'Europe/Berlin', 'Berlin, Germany', NULL, 1, 1, @Now, 'system', 9, '{"standardName":"CET","offsetHours":1}'),
    (10, NEWID(), 'timezones', 'Asia/Tokyo', 'Japan Standard Time', NULL, 1, 1, @Now, 'system', 10, '{"standardName":"JST","offsetHours":9}'),
    (11, NEWID(), 'timezones', 'Asia/Shanghai', 'China Standard Time', NULL, 1, 1, @Now, 'system', 11, '{"standardName":"CST","offsetHours":8}'),
    (12, NEWID(), 'timezones', 'Asia/Dubai', 'Gulf Standard Time', NULL, 1, 1, @Now, 'system', 12, '{"standardName":"GST","offsetHours":4}'),
    (13, NEWID(), 'timezones', 'Asia/Kolkata', 'India Standard Time', NULL, 1, 1, @Now, 'system', 13, '{"standardName":"IST","offsetHours":5.5}'),
    (14, NEWID(), 'timezones', 'Australia/Sydney', 'Australian Eastern Time', NULL, 1, 1, @Now, 'system', 14, '{"standardName":"AEDT","offsetHours":11}'),
    (15, NEWID(), 'timezones', 'Australia/Melbourne', 'Melbourne, Australia', NULL, 1, 1, @Now, 'system', 15, '{"standardName":"AEDT","offsetHours":11}')

PRINT @@ROWCOUNT & ' timezone records inserted.'

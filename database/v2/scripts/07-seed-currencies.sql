-- ============================================
-- V2: Seed Currencies (Global, IDs 201-300)
-- ============================================
-- Purpose: Seed global currency lookups
-- Scope: IsGlobalScope = 1, TenantId = NULL

SET NOCOUNT ON

DECLARE @Now DATETIME2 = GETUTCDATE()

INSERT INTO LoV.LovItems (IntId, Id, CategoryKey, Key, DisplayName, TenantId, IsGlobalScope, IsActive, CreatedAt, CreatedBy, SortOrder, Metadata)
VALUES
    (201, NEWID(), 'currencies', 'USD', 'US Dollar', NULL, 1, 1, @Now, 'system', 1, '{"symbol":"$","decimalPlaces":2}'),
    (202, NEWID(), 'currencies', 'EUR', 'Euro', NULL, 1, 1, @Now, 'system', 2, '{"symbol":"€","decimalPlaces":2}'),
    (203, NEWID(), 'currencies', 'GBP', 'British Pound', NULL, 1, 1, @Now, 'system', 3, '{"symbol":"£","decimalPlaces":2}'),
    (204, NEWID(), 'currencies', 'JPY', 'Japanese Yen', NULL, 1, 1, @Now, 'system', 4, '{"symbol":"¥","decimalPlaces":0}'),
    (205, NEWID(), 'currencies', 'CNY', 'Chinese Yuan', NULL, 1, 1, @Now, 'system', 5, '{"symbol":"¥","decimalPlaces":2}'),
    (206, NEWID(), 'currencies', 'INR', 'Indian Rupee', NULL, 1, 1, @Now, 'system', 6, '{"symbol":"₹","decimalPlaces":2}'),
    (207, NEWID(), 'currencies', 'CAD', 'Canadian Dollar', NULL, 1, 1, @Now, 'system', 7, '{"symbol":"$","decimalPlaces":2}'),
    (208, NEWID(), 'currencies', 'AUD', 'Australian Dollar', NULL, 1, 1, @Now, 'system', 8, '{"symbol":"$","decimalPlaces":2}'),
    (209, NEWID(), 'currencies', 'CHF', 'Swiss Franc', NULL, 1, 1, @Now, 'system', 9, '{"symbol":"Fr","decimalPlaces":2}'),
    (210, NEWID(), 'currencies', 'BRL', 'Brazilian Real', NULL, 1, 1, @Now, 'system', 10, '{"symbol":"R$","decimalPlaces":2}'),
    (211, NEWID(), 'currencies', 'MXN', 'Mexican Peso', NULL, 1, 1, @Now, 'system', 11, '{"symbol":"$","decimalPlaces":2}'),
    (212, NEWID(), 'currencies', 'SGD', 'Singapore Dollar', NULL, 1, 1, @Now, 'system', 12, '{"symbol":"$","decimalPlaces":2}'),
    (213, NEWID(), 'currencies', 'HKD', 'Hong Kong Dollar', NULL, 1, 1, @Now, 'system', 13, '{"symbol":"$","decimalPlaces":2}'),
    (214, NEWID(), 'currencies', 'SEK', 'Swedish Krona', NULL, 1, 1, @Now, 'system', 14, '{"symbol":"kr","decimalPlaces":2}'),
    (215, NEWID(), 'currencies', 'ZAR', 'South African Rand', NULL, 1, 1, @Now, 'system', 15, '{"symbol":"R","decimalPlaces":2}')

PRINT @@ROWCOUNT & ' currency records inserted.'

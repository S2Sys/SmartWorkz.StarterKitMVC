-- ============================================
-- V2: Seed Languages (Global, IDs 101-200)
-- ============================================
-- Purpose: Seed global language lookups
-- Scope: IsGlobalScope = 1, TenantId = NULL

SET NOCOUNT ON

DECLARE @Now DATETIME2 = GETUTCDATE()

INSERT INTO LoV.LovItems (IntId, Id, CategoryKey, Key, DisplayName, TenantId, IsGlobalScope, IsActive, CreatedAt, CreatedBy, SortOrder, Metadata, LocalizedNames)
VALUES
    (101, NEWID(), 'languages', 'en-US', 'English (US)', NULL, 1, 1, @Now, 'system', 1, '{"nativeName":"English","isDefault":true}', '{"en-US":"English","es-ES":"Inglés"}'),
    (102, NEWID(), 'languages', 'en-GB', 'English (UK)', NULL, 1, 1, @Now, 'system', 2, '{"nativeName":"English","isDefault":false}', '{"en-US":"English (UK)","es-ES":"Inglés (RU)"}'),
    (103, NEWID(), 'languages', 'fr-FR', 'Français', NULL, 1, 1, @Now, 'system', 3, '{"nativeName":"Français","isDefault":false}', '{"en-US":"French","es-ES":"Francés"}'),
    (104, NEWID(), 'languages', 'de-DE', 'Deutsch', NULL, 1, 1, @Now, 'system', 4, '{"nativeName":"Deutsch","isDefault":false}', '{"en-US":"German","es-ES":"Alemán"}'),
    (105, NEWID(), 'languages', 'es-ES', 'Español', NULL, 1, 1, @Now, 'system', 5, '{"nativeName":"Español","isDefault":false}', '{"en-US":"Spanish","es-ES":"Español"}'),
    (106, NEWID(), 'languages', 'it-IT', 'Italiano', NULL, 1, 1, @Now, 'system', 6, '{"nativeName":"Italiano","isDefault":false}', '{"en-US":"Italian","es-ES":"Italiano"}'),
    (107, NEWID(), 'languages', 'ja-JP', '日本語', NULL, 1, 1, @Now, 'system', 7, '{"nativeName":"日本語","isDefault":false}', '{"en-US":"Japanese","es-ES":"Japonés"}'),
    (108, NEWID(), 'languages', 'zh-CN', '中文(简体)', NULL, 1, 1, @Now, 'system', 8, '{"nativeName":"中文","isDefault":false}', '{"en-US":"Chinese (Simplified)","es-ES":"Chino (Simplificado)"}'),
    (109, NEWID(), 'languages', 'zh-TW', '中文(繁體)', NULL, 1, 1, @Now, 'system', 9, '{"nativeName":"中文","isDefault":false}', '{"en-US":"Chinese (Traditional)","es-ES":"Chino (Tradicional)"}'),
    (110, NEWID(), 'languages', 'hi-IN', 'हिन्दी', NULL, 1, 1, @Now, 'system', 10, '{"nativeName":"हिन्दी","isDefault":false}', '{"en-US":"Hindi","es-ES":"Hindi"}'),
    (111, NEWID(), 'languages', 'pt-BR', 'Português (Brasil)', NULL, 1, 1, @Now, 'system', 11, '{"nativeName":"Português","isDefault":false}', '{"en-US":"Portuguese (Brazil)","es-ES":"Portugués (Brasil)"}'),
    (112, NEWID(), 'languages', 'pt-PT', 'Português (Portugal)', NULL, 1, 1, @Now, 'system', 12, '{"nativeName":"Português","isDefault":false}', '{"en-US":"Portuguese (Portugal)","es-ES":"Portugués (Portugal)"}'),
    (113, NEWID(), 'languages', 'ru-RU', 'Русский', NULL, 1, 1, @Now, 'system', 13, '{"nativeName":"Русский","isDefault":false}', '{"en-US":"Russian","es-ES":"Ruso"}'),
    (114, NEWID(), 'languages', 'ko-KR', '한국어', NULL, 1, 1, @Now, 'system', 14, '{"nativeName":"한국어","isDefault":false}', '{"en-US":"Korean","es-ES":"Coreano"}'),
    (115, NEWID(), 'languages', 'ar-SA', 'العربية', NULL, 1, 1, @Now, 'system', 15, '{"nativeName":"العربية","isDefault":false}', '{"en-US":"Arabic","es-ES":"Árabe"}')

PRINT @@ROWCOUNT & ' language records inserted.'

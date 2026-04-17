# List of Values (LoV) Consolidation Design

**Date:** 2026-04-17  
**Status:** Approved  
**Scope:** Consolidate master lookups into unified LoV system with hierarchical tenant support and integer ID allocation

---

## Overview

Consolidate fragmented master lookup tables (Currencies, Languages, TimeZones, Countries) into the existing LoV (List of Values) system. Add support for:
- Integer IDs (1-999 reserved for system lookups)
- Hierarchical tenant inheritance (parent tenants auto-inherit to sub-tenants)
- Active/Inactive status management
- Metadata storage for lookup-specific data (e.g., currency symbols, timezone offsets)

---

## How LoV Works (Wiki)

### Concept

**LoV (List of Values)** is a unified lookup system for managing reference data across the application. Instead of creating separate tables for each type of reference data (Currencies, Languages, TimeZones), LoV consolidates them into a single, flexible table.

### Key Principles

1. **Category-based Organization**
   - Lookups are organized by `CategoryKey` (e.g., "currencies", "languages", "timezones")
   - Each category can have sub-categories via `SubCategoryKey`
   - Example: Category "locations" → SubCategory "north-america" → Items (US, Canada, Mexico)

2. **Hierarchical Tenant Inheritance**
   - **Global scope** (TenantId = null, IsGlobalScope = true): Available to all tenants
   - **Parent tenant scope** (TenantId = "ABC", IsGlobalScope = false): Defined by parent, inherited by all sub-tenants
   - **Auto-inheritance**: Sub-tenant "ABC-US" automatically inherits items from parent "ABC" and global scope
   - No override—sub-tenants get parent's items as-is (can be disabled per sub-tenant via IsActive = false)

3. **Status Management**
   - `IsActive = true`: Lookup is available for use
   - `IsActive = false`: Lookup is hidden (can be re-enabled without re-entering data)
   - `IsDeleted = true`: Permanent logical deletion (soft delete)

4. **Flexible Metadata Storage**
   - Lookup-specific data stored in `Metadata` JSON field
   - Examples:
     - Currency: `{ "symbol": "$", "decimalPlaces": 2 }`
     - Language: `{ "nativeName": "English", "isDefault": true }`
     - TimeZone: `{ "standardName": "EST", "offsetHours": -5 }`

### Query Example

Get all active currencies for tenant "ABC-US":
```sql
SELECT * FROM LoV.LovItems
WHERE CategoryKey = 'currencies'
  AND IsActive = true
  AND IsDeleted = false
  AND (IsGlobalScope = true OR TenantId = 'ABC' OR TenantId = 'ABC-US')
ORDER BY SortOrder, DisplayName
```

Returns:
- Global currencies (USD, EUR, GBP from IsGlobalScope = true)
- Parent tenant currencies (JPY, CNY from TenantId = 'ABC')
- Sub-tenant currencies (INR from TenantId = 'ABC-US')

### Use Cases

| Scenario | Implementation |
|----------|----------------|
| Dropdown: Select currency | Query by CategoryKey, filter by tenant scope |
| Admin: Manage currencies | UPSERT by IntId or (CategoryKey, Key, TenantId) |
| Report: Currency conversion | Look up metadata for symbol/decimals |
| Localization: Currency name | Query LocalizedNames by culture code |

---

## Current State

**Existing LoV System:**
- `LovItem` - List value with CategoryKey, Key, DisplayName, Tags, LocalizedNames
- `Category` - Lookup categories with optional ParentKey for nesting
- `SubCategory` - Sub-groupings within categories
- GUID-based IDs only

**Fragmented Master Tables:**
| Table | Fields | Status |
|-------|--------|--------|
| `Currencies` | Code, Name, Symbol, DecimalPlaces | Separate table |
| `Languages` | Code, Name, DisplayName, NativeName, IsDefault | Separate table |
| `TimeZones` | Identifier, DisplayName, StandardName, OffsetHours | Separate table |
| `Countries` | Code, Name | Separate table |

**Issues:**
- Redundant structures (all have Code/Name/DisplayName pattern)
- No unified integer ID system
- No consistent active/inactive management
- Difficult to add new lookup types without creating new tables

---

## Target State

### Enhanced LovItem Structure

```csharp
public sealed class LovItem
{
    // Primary identifiers
    public int IntId { get; init; }              // 1-999 reserved for system (nullable for tenant lookups)
    public Guid Id { get; init; }                // Keep existing GUID for compatibility
    
    // Category & Keys
    public string CategoryKey { get; init; }     // "currencies", "languages", "timezones", "countries"
    public string? SubCategoryKey { get; init; } // Optional subcategory
    public string Key { get; init; }             // "USD", "en-US", "America/New_York", "US"
    
    // Display & Localization
    public string DisplayName { get; init; }
    public IReadOnlyDictionary<string, string> LocalizedNames { get; init; }
    
    // Tenant & Scope
    public string? TenantId { get; init; }       // null = global, "ABC" = tenant-specific, "ABC-US" = sub-tenant
    public bool IsGlobalScope { get; init; }     // true = global, false = tenant-scoped
    
    // Status Management
    public bool IsActive { get; init; } = true;
    public DateTime CreatedAt { get; init; }
    public string CreatedBy { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public string UpdatedBy { get; init; }
    public bool IsDeleted { get; init; }
    
    // Metadata (JSON for lookup-specific data)
    public Dictionary<string, object> Metadata { get; init; } // e.g., { "Symbol": "$", "DecimalPlaces": 2 }
    
    // Sorting & Tags
    public int SortOrder { get; init; }
    public IReadOnlyCollection<string> Tags { get; init; }
}
```

### ID Allocation Strategy

| Range | Type | Usage | Scope |
|-------|------|-------|-------|
| 1-100 | System | TimeZones, Countries (global reference data) | Global (IsGlobalScope = true) |
| 101-200 | System | Languages (global defaults) | Global (IsGlobalScope = true) |
| 201-300 | System | Currencies (global defaults) | Global (IsGlobalScope = true) |
| 301-999 | Reserved | Future system lookups | Global or Tenant-scoped |

---

## Consolidation Plan

### 1. Currency Migration
**Current Table:** `Master.Currencies`
**Target:** `LoV.LovItem` with CategoryKey = "currencies"

| Source Field | Target Field | Metadata |
|--------------|--------------|----------|
| CurrencyId | IntId (201-300 range) | |
| Code | Key | |
| Name | DisplayName | |
| Symbol | — | Metadata["symbol"] |
| DecimalPlaces | — | Metadata["decimalPlaces"] |
| TenantId | TenantId | (preserve for tenant-specific) |
| IsActive | IsActive | |

**Example:**
```json
{
  "IntId": 201,
  "CategoryKey": "currencies",
  "Key": "USD",
  "DisplayName": "US Dollar",
  "IsGlobalScope": true,
  "TenantId": null,
  "Metadata": {
    "symbol": "$",
    "decimalPlaces": 2
  }
}
```

### 2. Language Migration
**Current Table:** `Master.Languages`
**Target:** `LoV.LovItem` with CategoryKey = "languages"

| Source Field | Target Field | Metadata |
|--------------|--------------|----------|
| LanguageId | IntId (101-200 range) | |
| Code | Key | |
| DisplayName | DisplayName | |
| NativeName | — | Metadata["nativeName"] |
| IsDefault | — | Metadata["isDefault"] |
| TenantId | TenantId | (preserve for tenant-specific) |
| IsActive | IsActive | |

### 3. TimeZone Migration
**Current Table:** `Master.TimeZones`
**Target:** `LoV.LovItem` with CategoryKey = "timezones"

| Source Field | Target Field | Metadata |
|--------------|--------------|----------|
| TimeZoneId | IntId (1-50 range) | |
| Identifier | Key | |
| DisplayName | DisplayName | |
| StandardName | — | Metadata["standardName"] |
| OffsetHours | — | Metadata["offsetHours"] |
| IsActive | IsActive | |

**Note:** TimeZones are global only (no tenant-specific variants).

### 4. Country Migration
**Current Table:** `Master.Countries`
**Target:** `LoV.LovItem` with CategoryKey = "countries"

| Source Field | Target Field | Metadata |
|--------------|--------------|----------|
| CountryId | IntId (51-100 range) | |
| Code | Key | |
| Name | DisplayName | |

**Note:** Countries are global only.

---

## Tenant Hierarchy & Inheritance

### Auto-Inheritance Model

**Scenario:** Tenant "ABC" (parent) with sub-tenants "ABC-US" and "ABC-IN"

1. **Global lookups** (IsGlobalScope = true, TenantId = null):
   - All tenants get: TimeZones, Countries, default Currencies, default Languages

2. **Parent tenant lookups** (TenantId = "ABC"):
   - Parent defines: Currencies [USD, EUR], Languages [en-US, fr-FR]
   - Sub-tenants "ABC-US", "ABC-IN" automatically inherit

3. **Sub-tenant customization** (TenantId = "ABC-US"):
   - Sub-tenant can disable inherited items by marking IsActive = false
   - Or add new items specific to sub-tenant (e.g., add INR currency for "ABC-IN")

### Query Pattern

To get lookups for tenant "ABC-US":
```sql
SELECT * FROM LoV.LovItems
WHERE CategoryKey = @category
  AND IsActive = true
  AND IsDeleted = false
  AND (
    IsGlobalScope = true                    -- Global lookups
    OR TenantId = 'ABC'                     -- Parent tenant
    OR TenantId = 'ABC-US'                  -- Specific sub-tenant
  )
ORDER BY SortOrder, DisplayName
```

---

## Seed Data

### System Lookups (IDs 1-999)

#### TimeZones (Global, IDs 1-50)
```
1  | America/New_York | Eastern Time (US & Canada)
2  | America/Chicago  | Central Time (US & Canada)
3  | America/Denver   | Mountain Time (US & Canada)
4  | America/Los_Angeles | Pacific Time (US & Canada)
5  | Europe/London    | GMT
6  | Europe/Paris     | Central European Time
7  | Asia/Tokyo       | Japan Standard Time
8  | Asia/Shanghai    | China Standard Time
9  | Asia/Dubai       | Gulf Standard Time
10 | Australia/Sydney | Australian Eastern Time
```

#### Countries (Global, IDs 51-100)
```
51 | US    | United States
52 | CA    | Canada
53 | MX    | Mexico
54 | GB    | United Kingdom
55 | FR    | France
56 | DE    | Germany
57 | IN    | India
58 | CN    | China
59 | JP    | Japan
60 | AU    | Australia
```

#### Languages (Global, IDs 101-150)
```
101 | en-US | English (US) | metadata: { "nativeName": "English", "isDefault": true }
102 | en-GB | English (UK) | metadata: { "nativeName": "English" }
103 | fr-FR | Français    | metadata: { "nativeName": "Français" }
104 | de-DE | Deutsch     | metadata: { "nativeName": "Deutsch" }
105 | es-ES | Español     | metadata: { "nativeName": "Español" }
106 | ja-JP | 日本語       | metadata: { "nativeName": "日本語" }
107 | zh-CN | 中文(简体)   | metadata: { "nativeName": "中文" }
108 | hi-IN | हिन्दी       | metadata: { "nativeName": "हिन्दी" }
```

#### Currencies (Global, IDs 201-250)
```
201 | USD | US Dollar       | metadata: { "symbol": "$", "decimalPlaces": 2 }
202 | EUR | Euro           | metadata: { "symbol": "€", "decimalPlaces": 2 }
203 | GBP | British Pound  | metadata: { "symbol": "£", "decimalPlaces": 2 }
204 | JPY | Japanese Yen   | metadata: { "symbol": "¥", "decimalPlaces": 0 }
205 | INR | Indian Rupee   | metadata: { "symbol": "₹", "decimalPlaces": 2 }
206 | CNY | Chinese Yuan   | metadata: { "symbol": "¥", "decimalPlaces": 2 }
207 | CAD | Canadian Dollar| metadata: { "symbol": "$", "decimalPlaces": 2 }
208 | AUD | Australian Dollar| metadata: { "symbol": "$", "decimalPlaces": 2 }
```

### Inactive/Deleted Items

Other common lookups can be seeded with IsActive = false or IsDeleted = true:
- Additional currencies (BRL, MXN, ZAR, etc.)
- Additional languages
- Can be enabled per tenant as needed

---

## Database Changes (V2 Schema)

### V2 Migration Strategy

**Keep v1 intact for backward compatibility:**
- Existing tables remain: `Master.Currencies`, `Master.Languages`, `Master.TimeZones`, `Master.Countries`
- Existing stored procedures and functions unchanged
- Applications can still reference v1 tables during transition

**New v2 schema:**
- Create new `LoV` schema for consolidated lookups
- Create new v2 stored procedures (prefixed with v2_)
- Run data migration from v1 tables → LoV.LovItems
- Applications gradually migrate to v2 APIs

### New LoV.LovItems Table (V2)

```sql
-- V2: Create new LoV schema with consolidated lookups
CREATE TABLE LoV.LovItems (
    IntId INT,
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    CategoryKey NVARCHAR(100) NOT NULL,
    SubCategoryKey NVARCHAR(100),
    Key NVARCHAR(100) NOT NULL,
    DisplayName NVARCHAR(500) NOT NULL,
    TenantId NVARCHAR(128),
    IsGlobalScope BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL,
    CreatedBy NVARCHAR(128),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(128),
    SortOrder INT NOT NULL DEFAULT 0,
    Metadata NVARCHAR(MAX),  -- JSON
    Tags NVARCHAR(MAX),      -- JSON array
    LocalizedNames NVARCHAR(MAX), -- JSON
    
    CONSTRAINT UQ_LovItems_IntId UNIQUE (IntId) WHERE IntId IS NOT NULL,
    CONSTRAINT UQ_LovItems_Key UNIQUE (CategoryKey, SubCategoryKey, Key, TenantId),
    INDEX IX_LovItems_Category (CategoryKey, IsActive, IsDeleted),
    INDEX IX_LovItems_Tenant (TenantId, IsGlobalScope, IsActive),
    INDEX IX_LovItems_Global (IsGlobalScope, IsActive, IsDeleted)
);
```

### V2 Migration Script

```sql
-- V2: Migrate data from v1 Master tables to LoV schema
-- Run ONCE, then old tables remain for backward compatibility

-- Migrate Currencies
INSERT INTO LoV.LovItems (IntId, Id, CategoryKey, Key, DisplayName, TenantId, IsGlobalScope, IsActive, IsDeleted, CreatedAt, CreatedBy, SortOrder, Metadata)
SELECT 
    CAST(CurrencyId AS INT) AS IntId,
    NEWID() AS Id,
    'currencies' AS CategoryKey,
    Code AS Key,
    Name AS DisplayName,
    TenantId,
    CASE WHEN TenantId IS NULL THEN 1 ELSE 0 END AS IsGlobalScope,
    IsActive,
    0 AS IsDeleted,
    CreatedAt,
    CreatedBy,
    0 AS SortOrder,
    JSON_OBJECT('symbol', Symbol, 'decimalPlaces', DecimalPlaces) AS Metadata
FROM Master.Currencies
WHERE IsDeleted = 0;

-- Migrate Languages
INSERT INTO LoV.LovItems (IntId, Id, CategoryKey, Key, DisplayName, TenantId, IsGlobalScope, IsActive, IsDeleted, CreatedAt, CreatedBy, SortOrder, Metadata)
SELECT 
    CAST(LanguageId AS INT) + 100 AS IntId,  -- Offset to 101-200 range
    NEWID() AS Id,
    'languages' AS CategoryKey,
    Code AS Key,
    DisplayName,
    TenantId,
    CASE WHEN TenantId IS NULL THEN 1 ELSE 0 END AS IsGlobalScope,
    IsActive,
    0 AS IsDeleted,
    CreatedAt,
    CreatedBy,
    0 AS SortOrder,
    JSON_OBJECT('nativeName', NativeName, 'isDefault', IsDefault) AS Metadata
FROM Master.Languages
WHERE IsDeleted = 0;

-- Migrate TimeZones
INSERT INTO LoV.LovItems (IntId, Id, CategoryKey, Key, DisplayName, IsGlobalScope, IsActive, IsDeleted, CreatedAt, CreatedBy, SortOrder, Metadata)
SELECT 
    TimeZoneId AS IntId,
    NEWID() AS Id,
    'timezones' AS CategoryKey,
    Identifier AS Key,
    DisplayName,
    1 AS IsGlobalScope,  -- TimeZones are always global
    IsActive,
    0 AS IsDeleted,
    CreatedAt,
    CreatedBy,
    0 AS SortOrder,
    JSON_OBJECT('standardName', StandardName, 'offsetHours', OffsetHours) AS Metadata
FROM Master.TimeZones
WHERE IsDeleted = 0;

-- Migrate Countries
INSERT INTO LoV.LovItems (IntId, Id, CategoryKey, Key, DisplayName, IsGlobalScope, IsActive, IsDeleted, CreatedAt, CreatedBy, SortOrder)
SELECT 
    CAST(CountryId AS INT) + 50 AS IntId,  -- Offset to 51-100 range
    NEWID() AS Id,
    'countries' AS CategoryKey,
    Code AS Key,
    Name AS DisplayName,
    1 AS IsGlobalScope,  -- Countries are always global
    1 AS IsActive,
    0 AS IsDeleted,
    GETUTCDATE() AS CreatedAt,
    'system' AS CreatedBy,
    0 AS SortOrder
FROM Master.Countries
WHERE IsDeleted = 0;
```

### V1 Tables (Keep for Backward Compatibility)

```sql
-- V1: Existing tables remain unchanged
-- Applications can still use these tables during transition period
-- Master.Currencies
-- Master.Languages
-- Master.TimeZones
-- Master.Countries

-- These are read-only during v2 migration period
-- All new entries should go to LoV.LovItems (v2)
```

---

## Dapper Implementation (Backend)

### UPSERT Stored Procedure

Use UPSERT instead of separate INSERT/UPDATE operations for atomic operations:

```sql
CREATE PROCEDURE LoV.sp_LovItem_Upsert
    @IntId INT = NULL,
    @Id UNIQUEIDENTIFIER,
    @CategoryKey NVARCHAR(100),
    @SubCategoryKey NVARCHAR(100) = NULL,
    @Key NVARCHAR(100),
    @DisplayName NVARCHAR(500),
    @TenantId NVARCHAR(128) = NULL,
    @IsGlobalScope BIT,
    @IsActive BIT = 1,
    @IsDeleted BIT = 0,
    @CreatedAt DATETIME2,
    @CreatedBy NVARCHAR(128),
    @UpdatedAt DATETIME2 = NULL,
    @UpdatedBy NVARCHAR(128) = NULL,
    @SortOrder INT = 0,
    @Metadata NVARCHAR(MAX) = NULL,
    @Tags NVARCHAR(MAX) = NULL,
    @LocalizedNames NVARCHAR(MAX) = NULL
AS
BEGIN
    MERGE LoV.LovItems AS target
    USING (
        SELECT 
            @IntId AS IntId,
            @Id AS Id,
            @CategoryKey AS CategoryKey,
            @SubCategoryKey AS SubCategoryKey,
            @Key AS Key,
            @DisplayName AS DisplayName,
            @TenantId AS TenantId,
            @IsGlobalScope AS IsGlobalScope,
            @IsActive AS IsActive,
            @IsDeleted AS IsDeleted,
            @CreatedAt AS CreatedAt,
            @CreatedBy AS CreatedBy,
            @UpdatedAt AS UpdatedAt,
            @UpdatedBy AS UpdatedBy,
            @SortOrder AS SortOrder,
            @Metadata AS Metadata,
            @Tags AS Tags,
            @LocalizedNames AS LocalizedNames
    ) AS source
    ON target.Id = source.Id
    WHEN MATCHED THEN
        UPDATE SET
            DisplayName = source.DisplayName,
            IsActive = source.IsActive,
            IsDeleted = source.IsDeleted,
            UpdatedAt = source.UpdatedAt,
            UpdatedBy = source.UpdatedBy,
            SortOrder = source.SortOrder,
            Metadata = source.Metadata,
            Tags = source.Tags,
            LocalizedNames = source.LocalizedNames
    WHEN NOT MATCHED THEN
        INSERT (IntId, Id, CategoryKey, SubCategoryKey, Key, DisplayName, TenantId, IsGlobalScope, IsActive, IsDeleted, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, SortOrder, Metadata, Tags, LocalizedNames)
        VALUES (source.IntId, source.Id, source.CategoryKey, source.SubCategoryKey, source.Key, source.DisplayName, source.TenantId, source.IsGlobalScope, source.IsActive, source.IsDeleted, source.CreatedAt, source.CreatedBy, source.UpdatedAt, source.UpdatedBy, source.SortOrder, source.Metadata, source.Tags, source.LocalizedNames);
END
```

### Dapper Repository Pattern

```csharp
public interface ILovRepository
{
    // Query operations
    Task<IEnumerable<LovItem>> GetByCategory(string categoryKey, string? tenantId = null);
    Task<IEnumerable<LovItem>> GetByTenantHierarchy(string categoryKey, string tenantId);
    Task<LovItem?> GetById(Guid id);
    Task<LovItem?> GetByIntId(int intId);
    Task<LovItem?> GetByKey(string categoryKey, string key, string? tenantId = null);
    
    // Write operations
    Task<int> Upsert(LovItem item);
    Task<int> UpsertBatch(IEnumerable<LovItem> items);
    Task<int> SetActive(Guid id, bool isActive);
    Task<int> Delete(Guid id);  // Soft delete
}

public class LovRepository : ILovRepository
{
    private readonly IDbConnection _db;
    
    public LovRepository(IDbConnection db) => _db = db;
    
    // Get all active items for a category (global + tenant scoped)
    public async Task<IEnumerable<LovItem>> GetByTenantHierarchy(string categoryKey, string tenantId)
    {
        const string sql = @"
            SELECT * FROM LoV.LovItems
            WHERE CategoryKey = @CategoryKey
              AND IsActive = 1
              AND IsDeleted = 0
              AND (IsGlobalScope = 1 OR TenantId IS NULL OR TenantId = @TenantId OR TenantId = @ParentTenantId)
            ORDER BY SortOrder, DisplayName
        ";
        
        var parentTenantId = GetParentTenantId(tenantId); // Extract parent from "ABC-US" → "ABC"
        
        return await _db.QueryAsync<LovItem>(sql, new { CategoryKey = categoryKey, TenantId = tenantId, ParentTenantId = parentTenantId });
    }
    
    // UPSERT using stored procedure
    public async Task<int> Upsert(LovItem item)
    {
        return await _db.ExecuteAsync("LoV.sp_LovItem_Upsert", new
        {
            item.IntId,
            item.Id,
            item.CategoryKey,
            item.SubCategoryKey,
            item.Key,
            item.DisplayName,
            item.TenantId,
            item.IsGlobalScope,
            item.IsActive,
            item.IsDeleted,
            item.CreatedAt,
            item.CreatedBy,
            item.UpdatedAt,
            item.UpdatedBy,
            item.SortOrder,
            Metadata = JsonConvert.SerializeObject(item.Metadata),
            Tags = JsonConvert.SerializeObject(item.Tags),
            LocalizedNames = JsonConvert.SerializeObject(item.LocalizedNames)
        }, commandType: CommandType.StoredProcedure);
    }
    
    private string? GetParentTenantId(string tenantId)
    {
        // "ABC-US" → "ABC"
        var parts = tenantId.Split('-');
        return parts.Length > 1 ? string.Join("-", parts.DropLast(1)) : null;
    }
}
```

### Service Layer

```csharp
public interface ILovService
{
    Task<IEnumerable<LovItemDto>> GetCurrencies(string tenantId);
    Task<IEnumerable<LovItemDto>> GetLanguages(string tenantId);
    Task<IEnumerable<LovItemDto>> GetTimeZones();
    Task<IEnumerable<LovItemDto>> GetCountries();
    
    Task<int> SaveCurrency(SaveCurrencyDto dto, string tenantId, string userId);
    Task<int> DisableLookup(Guid id, string userId);
}

public class LovService : ILovService
{
    private readonly ILovRepository _repository;
    
    public LovService(ILovRepository repository) => _repository = repository;
    
    public async Task<IEnumerable<LovItemDto>> GetCurrencies(string tenantId)
    {
        var items = await _repository.GetByTenantHierarchy("currencies", tenantId);
        return items.Select(MapToDto).ToList();
    }
    
    public async Task<int> SaveCurrency(SaveCurrencyDto dto, string tenantId, string userId)
    {
        var item = new LovItem
        {
            Id = dto.Id ?? Guid.NewGuid(),
            IntId = dto.IntId,
            CategoryKey = "currencies",
            Key = dto.Code,
            DisplayName = dto.Name,
            TenantId = tenantId,
            IsGlobalScope = tenantId == null,
            IsActive = dto.IsActive,
            Metadata = new Dictionary<string, object>
            {
                ["symbol"] = dto.Symbol,
                ["decimalPlaces"] = dto.DecimalPlaces
            },
            CreatedAt = dto.Id == null ? DateTime.UtcNow : item.CreatedAt,
            CreatedBy = dto.Id == null ? userId : item.CreatedBy,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = userId
        };
        
        return await _repository.Upsert(item);
    }
}
```

---

## UI Implementation

### Admin Page: Manage Lookups

**Route:** `/Admin/Lookups`

#### List View
- **Dropdown to filter by category:** Categories, Languages, TimeZones, Countries, Currencies
- **Table columns:** IntId | Key | DisplayName | TenantId | IsActive | Actions
- **Search:** Filter by Key or DisplayName
- **Actions:** Edit, Disable, Delete (soft delete)

#### Edit/Create Form
```
Category:        [Dropdown: currencies, languages, timezones, countries]
Key:             [TextBox: USD, en-US, America/New_York]
Display Name:    [TextBox: United States Dollar]
Tenant:          [Dropdown: Global, ABC, ABC-US, ABC-IN] (only if not system lookup)
Is Active:       [Checkbox]
Sort Order:      [Number]
Metadata:        [JSON Editor]
  - For Currencies: { "symbol": "$", "decimalPlaces": 2 }
  - For Languages: { "nativeName": "English", "isDefault": true }
  - For TimeZones: { "standardName": "EST", "offsetHours": -5 }
Localized Names: [Multi-language editor]
  - en-US: English
  - es-ES: Español
  - fr-FR: Français
```

**Save Button:** Calls POST /api/lookups/upsert
- Backend handles UPSERT (insert or update based on Id)
- Returns saved item with generated IntId (if creating)

#### Bulk Import
- **CSV Upload:** IntId | Key | DisplayName | Symbol | DecimalPlaces | IsActive
- **Backend:** Batch UPSERT all rows
- **Result:** Count of inserted/updated

### User Page: Select Currency/Language/TimeZone

**Scenario:** User profile settings form

```html
<select id="currency" name="currency">
    <option value="">-- Select Currency --</option>
    <!-- Load from GET /api/lookups/currencies?tenantId=ABC-US -->
    <option value="USD">US Dollar ($)</option>
    <option value="EUR">Euro (€)</option>
    <option value="INR">Indian Rupee (₹)</option>
</select>

<select id="language" name="language">
    <option value="">-- Select Language --</option>
    <!-- Load from GET /api/lookups/languages?tenantId=ABC-US -->
    <option value="en-US">English</option>
    <option value="es-ES">Español</option>
</select>

<select id="timezone" name="timezone">
    <option value="">-- Select Time Zone --</option>
    <!-- Load from GET /api/lookups/timezones (global) -->
    <option value="America/New_York">Eastern Time (US & Canada)</option>
    <option value="America/Los_Angeles">Pacific Time (US & Canada)</option>
</select>
```

**JavaScript:**
```javascript
// Load currencies on page init
fetch(`/api/lookups/currencies?tenantId=${currentTenantId}`)
    .then(r => r.json())
    .then(items => {
        // items: [{ key: "USD", displayName: "US Dollar", ... }]
        items.forEach(item => {
            const option = document.createElement('option');
            option.value = item.key;
            option.textContent = `${item.displayName} (${item.metadata.symbol})`;
            currencySelect.appendChild(option);
        });
    });
```

---

## API Endpoints

### GET /api/lookups/:category
Get all active lookups for a category (respects tenant hierarchy)

**Query Params:**
- `tenantId` (optional): If provided, returns items for that tenant + parent + global
- `culture` (optional): Return display names in specified culture (from LocalizedNames)

**Response:**
```json
[
    {
        "id": "guid",
        "intId": 201,
        "key": "USD",
        "displayName": "US Dollar",
        "tenantId": null,
        "isGlobalScope": true,
        "isActive": true,
        "sortOrder": 1,
        "metadata": {
            "symbol": "$",
            "decimalPlaces": 2
        }
    }
]
```

### POST /api/lookups/upsert
Create or update a lookup item (UPSERT operation)

**Body:**
```json
{
    "id": "guid-or-null",
    "intId": 201,
    "categoryKey": "currencies",
    "key": "USD",
    "displayName": "US Dollar",
    "tenantId": null,
    "isGlobalScope": true,
    "isActive": true,
    "sortOrder": 1,
    "metadata": {
        "symbol": "$",
        "decimalPlaces": 2
    }
}
```

**Response:**
```json
{
    "success": true,
    "id": "guid",
    "intId": 201,
    "message": "Lookup item saved successfully"
}
```

### PUT /api/lookups/:id/toggle-active
Toggle active status (don't delete, just deactivate)

**Response:**
```json
{
    "success": true,
    "isActive": false
}
```

### DELETE /api/lookups/:id
Soft delete (set IsDeleted = true)

**Response:**
```json
{
    "success": true
}
```

---

## Benefits

| Benefit | Impact |
|---------|--------|
| **Single lookup table** | Easier to add new lookup types, no new table schemas |
| **Consistent structure** | All lookups follow same pattern (CategoryKey/Key/DisplayName) |
| **Hierarchical inheritance** | Parent tenants manage, sub-tenants inherit automatically |
| **Metadata flexibility** | Store lookup-specific data (symbols, offsets) without schema changes |
| **Active/Inactive management** | Disable lookups without deleting (can re-enable later) |
| **Integer ID allocation** | Predictable IDs 1-999 for system lookups, easier for seed data |

---

## Backward Compatibility

- Keep GUID IDs (existing systems using them won't break)
- Old table schemas remain until migration complete
- Provide repository layer to abstract LoV queries
- Update services to query LoV instead of old tables

---

## Migration Path (V1 → V2)

**Goal:** Gradual migration without breaking existing applications

1. **Phase 1:** Create LoV schema (v2)
   - Create `LoV.LovItems` table
   - Create v2 stored procedures (`LoV.sp_LovItem_Upsert`, etc.)
   - All v1 tables remain unchanged and functional

2. **Phase 2:** Data migration (v1 → v2)
   - Run migration scripts to copy data from Master.Currencies, Master.Languages, Master.TimeZones, Master.Countries → LoV.LovItems
   - Seed system lookups (IntIds 1-999)
   - v1 tables become read-only (no new inserts/updates)

3. **Phase 3:** Application layer migration
   - Update services/repositories to query from LoV (v2 APIs)
   - Old v1 API endpoints remain functional but delegated to v2
   - Gradual rollout per module

4. **Phase 4:** Deprecation period
   - v1 tables remain but marked as deprecated (documentation updated)
   - Monitoring: Ensure no applications still directly accessing v1 tables
   - Run for 1-2 quarters for safety

5. **Phase 5:** Decommission v1 (Optional cleanup)
   - Only after all applications confirmed migrated to v2
   - Keep Category as separate domain entity (not moving to LoV)

---

## Backward Compatibility Views (Optional)

To help v1 applications transition without code changes, create compatibility views:

```sql
-- V2: Backward compatibility views (map v2 LoV back to v1 table structure)

CREATE VIEW Master.Currencies_v2 AS
SELECT 
    IntId AS CurrencyId,
    Key AS Code,
    DisplayName AS Name,
    JSON_VALUE(Metadata, '$.symbol') AS Symbol,
    CAST(JSON_VALUE(Metadata, '$.decimalPlaces') AS INT) AS DecimalPlaces,
    TenantId,
    IsActive,
    CreatedAt,
    CreatedBy,
    UpdatedAt,
    UpdatedBy,
    0 AS IsDeleted
FROM LoV.LovItems
WHERE CategoryKey = 'currencies';

CREATE VIEW Master.Languages_v2 AS
SELECT 
    IntId - 100 AS LanguageId,  -- Reverse the offset
    Key AS Code,
    DisplayName,
    DisplayName AS DisplayName,
    JSON_VALUE(Metadata, '$.nativeName') AS NativeName,
    CAST(JSON_VALUE(Metadata, '$.isDefault') AS BIT) AS IsDefault,
    TenantId,
    IsActive,
    CreatedAt,
    CreatedBy,
    UpdatedAt,
    UpdatedBy,
    0 AS IsDeleted
FROM LoV.LovItems
WHERE CategoryKey = 'languages';

-- Similar views for TimeZones and Countries
```

**Usage:** Applications can query `Master.Currencies_v2` instead of `Master.Currencies` during transition.

---

## Notes

- **Category.cs** remains separate (domain entity with Products relationships)
- **V1 tables unchanged:** Keep Master.Currencies, Master.Languages, Master.TimeZones, Master.Countries as-is
- **V2 is additive:** New LoV schema coexists with v1
- **Integer IDs:** Optional for custom tenant lookups (system lookups always have IntIds 1-999)
- **Metadata field:** JSON to avoid schema changes for new lookup-specific data
- **Indexes optimized for:** Category lookups, tenant-scoped queries, global scope queries

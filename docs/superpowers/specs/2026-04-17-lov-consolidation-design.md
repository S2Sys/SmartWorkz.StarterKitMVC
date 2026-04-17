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

## Database Changes

### New Table Structure

```sql
CREATE TABLE LoV.LovItems (
    IntId INT,
    Id GUID PRIMARY KEY,
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

-- Drop old master tables (after migration)
-- DROP TABLE Master.Currencies
-- DROP TABLE Master.Languages
-- DROP TABLE Master.TimeZones
-- DROP TABLE Master.Countries
```

### DbContext Changes

```csharp
public DbSet<LovItem> LovItems { get; set; }

// Remove:
// public DbSet<Currency> Currencies { get; set; }
// public DbSet<Language> Languages { get; set; }
// public DbSet<TimeZone> TimeZones { get; set; }
// public DbSet<Country> Countries { get; set; }
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

## Migration Path

1. **Phase 1:** Create new LoV structure, seed system lookups (1-999)
2. **Phase 2:** Migrate existing Currencies, Languages, TimeZones, Countries to LoV
3. **Phase 3:** Update services/repositories to query from LoV
4. **Phase 4:** Decommission old Master tables (Currencies, Languages, TimeZones, Countries)
5. **Phase 5:** Keep Category as separate domain entity (not moving to LoV)

---

## Notes

- **Category.cs** remains separate (domain entity with Products relationships)
- Integer IDs are optional (IntId can be null for custom tenant lookups)
- Metadata field is JSON to avoid schema changes for new lookup-specific data
- Indexes optimized for: category lookups, tenant-scoped queries, global scope queries

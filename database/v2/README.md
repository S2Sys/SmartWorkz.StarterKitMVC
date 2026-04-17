# List of Values (LoV) Consolidation - V2 Schema

**Version:** 2.0  
**Date:** 2026-04-17  
**Status:** Design Phase → Implementation Phase

---

## Overview

LoV (List of Values) is a unified lookup system for managing reference data. This v2 schema consolidates fragmented master tables (Currencies, Languages, TimeZones, Countries) into a single, flexible LoV table with support for hierarchical tenant inheritance.

---

## How LoV Works

### Core Concept

Instead of separate tables for each lookup type, LoV uses a single table with:
- **CategoryKey** to organize by type (e.g., "currencies", "languages", "timezones")
- **Key** for unique identifier within category (e.g., "USD", "en-US")
- **DisplayName** for user-friendly name
- **Metadata** JSON field for lookup-specific data (symbols, offsets, etc.)

### Hierarchical Tenant Inheritance (Auto-Inherit Model)

**Scenario:** Tenant "ABC" (parent) → Sub-tenants "ABC-US", "ABC-IN"

1. **Global scope** (IsGlobalScope = true, TenantId = null)
   - Available to ALL tenants
   - Examples: TimeZones, Countries

2. **Parent tenant scope** (TenantId = "ABC", IsGlobalScope = false)
   - Defined by parent "ABC"
   - Automatically inherited by sub-tenants "ABC-US", "ABC-IN"
   - Sub-tenants can disable via IsActive = false

3. **Query logic:**
   ```sql
   SELECT * FROM LoV.LovItems
   WHERE CategoryKey = 'currencies'
     AND IsActive = 1
     AND (IsGlobalScope = 1 OR TenantId = 'ABC' OR TenantId = 'ABC-US')
   ORDER BY SortOrder, DisplayName
   ```
   Returns: Global + Parent + Sub-tenant lookups

### Integer ID Allocation

| Range | Type | Examples |
|-------|------|----------|
| 1-50 | System | TimeZones |
| 51-100 | System | Countries |
| 101-200 | System | Languages |
| 201-300 | System | Currencies |
| 301-999 | Reserved | Future system lookups |

System lookups (1-999) are read-only global defaults. Tenant-specific lookups get auto-generated IntIds >= 1000.

### Key Fields

```
IntId              → Integer ID (1-999 for system, optional for tenant lookups)
Id                 → GUID (for compatibility)
CategoryKey        → "currencies", "languages", "timezones", "countries"
Key                → Unique identifier (USD, en-US, America/New_York, US)
DisplayName        → User-friendly name
TenantId           → null (global) or "ABC" (parent) or "ABC-US" (sub-tenant)
IsGlobalScope      → true (global) or false (tenant-scoped)
IsActive           → Available for use?
IsDeleted          → Soft delete flag
Metadata           → JSON field { "symbol": "$", "decimalPlaces": 2, ... }
LocalizedNames     → Multi-language names { "en-US": "...", "es-ES": "..." }
```

---

## Use Cases

| Scenario | Query Pattern |
|----------|---------------|
| **User selects currency** | GET /api/lookups/currencies?tenantId=ABC-US (get all active, respecting hierarchy) |
| **Admin manages currencies** | POST /api/lookups/upsert (UPSERT a single item) |
| **Currency formatting** | Look up Metadata["symbol"], Metadata["decimalPlaces"] |
| **Localization** | Look up LocalizedNames["es-ES"] for Spanish name |
| **Disable currency** | PUT /api/lookups/:id/toggle-active (set IsActive = false) |

---

## V1 vs V2

| Aspect | V1 (Old) | V2 (New) |
|--------|----------|----------|
| Structure | Separate tables (Currencies, Languages, etc.) | Single LoV.LovItems table + metadata |
| Tenant scope | Per-table tenant relationships | TenantId field + hierarchy |
| Metadata | Hardcoded columns (Symbol, OffsetHours, etc.) | Flexible JSON field |
| Extensibility | Add column → new migration | Add metadata → no schema change |
| Status | V1 tables remain for backward compatibility | New v2 schema alongside v1 |

---

## Migration Strategy

**Keep v1 intact (no modifications):**
1. V1 tables (`Master.Currencies`, `Master.Languages`, `Master.TimeZones`, `Master.Countries`) remain unchanged
2. V2 created alongside v1
3. Data migrated from v1 → v2 (one-time migration script)
4. Applications gradually migrate to v2 APIs
5. Optional backward compatibility views available
6. V1 decommissioned only after all apps migrated

---

## File Structure

```
/database/v2/
├── README.md                      (this file - wiki)
├── IMPLEMENTATION_PLAN.md         (detailed SDLC plan)
└── scripts/
    ├── 01-create-lov-schema.sql           (create LoV schema)
    ├── 02-create-lov-items-table.sql      (create LovItems table)
    ├── 03-create-upsert-procedures.sql    (UPSERT stored procedures)
    ├── 04-seed-timezones.sql              (seed global timezones)
    ├── 05-seed-countries.sql              (seed global countries)
    ├── 06-seed-languages.sql              (seed global languages)
    ├── 07-seed-currencies.sql             (seed global currencies)
    ├── 10-migrate-from-v1.sql             (migrate Master tables → LoV)
    ├── 20-create-compatibility-views.sql  (optional: backward compat views)
    └── 99-cleanup-v1.sql                  (optional: drop v1 tables after deprecation)
```

---

## Implementation Phases

**Phase 1:** Database setup (create v2 schema, tables, procedures, seed data)
**Phase 2:** Backend implementation (Dapper repositories, services, API endpoints)
**Phase 3:** Admin UI (manage lookups, bulk import, enable/disable)
**Phase 4:** Public UI (dropdowns, multi-select, preference saving)
**Phase 5:** Testing & Deployment

See `IMPLEMENTATION_PLAN.md` for detailed tasks and code examples.

---

## Getting Started

1. **Database Setup:** Run scripts in `/database/v2/scripts/` in order (01-99)
2. **Backend:** Implement Dapper repository + service layer (see implementation plan)
3. **Admin UI:** Create lookup management pages (see implementation plan)
4. **Public UI:** Update dropdowns to use new API (see implementation plan)
5. **Deploy:** Gradual rollout, keep v1 APIs functional during transition

---

## Questions?

Refer to specific sections:
- **"How LoV Works"** → Conceptual understanding
- **"Use Cases"** → Real-world examples
- **"IMPLEMENTATION_PLAN.md"** → Code and UI details

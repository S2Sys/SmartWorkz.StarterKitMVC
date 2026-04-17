# LoV V2 Implementation Plan - Full SDLC

**Version:** 2.0  
**Date:** 2026-04-17  
**Scope:** Complete implementation from database → backend → frontend (Admin + Public)

---

## Phases & Tasks

### Phase 1: Database Setup (Scripts)
### Phase 2: Backend Implementation (Repository + Service)
### Phase 3: Admin UI Implementation (Manage Lookups)
### Phase 4: Public UI Implementation (Use Lookups)
### Phase 5: Testing & Deployment

---

# PHASE 1: DATABASE SETUP

## Quick Start

**Run single master script for complete v2 setup:**
```bash
-- Execute: /database/v2/scripts/00-master-migration.sql
-- This creates everything in one script:
--   ✓ LoV schema
--   ✓ LovItems table with indexes
--   ✓ UPSERT procedures
--   ✓ 30 parent lookups (IDs 1-999)
```

**OR run individual scripts:**
Execute scripts in `/database/v2/scripts/` in order (01 → 07)

---

## ID Allocation Strategy

| Range | Type | Scope | IsGlobalScope | TenantId | Examples |
|-------|------|-------|---------------|----------|----------|
| 1-50 | Parent | TimeZones | 1 | NULL | 1-10 |
| 51-100 | Parent | Countries | 1 | NULL | 51-60 |
| 101-200 | Parent | Languages | 1 | NULL | 101-110 |
| 201-300 | Parent | Currencies | 1 | NULL | 201-210 |
| 301-999 | Parent | Reserved | 1 | NULL | Future |
| 1000+ | Child | Tenant-specific | 0 | "ABC" | Auto-generated |

**Parent Lookups (1-999):** System defaults, inherited by all tenants  
**Child Lookups (1000+):** Tenant customizations, added when tenant creates own lookups

---

## Database Setup Tasks

## Task 1.1: Create LoV Schema
**File:** `01-create-lov-schema.sql`
```sql
-- Create LoV schema for v2 lookups
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'LoV')
BEGIN
    EXEC sp_executesql N'CREATE SCHEMA LoV'
END
```

## Task 1.2: Create LovItems Table
**File:** `02-create-lov-items-table.sql`
```sql
CREATE TABLE LoV.LovItems (
    IntId INT,
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CategoryKey NVARCHAR(100) NOT NULL,
    SubCategoryKey NVARCHAR(100),
    Key NVARCHAR(100) NOT NULL,
    DisplayName NVARCHAR(500) NOT NULL,
    TenantId NVARCHAR(128),
    IsGlobalScope BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(128),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(128),
    SortOrder INT NOT NULL DEFAULT 0,
    Metadata NVARCHAR(MAX),      -- JSON: { "symbol": "$", "decimalPlaces": 2 }
    Tags NVARCHAR(MAX),          -- JSON: ["tag1", "tag2"]
    LocalizedNames NVARCHAR(MAX), -- JSON: { "en-US": "English", "es-ES": "Español" }
    
    CONSTRAINT UQ_LovItems_IntId UNIQUE (IntId) WHERE IntId IS NOT NULL,
    CONSTRAINT UQ_LovItems_Key UNIQUE (CategoryKey, SubCategoryKey, Key, TenantId)
);

-- Indexes
CREATE INDEX IX_LovItems_Category ON LoV.LovItems(CategoryKey, IsActive, IsDeleted);
CREATE INDEX IX_LovItems_Tenant ON LoV.LovItems(TenantId, IsGlobalScope, IsActive);
CREATE INDEX IX_LovItems_Global ON LoV.LovItems(IsGlobalScope, IsActive, IsDeleted);
CREATE INDEX IX_LovItems_Key ON LoV.LovItems(CategoryKey, Key, TenantId);
```

## Task 1.3: Create UPSERT Stored Procedure
**File:** `03-create-upsert-procedures.sql`
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
    USING (SELECT @IntId, @Id, @CategoryKey, @SubCategoryKey, @Key, @DisplayName, @TenantId, @IsGlobalScope, @IsActive, @IsDeleted, @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy, @SortOrder, @Metadata, @Tags, @LocalizedNames) 
    AS source(IntId, Id, CategoryKey, SubCategoryKey, Key, DisplayName, TenantId, IsGlobalScope, IsActive, IsDeleted, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, SortOrder, Metadata, Tags, LocalizedNames)
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
GO

-- Additional procedure: Get by tenant hierarchy
CREATE PROCEDURE LoV.sp_LovItem_GetByTenantHierarchy
    @CategoryKey NVARCHAR(100),
    @TenantId NVARCHAR(128) = NULL
AS
BEGIN
    DECLARE @ParentTenantId NVARCHAR(128) = NULL
    
    -- Extract parent tenant ID (ABC-US → ABC)
    IF @TenantId LIKE '%-'
        SET @ParentTenantId = LEFT(@TenantId, CHARINDEX('-', @TenantId) - 1)
    
    SELECT * FROM LoV.LovItems
    WHERE CategoryKey = @CategoryKey
      AND IsActive = 1
      AND IsDeleted = 0
      AND (
        IsGlobalScope = 1
        OR TenantId IS NULL
        OR TenantId = @ParentTenantId
        OR TenantId = @TenantId
      )
    ORDER BY SortOrder, DisplayName
END
GO
```

## Task 1.4-1.7: Seed System Lookups
**Files:** `04-seed-timezones.sql`, `05-seed-countries.sql`, `06-seed-languages.sql`, `07-seed-currencies.sql`

Example: Seed Currencies
```sql
INSERT INTO LoV.LovItems (IntId, Id, CategoryKey, Key, DisplayName, TenantId, IsGlobalScope, IsActive, CreatedAt, CreatedBy, SortOrder, Metadata)
VALUES
  (201, NEWID(), 'currencies', 'USD', 'US Dollar', NULL, 1, 1, GETUTCDATE(), 'system', 1, '{"symbol":"$","decimalPlaces":2}'),
  (202, NEWID(), 'currencies', 'EUR', 'Euro', NULL, 1, 1, GETUTCDATE(), 'system', 2, '{"symbol":"€","decimalPlaces":2}'),
  (203, NEWID(), 'currencies', 'GBP', 'British Pound', NULL, 1, 1, GETUTCDATE(), 'system', 3, '{"symbol":"£","decimalPlaces":2}'),
  (204, NEWID(), 'currencies', 'JPY', 'Japanese Yen', NULL, 1, 1, GETUTCDATE(), 'system', 4, '{"symbol":"¥","decimalPlaces":0}'),
  (205, NEWID(), 'currencies', 'INR', 'Indian Rupee', NULL, 1, 1, GETUTCDATE(), 'system', 5, '{"symbol":"₹","decimalPlaces":2}');
```

## Task 1.8: Migrate from V1
**File:** `10-migrate-from-v1.sql`
Runs once to copy data from Master.Currencies → LoV.LovItems (see README.md)

---

# PHASE 2: BACKEND IMPLEMENTATION

## Task 2.1: Create Dapper Model
**File:** `src/SmartWorkz.StarterKitMVC.Domain/LoV/LovItemV2.cs`
```csharp
namespace SmartWorkz.StarterKitMVC.Domain.LoV;

public sealed class LovItemV2
{
    public int? IntId { get; set; }
    public Guid Id { get; set; }
    public string CategoryKey { get; set; } = string.Empty;
    public string? SubCategoryKey { get; set; }
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? TenantId { get; set; }
    public bool IsGlobalScope { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public int SortOrder { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public List<string>? Tags { get; set; }
    public Dictionary<string, string>? LocalizedNames { get; set; }
}
```

## Task 2.2: Create Dapper Repository Interface
**File:** `src/SmartWorkz.StarterKitMVC.Application/Repositories/ILovRepositoryV2.cs`
```csharp
public interface ILovRepositoryV2
{
    // Queries
    Task<IEnumerable<LovItemV2>> GetByCategory(string categoryKey, string? tenantId = null);
    Task<IEnumerable<LovItemV2>> GetByTenantHierarchy(string categoryKey, string tenantId);
    Task<LovItemV2?> GetById(Guid id);
    Task<LovItemV2?> GetByKey(string categoryKey, string key, string? tenantId = null);
    
    // Writes (using UPSERT)
    Task<int> Upsert(LovItemV2 item);
    Task<int> UpsertBatch(IEnumerable<LovItemV2> items);
    Task<int> SetActive(Guid id, bool isActive);
    Task<int> Delete(Guid id);  // Soft delete
}
```

## Task 2.3: Implement Dapper Repository
**File:** `src/SmartWorkz.StarterKitMVC.Infrastructure/Repositories/LovRepositoryV2.cs`
```csharp
public class LovRepositoryV2 : ILovRepositoryV2
{
    private readonly IDbConnection _db;
    
    public LovRepositoryV2(IDbConnection db) => _db = db;
    
    public async Task<IEnumerable<LovItemV2>> GetByTenantHierarchy(string categoryKey, string tenantId)
    {
        const string sql = @"
            DECLARE @ParentTenantId NVARCHAR(128) = NULL
            IF @TenantId LIKE '%-'
                SET @ParentTenantId = LEFT(@TenantId, CHARINDEX('-', @TenantId) - 1)
            
            SELECT * FROM LoV.LovItems
            WHERE CategoryKey = @CategoryKey
              AND IsActive = 1
              AND IsDeleted = 0
              AND (IsGlobalScope = 1 OR TenantId IS NULL OR TenantId = @ParentTenantId OR TenantId = @TenantId)
            ORDER BY SortOrder, DisplayName
        ";
        
        return await _db.QueryAsync<LovItemV2>(sql, new { CategoryKey = categoryKey, TenantId = tenantId });
    }
    
    public async Task<int> Upsert(LovItemV2 item)
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
            Metadata = item.Metadata != null ? JsonConvert.SerializeObject(item.Metadata) : null,
            Tags = item.Tags != null ? JsonConvert.SerializeObject(item.Tags) : null,
            LocalizedNames = item.LocalizedNames != null ? JsonConvert.SerializeObject(item.LocalizedNames) : null
        }, commandType: CommandType.StoredProcedure);
    }
    
    public async Task<int> Delete(Guid id)
    {
        const string sql = "UPDATE LoV.LovItems SET IsDeleted = 1, UpdatedAt = GETUTCDATE() WHERE Id = @Id";
        return await _db.ExecuteAsync(sql, new { Id = id });
    }
}
```

## Task 2.4: Create Service Layer
**File:** `src/SmartWorkz.StarterKitMVC.Application/Services/LovServiceV2.cs`
```csharp
public interface ILovServiceV2
{
    Task<IEnumerable<LovItemDto>> GetCurrencies(string tenantId);
    Task<IEnumerable<LovItemDto>> GetLanguages(string tenantId);
    Task<IEnumerable<LovItemDto>> GetTimeZones();
    Task<IEnumerable<LovItemDto>> GetCountries();
    Task<LovItemDto?> GetById(Guid id);
    Task<int> SaveLookup(SaveLookupDto dto, string tenantId, string userId);
    Task<int> DisableLookup(Guid id, string userId);
}

public class LovServiceV2 : ILovServiceV2
{
    private readonly ILovRepositoryV2 _repository;
    
    public LovServiceV2(ILovRepositoryV2 repository) => _repository = repository;
    
    public async Task<IEnumerable<LovItemDto>> GetCurrencies(string tenantId)
    {
        var items = await _repository.GetByTenantHierarchy("currencies", tenantId);
        return items.Select(MapToDto).ToList();
    }
    
    public async Task<int> SaveLookup(SaveLookupDto dto, string tenantId, string userId)
    {
        var item = new LovItemV2
        {
            Id = dto.Id ?? Guid.NewGuid(),
            IntId = dto.IntId,
            CategoryKey = dto.CategoryKey,
            Key = dto.Key,
            DisplayName = dto.DisplayName,
            TenantId = tenantId,
            IsGlobalScope = tenantId == null,
            IsActive = dto.IsActive,
            Metadata = dto.Metadata,
            LocalizedNames = dto.LocalizedNames,
            CreatedAt = dto.Id == null ? DateTime.UtcNow : DateTime.UtcNow,
            CreatedBy = userId,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = userId
        };
        
        return await _repository.Upsert(item);
    }
}
```

## Task 2.5: Create API Controller
**File:** `src/SmartWorkz.StarterKitMVC.Admin/Pages/Api/LookupsController.cs`
```csharp
[ApiController]
[Route("api/[controller]")]
public class LookupsController : ControllerBase
{
    private readonly ILovServiceV2 _service;
    
    public LookupsController(ILovServiceV2 service) => _service = service;
    
    [HttpGet("currencies")]
    public async Task<IActionResult> GetCurrencies([FromQuery] string tenantId)
    {
        var items = await _service.GetCurrencies(tenantId);
        return Ok(items);
    }
    
    [HttpGet("languages")]
    public async Task<IActionResult> GetLanguages([FromQuery] string tenantId)
    {
        var items = await _service.GetLanguages(tenantId);
        return Ok(items);
    }
    
    [HttpPost("upsert")]
    public async Task<IActionResult> UpsertLookup([FromBody] SaveLookupDto dto)
    {
        var tenantId = HttpContext.GetTenantId();
        var userId = HttpContext.GetUserId();
        
        var result = await _service.SaveLookup(dto, tenantId, userId);
        
        return Ok(new { success = result > 0, message = "Lookup saved successfully" });
    }
    
    [HttpPut("{id}/toggle-active")]
    public async Task<IActionResult> ToggleActive(Guid id)
    {
        var userId = HttpContext.GetUserId();
        var result = await _service.DisableLookup(id, userId);
        
        return Ok(new { success = result > 0 });
    }
}
```

---

# PHASE 3: ADMIN UI IMPLEMENTATION

## Task 3.1: Create Admin Lookup Page
**File:** `src/SmartWorkz.StarterKitMVC.Admin/Pages/Admin/Lookups/Index.cshtml`
```html
@page "/admin/lookups"
@model LookupIndexModel

<div class="container-fluid">
    <h2>Manage Lookups</h2>
    
    <!-- Filter by Category -->
    <div class="row mb-3">
        <div class="col-md-4">
            <label>Category</label>
            <select id="categoryFilter" class="form-control" onchange="filterByCategory()">
                <option value="">All Categories</option>
                <option value="currencies">Currencies</option>
                <option value="languages">Languages</option>
                <option value="timezones">Time Zones</option>
                <option value="countries">Countries</option>
            </select>
        </div>
        <div class="col-md-4">
            <label>Tenant</label>
            <select id="tenantFilter" class="form-control" onchange="filterByTenant()">
                <option value="">Global</option>
                <option value="@Model.CurrentTenant.Id">@Model.CurrentTenant.Name</option>
                @foreach(var subTenant in Model.SubTenants)
                {
                    <option value="@subTenant.Id">@subTenant.Name</option>
                }
            </select>
        </div>
        <div class="col-md-4 pt-2">
            <button class="btn btn-primary mt-4" onclick="openCreateModal()">+ New Lookup</button>
            <button class="btn btn-secondary mt-4" onclick="importCSV()">Import CSV</button>
        </div>
    </div>
    
    <!-- Table -->
    <table class="table table-striped">
        <thead>
            <tr>
                <th>IntId</th>
                <th>Key</th>
                <th>Display Name</th>
                <th>Category</th>
                <th>Tenant</th>
                <th>Active</th>
                <th>Actions</th>
            </tr>
        </thead>
        <tbody id="tableBody">
            <!-- Populated by JavaScript -->
        </tbody>
    </table>
</div>

<!-- Create/Edit Modal -->
<div class="modal fade" id="lookupModal" tabindex="-1">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="modalTitle">New Lookup</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <form id="lookupForm">
                    <input type="hidden" id="lookupId" />
                    
                    <div class="mb-3">
                        <label>Category *</label>
                        <select id="categorySelect" class="form-control" required>
                            <option value="">Select Category</option>
                            <option value="currencies">Currencies</option>
                            <option value="languages">Languages</option>
                            <option value="timezones">Time Zones</option>
                            <option value="countries">Countries</option>
                        </select>
                    </div>
                    
                    <div class="mb-3">
                        <label>Key *</label>
                        <input type="text" id="keyInput" class="form-control" placeholder="USD, en-US" required />
                    </div>
                    
                    <div class="mb-3">
                        <label>Display Name *</label>
                        <input type="text" id="displayNameInput" class="form-control" placeholder="US Dollar" required />
                    </div>
                    
                    <div class="mb-3">
                        <label>Metadata (JSON)</label>
                        <textarea id="metadataInput" class="form-control" rows="4" placeholder='{"symbol":"$","decimalPlaces":2}'></textarea>
                    </div>
                    
                    <div class="mb-3">
                        <label>
                            <input type="checkbox" id="isActiveCheck" checked /> Active
                        </label>
                    </div>
                </form>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                <button type="button" class="btn btn-primary" onclick="saveLookup()">Save</button>
            </div>
        </div>
    </div>
</div>

<script>
// Load lookups on page init
document.addEventListener('DOMContentLoaded', function() {
    loadLookups();
});

async function loadLookups() {
    const category = document.getElementById('categoryFilter').value;
    const tenant = document.getElementById('tenantFilter').value;
    
    const url = `/api/lookups/${category || 'all'}?tenantId=${tenant}`;
    const response = await fetch(url);
    const items = await response.json();
    
    const tbody = document.getElementById('tableBody');
    tbody.innerHTML = items.map(item => `
        <tr>
            <td>${item.intId || '-'}</td>
            <td>${item.key}</td>
            <td>${item.displayName}</td>
            <td>${item.categoryKey}</td>
            <td>${item.tenantId || 'Global'}</td>
            <td><input type="checkbox" ${item.isActive ? 'checked' : ''} onchange="toggleActive('${item.id}')" /></td>
            <td>
                <button class="btn btn-sm btn-primary" onclick="editLookup('${item.id}')">Edit</button>
                <button class="btn btn-sm btn-danger" onclick="deleteLookup('${item.id}')">Delete</button>
            </td>
        </tr>
    `).join('');
}

function openCreateModal() {
    document.getElementById('lookupId').value = '';
    document.getElementById('lookupForm').reset();
    document.getElementById('modalTitle').textContent = 'New Lookup';
    new bootstrap.Modal(document.getElementById('lookupModal')).show();
}

async function saveLookup() {
    const dto = {
        id: document.getElementById('lookupId').value || null,
        categoryKey: document.getElementById('categorySelect').value,
        key: document.getElementById('keyInput').value,
        displayName: document.getElementById('displayNameInput').value,
        isActive: document.getElementById('isActiveCheck').checked,
        metadata: document.getElementById('metadataInput').value ? JSON.parse(document.getElementById('metadataInput').value) : null
    };
    
    const response = await fetch('/api/lookups/upsert', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(dto)
    });
    
    if (response.ok) {
        bootstrap.Modal.getInstance(document.getElementById('lookupModal')).hide();
        loadLookups();
    }
}

async function toggleActive(id) {
    await fetch(`/api/lookups/${id}/toggle-active`, { method: 'PUT' });
    loadLookups();
}

async function deleteLookup(id) {
    if (!confirm('Delete this lookup?')) return;
    await fetch(`/api/lookups/${id}`, { method: 'DELETE' });
    loadLookups();
}

function filterByCategory() {
    loadLookups();
}

function filterByTenant() {
    loadLookups();
}

function importCSV() {
    // TODO: Implement CSV import
}
</script>
```

## Task 3.2: Create Page Model
**File:** `src/SmartWorkz.StarterKitMVC.Admin/Pages/Admin/Lookups/Index.cshtml.cs`
```csharp
public class LookupIndexModel : PageModel
{
    private readonly ILovServiceV2 _service;
    
    [BindProperty(SupportsGet = true)]
    public string? CategoryFilter { get; set; }
    
    public Tenant CurrentTenant { get; set; }
    public List<Tenant> SubTenants { get; set; } = new();
    
    public async Task OnGetAsync()
    {
        CurrentTenant = HttpContext.GetCurrentTenant();
        SubTenants = await _service.GetSubTenants(CurrentTenant.Id);
    }
}
```

---

# PHASE 4: PUBLIC UI IMPLEMENTATION

## Task 4.1: Create Currency Selector Component
**File:** `src/SmartWorkz.StarterKitMVC.Web/Pages/Components/CurrencySelector.cshtml`
```html
<div class="form-group">
    <label for="currencySelect">Currency</label>
    <select id="currencySelect" name="Currency" class="form-control" required>
        <option value="">-- Select Currency --</option>
    </select>
</div>

<script>
    // Load currencies on init
    document.addEventListener('DOMContentLoaded', async function() {
        const tenantId = '@User.GetTenantId()';
        const response = await fetch(`/api/lookups/currencies?tenantId=${tenantId}`);
        const items = await response.json();
        
        const select = document.getElementById('currencySelect');
        items.forEach(item => {
            const option = document.createElement('option');
            option.value = item.key;
            option.textContent = `${item.displayName} (${item.metadata?.symbol || ''})`;
            select.appendChild(option);
        });
    });
</script>
```

## Task 4.2: Create Language Selector Component
**File:** `src/SmartWorkz.StarterKitMVC.Web/Pages/Components/LanguageSelector.cshtml`
```html
<div class="form-group">
    <label for="languageSelect">Language</label>
    <select id="languageSelect" name="Language" class="form-control">
        <option value="">-- Select Language --</option>
    </select>
</div>

<script>
    document.addEventListener('DOMContentLoaded', async function() {
        const tenantId = '@User.GetTenantId()';
        const response = await fetch(`/api/lookups/languages?tenantId=${tenantId}`);
        const items = await response.json();
        
        const select = document.getElementById('languageSelect');
        items.forEach(item => {
            const option = document.createElement('option');
            option.value = item.key;
            option.textContent = item.displayName;
            select.appendChild(option);
        });
    });
</script>
```

## Task 4.3: Create TimeZone Selector Component
**File:** `src/SmartWorkz.StarterKitMVC.Web/Pages/Components/TimeZoneSelector.cshtml`
```html
<div class="form-group">
    <label for="timezonSelect">Time Zone</label>
    <select id="timezonSelect" name="TimeZone" class="form-control">
        <option value="">-- Select Time Zone --</option>
    </select>
</div>

<script>
    document.addEventListener('DOMContentLoaded', async function() {
        const response = await fetch('/api/lookups/timezones');  // Global, no tenant filter
        const items = await response.json();
        
        const select = document.getElementById('timezonSelect');
        items.forEach(item => {
            const option = document.createElement('option');
            option.value = item.key;
            option.textContent = item.displayName;
            select.appendChild(option);
        });
    });
</script>
```

## Task 4.4: Integrate in User Profile Page
**File:** `src/SmartWorkz.StarterKitMVC.Web/Pages/Account/Profile.cshtml`
```html
@page
@model ProfileModel

<form method="post">
    <div class="row">
        <div class="col-md-6">
            <partial name="Components/CurrencySelector" />
            <partial name="Components/LanguageSelector" />
            <partial name="Components/TimeZoneSelector" />
        </div>
    </div>
    
    <button type="submit" class="btn btn-primary mt-3">Save Preferences</button>
</form>
```

**Page Model:** `Profile.cshtml.cs`
```csharp
public class ProfileModel : PageModel
{
    [BindProperty]
    public string Currency { get; set; }
    
    [BindProperty]
    public string Language { get; set; }
    
    [BindProperty]
    public string TimeZone { get; set; }
    
    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userService.GetCurrentUserAsync();
        user.PreferredCurrency = Currency;
        user.PreferredLanguage = Language;
        user.PreferredTimeZone = TimeZone;
        
        await _userService.UpdateAsync(user);
        
        return RedirectToPage();
    }
}
```

---

# PHASE 5: TESTING & DEPLOYMENT

## Task 5.1: Unit Tests - Repository
```csharp
public class LovRepositoryV2Tests
{
    [Fact]
    public async Task GetByTenantHierarchy_ReturnsGlobalAndTenantItems()
    {
        // Arrange
        var repo = new LovRepositoryV2(_db);
        
        // Act
        var items = await repo.GetByTenantHierarchy("currencies", "ABC-US");
        
        // Assert
        Assert.Contains(items, x => x.IsGlobalScope && x.Key == "USD");  // Global
        Assert.Contains(items, x => x.TenantId == "ABC" && x.Key == "EUR");  // Parent
        Assert.Contains(items, x => x.TenantId == "ABC-US" && x.Key == "INR");  // Sub-tenant
    }
    
    [Fact]
    public async Task Upsert_InsertsNewItem()
    {
        // Arrange
        var repo = new LovRepositoryV2(_db);
        var item = new LovItemV2 { Id = Guid.NewGuid(), Key = "AUD", ... };
        
        // Act
        var result = await repo.Upsert(item);
        
        // Assert
        Assert.Equal(1, result);
    }
}
```

## Task 5.2: Integration Tests - Service
```csharp
public class LovServiceV2Tests
{
    [Fact]
    public async Task GetCurrencies_ReturnsCurrenciesForTenant()
    {
        // Integration test with real DB
        var service = new LovServiceV2(_repository);
        
        var items = await service.GetCurrencies("ABC-US");
        
        Assert.NotEmpty(items);
        Assert.All(items, x => Assert.Equal("currencies", x.CategoryKey));
    }
}
```

## Task 5.3: E2E Tests - UI
```javascript
// Admin page: Add currency
describe('Admin Lookups', () => {
    it('should create new currency', async () => {
        await page.goto('/admin/lookups');
        await page.click('button:has-text("New Lookup")');
        
        await page.selectOption('#categorySelect', 'currencies');
        await page.fill('#keyInput', 'BRL');
        await page.fill('#displayNameInput', 'Brazilian Real');
        await page.fill('#metadataInput', '{"symbol":"R$","decimalPlaces":2}');
        
        await page.click('button:has-text("Save")');
        
        await expect(page.locator('text=BRL')).toBeVisible();
    });
});
```

## Task 5.4: Deployment Checklist
- [ ] Run all database scripts in order (01-10)
- [ ] Verify LoV.LovItems table created with correct indexes
- [ ] Seed system lookups (1-999)
- [ ] Deploy backend (DLL)
- [ ] Register Dapper services in DI container
- [ ] Deploy Admin UI pages
- [ ] Deploy Public UI components
- [ ] Smoke test: Load currency dropdown
- [ ] Smoke test: Admin can create lookup
- [ ] Monitor: Check for any errors in logs
- [ ] Document: Update internal wiki with v2 migration info

---

## Summary

This implementation plan covers:
1. **Database:** Schema, tables, procedures, seed data
2. **Backend:** Dapper repository, service layer, API endpoints
3. **Admin UI:** Manage, create, edit, delete, import lookups
4. **Public UI:** Currency, Language, TimeZone selectors
5. **Testing:** Unit, integration, E2E tests
6. **Deployment:** Complete checklist

Total effort: ~40-50 hours (depending on team size & experience with Dapper)

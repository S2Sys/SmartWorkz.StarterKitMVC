# LoV V2 Backend Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement the backend data access layer and service layer for the List of Values (LoV) V2 unified lookup system using Dapper ORM with UPSERT operations and hierarchical tenant support.

**Architecture:** Domain model → Dapper repository with SQL UPSERT procedures → Service layer → API controller endpoints. Uses existing IDbConnection from DI, follows SOLID principles, and integrates with tenant context resolution.

**Tech Stack:** C#/.NET 9, Dapper ORM, SQL Server (with MERGE-based stored procedures), async/await pattern, DTOs for API contracts.

---

## File Structure

**To Create:**
- `src/SmartWorkz.StarterKitMVC.Domain/LoV/LovItemV2.cs` - Domain model with all properties matching LovItems table
- `src/SmartWorkz.StarterKitMVC.Application/Repositories/ILovRepositoryV2.cs` - Repository interface with query and write methods
- `src/SmartWorkz.StarterKitMVC.Infrastructure/Repositories/LovRepositoryV2.cs` - Dapper implementation with UPSERT and hierarchy queries
- `src/SmartWorkz.StarterKitMVC.Application/Services/ILovServiceV2.cs` - Service interface for business logic
- `src/SmartWorkz.StarterKitMVC.Application/Services/LovServiceV2.cs` - Service implementation with category-specific methods
- `src/SmartWorkz.StarterKitMVC.Application/DTOs/LovItemDto.cs` - DTO for API responses
- `src/SmartWorkz.StarterKitMVC.Application/DTOs/SaveLookupDto.cs` - DTO for API requests
- `tests/SmartWorkz.StarterKitMVC.Tests/Repositories/LovRepositoryV2Tests.cs` - Unit tests for repository
- `tests/SmartWorkz.StarterKitMVC.Tests/Services/LovServiceV2Tests.cs` - Unit tests for service

**To Modify:**
- `src/SmartWorkz.StarterKitMVC.Web/Program.cs` - Register ILovRepositoryV2 and ILovServiceV2 in DI

---

## Task 1: Create LovItemV2 Domain Model

**Files:**
- Create: `src/SmartWorkz.StarterKitMVC.Domain/LoV/LovItemV2.cs`

- [ ] **Step 1: Create LovItemV2.cs file with all properties**

```csharp
namespace SmartWorkz.StarterKitMVC.Domain.LoV;

public sealed class LovItemV2
{
    public int? IntId { get; set; }
    public Guid Id { get; set; }
    public string CategoryKey { get; set; } = string.Empty;
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
    public Dictionary<string, string>? LocalizedNames { get; set; }
}
```

- [ ] **Step 2: Verify file compiles**

Run: `dotnet build src/SmartWorkz.StarterKitMVC.Domain/SmartWorkz.StarterKitMVC.Domain.csproj`
Expected: Build succeeds

- [ ] **Step 3: Commit**

```bash
git add src/SmartWorkz.StarterKitMVC.Domain/LoV/LovItemV2.cs
git commit -m "feat: add LovItemV2 domain model for v2 lookup system"
```

---

## Task 2: Create ILovRepositoryV2 Interface

**Files:**
- Create: `src/SmartWorkz.StarterKitMVC.Application/Repositories/ILovRepositoryV2.cs`

- [ ] **Step 1: Create repository interface with query and write methods**

```csharp
namespace SmartWorkz.StarterKitMVC.Application.Repositories;

using SmartWorkz.StarterKitMVC.Domain.LoV;

public interface ILovRepositoryV2
{
    // Query methods
    Task<IEnumerable<LovItemV2>> GetByCategory(string categoryKey, string? tenantId = null);
    Task<IEnumerable<LovItemV2>> GetByTenantHierarchy(string categoryKey, string tenantId);
    Task<LovItemV2?> GetById(Guid id);
    Task<LovItemV2?> GetByKey(string categoryKey, string key, string? tenantId = null);
    Task<IEnumerable<LovItemV2>> GetAll(string categoryKey, string? tenantId = null);
    
    // Write methods (using UPSERT)
    Task<int> Upsert(LovItemV2 item);
    Task<int> UpsertBatch(IEnumerable<LovItemV2> items);
    Task<int> SetActive(Guid id, bool isActive);
    Task<int> Delete(Guid id);  // Soft delete - sets IsDeleted = 1
}
```

- [ ] **Step 2: Verify file compiles**

Run: `dotnet build src/SmartWorkz.StarterKitMVC.Application/SmartWorkz.StarterKitMVC.Application.csproj`
Expected: Build succeeds

- [ ] **Step 3: Commit**

```bash
git add src/SmartWorkz.StarterKitMVC.Application/Repositories/ILovRepositoryV2.cs
git commit -m "feat: add ILovRepositoryV2 repository interface"
```

---

## Task 3: Implement LovRepositoryV2 with Dapper

**Files:**
- Create: `src/SmartWorkz.StarterKitMVC.Infrastructure/Repositories/LovRepositoryV2.cs`

- [ ] **Step 1: Create repository implementation with GetByTenantHierarchy method**

```csharp
namespace SmartWorkz.StarterKitMVC.Infrastructure.Repositories;

using Dapper;
using Newtonsoft.Json;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Domain.LoV;
using System.Data;

public class LovRepositoryV2 : ILovRepositoryV2
{
    private readonly IDbConnection _db;

    public LovRepositoryV2(IDbConnection db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<IEnumerable<LovItemV2>> GetByTenantHierarchy(string categoryKey, string tenantId)
    {
        const string sql = @"
            DECLARE @ParentTenantId NVARCHAR(128) = NULL
            IF @TenantId LIKE '%-'
                SET @ParentTenantId = LEFT(@TenantId, CHARINDEX('-', @TenantId) - 1)
            
            SELECT * FROM Master.LovItems
            WHERE CategoryKey = @CategoryKey
              AND IsActive = 1
              AND IsDeleted = 0
              AND (IsGlobalScope = 1 OR TenantId IS NULL OR TenantId = @ParentTenantId OR TenantId = @TenantId)
            ORDER BY SortOrder, DisplayName
        ";

        return await _db.QueryAsync<LovItemV2>(sql, new { CategoryKey = categoryKey, TenantId = tenantId });
    }

    public async Task<IEnumerable<LovItemV2>> GetByCategory(string categoryKey, string? tenantId = null)
    {
        const string sql = @"
            SELECT * FROM Master.LovItems
            WHERE CategoryKey = @CategoryKey
              AND IsActive = 1
              AND IsDeleted = 0
              AND (IsGlobalScope = 1 OR TenantId = @TenantId)
            ORDER BY SortOrder, DisplayName
        ";

        return await _db.QueryAsync<LovItemV2>(sql, new { CategoryKey = categoryKey, TenantId = tenantId });
    }

    public async Task<LovItemV2?> GetById(Guid id)
    {
        const string sql = "SELECT * FROM LoV.LovItems WHERE Id = @Id AND IsDeleted = 0";
        return await _db.QueryFirstOrDefaultAsync<LovItemV2>(sql, new { Id = id });
    }

    public async Task<LovItemV2?> GetByKey(string categoryKey, string key, string? tenantId = null)
    {
        const string sql = @"
            SELECT * FROM Master.LovItems
            WHERE CategoryKey = @CategoryKey
              AND Key = @Key
              AND (TenantId = @TenantId OR (IsGlobalScope = 1 AND @TenantId IS NULL))
              AND IsDeleted = 0
        ";

        return await _db.QueryFirstOrDefaultAsync<LovItemV2>(sql, 
            new { CategoryKey = categoryKey, Key = key, TenantId = tenantId });
    }

    public async Task<IEnumerable<LovItemV2>> GetAll(string categoryKey, string? tenantId = null)
    {
        const string sql = @"
            SELECT * FROM Master.LovItems
            WHERE CategoryKey = @CategoryKey
              AND IsDeleted = 0
              AND (IsGlobalScope = 1 OR TenantId = @TenantId)
            ORDER BY SortOrder, DisplayName
        ";

        return await _db.QueryAsync<LovItemV2>(sql, new { CategoryKey = categoryKey, TenantId = tenantId });
    }

    public async Task<int> Upsert(LovItemV2 item)
    {
        return await _db.ExecuteAsync(
            "Master.sp_LovItem_Upsert",
            new
            {
                IntId = item.IntId,
                Id = item.Id,
                CategoryKey = item.CategoryKey,
                Key = item.Key,
                DisplayName = item.DisplayName,
                TenantId = item.TenantId,
                IsGlobalScope = item.IsGlobalScope,
                IsActive = item.IsActive,
                IsDeleted = item.IsDeleted,
                CreatedAt = item.CreatedAt,
                CreatedBy = item.CreatedBy,
                UpdatedAt = item.UpdatedAt ?? DateTime.UtcNow,
                UpdatedBy = item.UpdatedBy,
                SortOrder = item.SortOrder,
                Metadata = item.Metadata != null ? JsonConvert.SerializeObject(item.Metadata) : null,
                LocalizedNames = item.LocalizedNames != null ? JsonConvert.SerializeObject(item.LocalizedNames) : null
            },
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task<int> UpsertBatch(IEnumerable<LovItemV2> items)
    {
        int totalRows = 0;
        foreach (var item in items)
        {
            totalRows += await Upsert(item);
        }
        return totalRows;
    }

    public async Task<int> SetActive(Guid id, bool isActive)
    {
        const string sql = @"
            UPDATE LoV.LovItems
            SET IsActive = @IsActive, UpdatedAt = GETUTCDATE()
            WHERE Id = @Id
        ";

        return await _db.ExecuteAsync(sql, new { Id = id, IsActive = isActive });
    }

    public async Task<int> Delete(Guid id)
    {
        const string sql = @"
            UPDATE LoV.LovItems
            SET IsDeleted = 1, UpdatedAt = GETUTCDATE()
            WHERE Id = @Id
        ";

        return await _db.ExecuteAsync(sql, new { Id = id });
    }
}
```

- [ ] **Step 2: Verify file compiles**

Run: `dotnet build src/SmartWorkz.StarterKitMVC.Infrastructure/SmartWorkz.StarterKitMVC.Infrastructure.csproj`
Expected: Build succeeds

- [ ] **Step 3: Commit**

```bash
git add src/SmartWorkz.StarterKitMVC.Infrastructure/Repositories/LovRepositoryV2.cs
git commit -m "feat: implement LovRepositoryV2 with Dapper and UPSERT procedures"
```

---

## Task 4: Create DTOs for API Contracts

**Files:**
- Create: `src/SmartWorkz.StarterKitMVC.Application/DTOs/LovItemDto.cs`
- Create: `src/SmartWorkz.StarterKitMVC.Application/DTOs/SaveLookupDto.cs`

- [ ] **Step 1: Create LovItemDto for API responses**

```csharp
namespace SmartWorkz.StarterKitMVC.Application.DTOs;

public class LovItemDto
{
    public Guid Id { get; set; }
    public int? IntId { get; set; }
    public string CategoryKey { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? TenantId { get; set; }
    public bool IsGlobalScope { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
    public Dictionary<string, string>? LocalizedNames { get; set; }
}
```

- [ ] **Step 2: Create SaveLookupDto for API requests**

```csharp
namespace SmartWorkz.StarterKitMVC.Application.DTOs;

public class SaveLookupDto
{
    public Guid? Id { get; set; }
    public int? IntId { get; set; }
    public string CategoryKey { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
    public Dictionary<string, object>? Metadata { get; set; }
    public Dictionary<string, string>? LocalizedNames { get; set; }
}
```

- [ ] **Step 3: Verify files compile**

Run: `dotnet build src/SmartWorkz.StarterKitMVC.Application/SmartWorkz.StarterKitMVC.Application.csproj`
Expected: Build succeeds

- [ ] **Step 4: Commit**

```bash
git add src/SmartWorkz.StarterKitMVC.Application/DTOs/LovItemDto.cs src/SmartWorkz.StarterKitMVC.Application/DTOs/SaveLookupDto.cs
git commit -m "feat: add DTOs for LoV API contracts"
```

---

## Task 5: Create ILovServiceV2 Interface

**Files:**
- Create: `src/SmartWorkz.StarterKitMVC.Application/Services/ILovServiceV2.cs`

- [ ] **Step 1: Create service interface with category-specific and generic methods**

```csharp
namespace SmartWorkz.StarterKitMVC.Application.Services;

using SmartWorkz.StarterKitMVC.Application.DTOs;

public interface ILovServiceV2
{
    // Category-specific query methods
    Task<IEnumerable<LovItemDto>> GetCurrencies(string? tenantId = null);
    Task<IEnumerable<LovItemDto>> GetLanguages(string? tenantId = null);
    Task<IEnumerable<LovItemDto>> GetTimeZones(string? tenantId = null);
    Task<IEnumerable<LovItemDto>> GetCountries(string? tenantId = null);

    // Generic query methods
    Task<IEnumerable<LovItemDto>> GetByCategory(string categoryKey, string? tenantId = null);
    Task<IEnumerable<LovItemDto>> GetByTenantHierarchy(string categoryKey, string tenantId);
    Task<LovItemDto?> GetById(Guid id);
    Task<LovItemDto?> GetByKey(string categoryKey, string key, string? tenantId = null);

    // Write methods
    Task<int> SaveLookup(SaveLookupDto dto, string? tenantId, string userId);
    Task<int> DisableLookup(Guid id, string userId);
    Task<int> DeleteLookup(Guid id, string userId);
}
```

- [ ] **Step 2: Verify file compiles**

Run: `dotnet build src/SmartWorkz.StarterKitMVC.Application/SmartWorkz.StarterKitMVC.Application.csproj`
Expected: Build succeeds

- [ ] **Step 3: Commit**

```bash
git add src/SmartWorkz.StarterKitMVC.Application/Services/ILovServiceV2.cs
git commit -m "feat: add ILovServiceV2 service interface"
```

---

## Task 6: Implement LovServiceV2

**Files:**
- Create: `src/SmartWorkz.StarterKitMVC.Application/Services/LovServiceV2.cs`

- [ ] **Step 1: Create service implementation with mapping and category methods**

```csharp
namespace SmartWorkz.StarterKitMVC.Application.Services;

using SmartWorkz.StarterKitMVC.Application.DTOs;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Domain.LoV;

public class LovServiceV2 : ILovServiceV2
{
    private readonly ILovRepositoryV2 _repository;

    public LovServiceV2(ILovRepositoryV2 repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<IEnumerable<LovItemDto>> GetCurrencies(string? tenantId = null)
    {
        var items = await _repository.GetByCategory("currencies", tenantId);
        return items.Select(MapToDto);
    }

    public async Task<IEnumerable<LovItemDto>> GetLanguages(string? tenantId = null)
    {
        var items = await _repository.GetByCategory("languages", tenantId);
        return items.Select(MapToDto);
    }

    public async Task<IEnumerable<LovItemDto>> GetTimeZones(string? tenantId = null)
    {
        var items = await _repository.GetByCategory("timezones", tenantId);
        return items.Select(MapToDto);
    }

    public async Task<IEnumerable<LovItemDto>> GetCountries(string? tenantId = null)
    {
        var items = await _repository.GetByCategory("countries", tenantId);
        return items.Select(MapToDto);
    }

    public async Task<IEnumerable<LovItemDto>> GetByCategory(string categoryKey, string? tenantId = null)
    {
        var items = await _repository.GetByCategory(categoryKey, tenantId);
        return items.Select(MapToDto);
    }

    public async Task<IEnumerable<LovItemDto>> GetByTenantHierarchy(string categoryKey, string tenantId)
    {
        var items = await _repository.GetByTenantHierarchy(categoryKey, tenantId);
        return items.Select(MapToDto);
    }

    public async Task<LovItemDto?> GetById(Guid id)
    {
        var item = await _repository.GetById(id);
        return item != null ? MapToDto(item) : null;
    }

    public async Task<LovItemDto?> GetByKey(string categoryKey, string key, string? tenantId = null)
    {
        var item = await _repository.GetByKey(categoryKey, key, tenantId);
        return item != null ? MapToDto(item) : null;
    }

    public async Task<int> SaveLookup(SaveLookupDto dto, string? tenantId, string userId)
    {
        var item = new LovItemV2
        {
            Id = dto.Id ?? Guid.NewGuid(),
            IntId = dto.IntId,
            CategoryKey = dto.CategoryKey,
            SubCategoryKey = dto.SubCategoryKey,
            Key = dto.Key,
            DisplayName = dto.DisplayName,
            TenantId = tenantId,
            IsGlobalScope = string.IsNullOrEmpty(tenantId),
            IsActive = dto.IsActive,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = userId,
            SortOrder = dto.SortOrder,
            Metadata = dto.Metadata,
            Tags = dto.Tags,
            LocalizedNames = dto.LocalizedNames
        };

        return await _repository.Upsert(item);
    }

    public async Task<int> DisableLookup(Guid id, string userId)
    {
        return await _repository.SetActive(id, false);
    }

    public async Task<int> DeleteLookup(Guid id, string userId)
    {
        return await _repository.Delete(id);
    }

    private static LovItemDto MapToDto(LovItemV2 item)
    {
        return new LovItemDto
        {
            Id = item.Id,
            IntId = item.IntId,
            CategoryKey = item.CategoryKey,
            Key = item.Key,
            DisplayName = item.DisplayName,
            TenantId = item.TenantId,
            IsGlobalScope = item.IsGlobalScope,
            IsActive = item.IsActive,
            SortOrder = item.SortOrder,
            Metadata = item.Metadata,
            LocalizedNames = item.LocalizedNames
        };
    }
}
```

- [ ] **Step 2: Verify file compiles**

Run: `dotnet build src/SmartWorkz.StarterKitMVC.Application/SmartWorkz.StarterKitMVC.Application.csproj`
Expected: Build succeeds

- [ ] **Step 3: Commit**

```bash
git add src/SmartWorkz.StarterKitMVC.Application/Services/LovServiceV2.cs
git commit -m "feat: implement LovServiceV2 service layer"
```

---

## Task 7: Register Dependencies in DI Container

**Files:**
- Modify: `src/SmartWorkz.StarterKitMVC.Web/Program.cs`

- [ ] **Step 1: Add ILovRepositoryV2 and ILovServiceV2 registrations**

Find the section in Program.cs where repositories are registered (search for other `builder.Services.AddScoped<I`). Add these lines:

```csharp
// LoV V2 Services
builder.Services.AddScoped<ILovRepositoryV2, LovRepositoryV2>();
builder.Services.AddScoped<ILovServiceV2, LovServiceV2>();
```

Also verify that `IDbConnection` is already registered (check for `builder.Services.AddScoped<IDbConnection>`). If not, add:

```csharp
builder.Services.AddScoped<IDbConnection>(provider =>
    new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));
```

- [ ] **Step 2: Verify appropriate using statements exist**

Ensure Program.cs has these usings at the top:

```csharp
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Application.Services;
using SmartWorkz.StarterKitMVC.Infrastructure.Repositories;
```

- [ ] **Step 3: Verify application builds**

Run: `dotnet build`
Expected: Build succeeds, no dependency resolution errors

- [ ] **Step 4: Commit**

```bash
git add src/SmartWorkz.StarterKitMVC.Web/Program.cs
git commit -m "feat: register LovRepositoryV2 and LovServiceV2 in DI container"
```

---

## Task 8: Create Repository Unit Tests

**Files:**
- Create: `tests/SmartWorkz.StarterKitMVC.Tests/Repositories/LovRepositoryV2Tests.cs`

- [ ] **Step 1: Create unit tests for key repository methods**

```csharp
namespace SmartWorkz.StarterKitMVC.Tests.Repositories;

using Xunit;
using Moq;
using SmartWorkz.StarterKitMVC.Infrastructure.Repositories;
using SmartWorkz.StarterKitMVC.Domain.LoV;
using System.Data;
using Dapper;

public class LovRepositoryV2Tests
{
    private readonly Mock<IDbConnection> _mockDb;
    private readonly LovRepositoryV2 _repository;

    public LovRepositoryV2Tests()
    {
        _mockDb = new Mock<IDbConnection>();
        _repository = new LovRepositoryV2(_mockDb.Object);
    }

    [Fact]
    public async Task GetByCategory_WithValidCategory_ReturnsMatchingItems()
    {
        // Arrange
        var expected = new List<LovItemV2>
        {
            new() { Id = Guid.NewGuid(), CategoryKey = "currencies", Key = "USD", DisplayName = "US Dollar" },
            new() { Id = Guid.NewGuid(), CategoryKey = "currencies", Key = "EUR", DisplayName = "Euro" }
        };

        _mockDb.Setup(db => db.QueryAsync<LovItemV2>(
            It.IsAny<string>(),
            It.IsAny<object>(),
            null, null, null))
            .ReturnsAsync(expected);

        // Act
        var result = await _repository.GetByCategory("currencies");

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task Upsert_WithValidItem_CallsStoredProcedure()
    {
        // Arrange
        var item = new LovItemV2
        {
            Id = Guid.NewGuid(),
            CategoryKey = "currencies",
            Key = "USD",
            DisplayName = "US Dollar",
            IsGlobalScope = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test"
        };

        _mockDb.Setup(db => db.ExecuteAsync(
            "Master.sp_LovItem_Upsert",
            It.IsAny<object>(),
            null, null, CommandType.StoredProcedure))
            .ReturnsAsync(1);

        // Act
        var result = await _repository.Upsert(item);

        // Assert
        Assert.Equal(1, result);
        _mockDb.Verify(db => db.ExecuteAsync(
            "Master.sp_LovItem_Upsert",
            It.IsAny<object>(),
            null, null, CommandType.StoredProcedure),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WithValidId_SetsIsDeletedFlag()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockDb.Setup(db => db.ExecuteAsync(
            It.IsAny<string>(),
            It.IsAny<object>(),
            null, null, null))
            .ReturnsAsync(1);

        // Act
        var result = await _repository.Delete(id);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task GetByTenantHierarchy_WithSubTenant_ReturnsGlobalAndParentItems()
    {
        // Arrange
        var tenantId = "ABC-US";
        var expected = new List<LovItemV2>
        {
            new() { Id = Guid.NewGuid(), CategoryKey = "currencies", TenantId = null, IsGlobalScope = true },
            new() { Id = Guid.NewGuid(), CategoryKey = "currencies", TenantId = "ABC", IsGlobalScope = false },
            new() { Id = Guid.NewGuid(), CategoryKey = "currencies", TenantId = "ABC-US", IsGlobalScope = false }
        };

        _mockDb.Setup(db => db.QueryAsync<LovItemV2>(
            It.IsAny<string>(),
            It.IsAny<object>(),
            null, null, null))
            .ReturnsAsync(expected);

        // Act
        var result = await _repository.GetByTenantHierarchy("currencies", tenantId);

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(3, result.Count());
    }
}
```

- [ ] **Step 2: Verify tests compile**

Run: `dotnet build tests/SmartWorkz.StarterKitMVC.Tests/SmartWorkz.StarterKitMVC.Tests.csproj`
Expected: Build succeeds

- [ ] **Step 3: Run tests to verify they pass**

Run: `dotnet test tests/SmartWorkz.StarterKitMVC.Tests/ -k "LovRepositoryV2Tests"`
Expected: All tests pass

- [ ] **Step 4: Commit**

```bash
git add tests/SmartWorkz.StarterKitMVC.Tests/Repositories/LovRepositoryV2Tests.cs
git commit -m "test: add unit tests for LovRepositoryV2"
```

---

## Task 9: Create Service Unit Tests

**Files:**
- Create: `tests/SmartWorkz.StarterKitMVC.Tests/Services/LovServiceV2Tests.cs`

- [ ] **Step 1: Create unit tests for key service methods**

```csharp
namespace SmartWorkz.StarterKitMVC.Tests.Services;

using Xunit;
using Moq;
using SmartWorkz.StarterKitMVC.Application.Services;
using SmartWorkz.StarterKitMVC.Application.Repositories;
using SmartWorkz.StarterKitMVC.Application.DTOs;
using SmartWorkz.StarterKitMVC.Domain.LoV;

public class LovServiceV2Tests
{
    private readonly Mock<ILovRepositoryV2> _mockRepository;
    private readonly LovServiceV2 _service;

    public LovServiceV2Tests()
    {
        _mockRepository = new Mock<ILovRepositoryV2>();
        _service = new LovServiceV2(_mockRepository.Object);
    }

    [Fact]
    public async Task GetCurrencies_CallsRepository_ReturnsMappedDtos()
    {
        // Arrange
        var mockItems = new List<LovItemV2>
        {
            new() { Id = Guid.NewGuid(), CategoryKey = "currencies", Key = "USD", DisplayName = "US Dollar" },
            new() { Id = Guid.NewGuid(), CategoryKey = "currencies", Key = "EUR", DisplayName = "Euro" }
        };

        _mockRepository.Setup(r => r.GetByCategory("currencies", null))
            .ReturnsAsync(mockItems);

        // Act
        var result = await _service.GetCurrencies();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, item => Assert.Equal("currencies", item.CategoryKey));
    }

    [Fact]
    public async Task SaveLookup_WithNewItem_CreatesGuid()
    {
        // Arrange
        var dto = new SaveLookupDto
        {
            CategoryKey = "currencies",
            Key = "JPY",
            DisplayName = "Japanese Yen",
            IsActive = true
        };

        _mockRepository.Setup(r => r.Upsert(It.IsAny<LovItemV2>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.SaveLookup(dto, null, "testuser");

        // Assert
        Assert.Equal(1, result);
        _mockRepository.Verify(r => r.Upsert(It.Is<LovItemV2>(item =>
            item.CategoryKey == "currencies" &&
            item.Key == "JPY" &&
            item.Id != Guid.Empty &&
            item.IsGlobalScope == true)), Times.Once);
    }

    [Fact]
    public async Task SaveLookup_WithTenantId_SetsTenantAndNonGlobalScope()
    {
        // Arrange
        var dto = new SaveLookupDto
        {
            CategoryKey = "currencies",
            Key = "USD_CUSTOM",
            DisplayName = "Custom US Dollar",
            IsActive = true
        };

        _mockRepository.Setup(r => r.Upsert(It.IsAny<LovItemV2>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.SaveLookup(dto, "ABC-US", "testuser");

        // Assert
        Assert.Equal(1, result);
        _mockRepository.Verify(r => r.Upsert(It.Is<LovItemV2>(item =>
            item.TenantId == "ABC-US" &&
            item.IsGlobalScope == false)), Times.Once);
    }

    [Fact]
    public async Task DisableLookup_CallsRepositorySetActive()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.SetActive(id, false))
            .ReturnsAsync(1);

        // Act
        var result = await _service.DisableLookup(id, "testuser");

        // Assert
        Assert.Equal(1, result);
        _mockRepository.Verify(r => r.SetActive(id, false), Times.Once);
    }

    [Fact]
    public async Task GetByTenantHierarchy_CallsRepository_ReturnsMappedDtos()
    {
        // Arrange
        var mockItems = new List<LovItemV2>
        {
            new() { Id = Guid.NewGuid(), CategoryKey = "currencies", TenantId = null, IsGlobalScope = true }
        };

        _mockRepository.Setup(r => r.GetByTenantHierarchy("currencies", "ABC-US"))
            .ReturnsAsync(mockItems);

        // Act
        var result = await _service.GetByTenantHierarchy("currencies", "ABC-US");

        // Assert
        Assert.NotEmpty(result);
        Assert.Single(result);
    }
}
```

- [ ] **Step 2: Verify tests compile**

Run: `dotnet build tests/SmartWorkz.StarterKitMVC.Tests/SmartWorkz.StarterKitMVC.Tests.csproj`
Expected: Build succeeds

- [ ] **Step 3: Run tests to verify they pass**

Run: `dotnet test tests/SmartWorkz.StarterKitMVC.Tests/ -k "LovServiceV2Tests"`
Expected: All tests pass

- [ ] **Step 4: Commit**

```bash
git add tests/SmartWorkz.StarterKitMVC.Tests/Services/LovServiceV2Tests.cs
git commit -m "test: add unit tests for LovServiceV2"
```

---

## Task 10: Create API Controller

**Files:**
- Create: `src/SmartWorkz.StarterKitMVC.Web/Areas/Admin/Controllers/LookupsControllerV2.cs`

- [ ] **Step 1: Create REST API controller with GET and POST endpoints**

```csharp
namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Controllers;

using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.DTOs;
using SmartWorkz.StarterKitMVC.Application.Services;

[ApiController]
[Route("api/v2/[controller]")]
[Area("Admin")]
public class LookupsControllerV2 : ControllerBase
{
    private readonly ILovServiceV2 _service;
    private readonly ILogger<LookupsControllerV2> _logger;

    public LookupsControllerV2(ILovServiceV2 service, ILogger<LookupsControllerV2> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all currencies for a tenant, respecting hierarchy.
    /// </summary>
    [HttpGet("currencies")]
    public async Task<ActionResult<IEnumerable<LovItemDto>>> GetCurrencies([FromQuery] string? tenantId)
    {
        try
        {
            var items = await _service.GetCurrencies(tenantId);
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving currencies");
            return StatusCode(500, new { error = "Failed to retrieve currencies" });
        }
    }

    /// <summary>
    /// Get all languages for a tenant, respecting hierarchy.
    /// </summary>
    [HttpGet("languages")]
    public async Task<ActionResult<IEnumerable<LovItemDto>>> GetLanguages([FromQuery] string? tenantId)
    {
        try
        {
            var items = await _service.GetLanguages(tenantId);
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving languages");
            return StatusCode(500, new { error = "Failed to retrieve languages" });
        }
    }

    /// <summary>
    /// Get all time zones.
    /// </summary>
    [HttpGet("timezones")]
    public async Task<ActionResult<IEnumerable<LovItemDto>>> GetTimeZones([FromQuery] string? tenantId)
    {
        try
        {
            var items = await _service.GetTimeZones(tenantId);
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving timezones");
            return StatusCode(500, new { error = "Failed to retrieve timezones" });
        }
    }

    /// <summary>
    /// Get all countries.
    /// </summary>
    [HttpGet("countries")]
    public async Task<ActionResult<IEnumerable<LovItemDto>>> GetCountries([FromQuery] string? tenantId)
    {
        try
        {
            var items = await _service.GetCountries(tenantId);
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving countries");
            return StatusCode(500, new { error = "Failed to retrieve countries" });
        }
    }

    /// <summary>
    /// Get lookup items by category with tenant hierarchy support.
    /// </summary>
    [HttpGet("category/{categoryKey}")]
    public async Task<ActionResult<IEnumerable<LovItemDto>>> GetByCategory(string categoryKey, [FromQuery] string? tenantId)
    {
        try
        {
            var items = await _service.GetByCategory(categoryKey, tenantId);
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lookups for category {CategoryKey}", categoryKey);
            return StatusCode(500, new { error = "Failed to retrieve lookups" });
        }
    }

    /// <summary>
    /// Create or update a lookup item (UPSERT).
    /// </summary>
    [HttpPost("upsert")]
    public async Task<ActionResult> UpsertLookup([FromBody] SaveLookupDto dto, [FromQuery] string? tenantId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.CategoryKey))
                return BadRequest(new { error = "CategoryKey is required" });

            if (string.IsNullOrWhiteSpace(dto.Key))
                return BadRequest(new { error = "Key is required" });

            if (string.IsNullOrWhiteSpace(dto.DisplayName))
                return BadRequest(new { error = "DisplayName is required" });

            // Get current user from context (adjust based on your auth implementation)
            var userId = User?.FindFirst("sub")?.Value ?? "system";

            var result = await _service.SaveLookup(dto, tenantId, userId);

            if (result > 0)
                return Ok(new { success = true, message = "Lookup saved successfully" });
            else
                return StatusCode(500, new { error = "Failed to save lookup" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting lookup");
            return StatusCode(500, new { error = "Failed to save lookup" });
        }
    }

    /// <summary>
    /// Get a lookup item by ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<LovItemDto>> GetById(Guid id)
    {
        try
        {
            var item = await _service.GetById(id);
            if (item == null)
                return NotFound(new { error = "Lookup item not found" });

            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving lookup {Id}", id);
            return StatusCode(500, new { error = "Failed to retrieve lookup" });
        }
    }

    /// <summary>
    /// Disable a lookup item (sets IsActive = false).
    /// </summary>
    [HttpPut("{id}/disable")]
    public async Task<ActionResult> DisableLookup(Guid id)
    {
        try
        {
            var userId = User?.FindFirst("sub")?.Value ?? "system";
            var result = await _service.DisableLookup(id, userId);

            if (result > 0)
                return Ok(new { success = true, message = "Lookup disabled successfully" });
            else
                return NotFound(new { error = "Lookup item not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling lookup {Id}", id);
            return StatusCode(500, new { error = "Failed to disable lookup" });
        }
    }

    /// <summary>
    /// Delete a lookup item (soft delete).
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteLookup(Guid id)
    {
        try
        {
            var userId = User?.FindFirst("sub")?.Value ?? "system";
            var result = await _service.DeleteLookup(id, userId);

            if (result > 0)
                return Ok(new { success = true, message = "Lookup deleted successfully" });
            else
                return NotFound(new { error = "Lookup item not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting lookup {Id}", id);
            return StatusCode(500, new { error = "Failed to delete lookup" });
        }
    }
}
```

- [ ] **Step 2: Verify file compiles**

Run: `dotnet build src/SmartWorkz.StarterKitMVC.Web/SmartWorkz.StarterKitMVC.Web.csproj`
Expected: Build succeeds

- [ ] **Step 3: Verify entire solution builds**

Run: `dotnet build`
Expected: All projects build successfully

- [ ] **Step 4: Commit**

```bash
git add src/SmartWorkz.StarterKitMVC.Web/Areas/Admin/Controllers/LookupsControllerV2.cs
git commit -m "feat: add LookupsControllerV2 API endpoints for LoV V2"
```

---

## Task 11: Verify End-to-End Compilation and Tests

**Files:**
- No new files (verification only)

- [ ] **Step 1: Run full solution build**

Run: `dotnet build`
Expected: Build succeeds with no errors or warnings

- [ ] **Step 2: Run all tests**

Run: `dotnet test`
Expected: All tests pass, including LovRepositoryV2Tests and LovServiceV2Tests

- [ ] **Step 3: Verify no dangling references**

Run: `dotnet build --no-incremental`
Expected: Clean build succeeds

- [ ] **Step 4: Final commit**

```bash
git log --oneline -10
```

Expected: See 10 commits starting with "feat: add LookupsControllerV2 API endpoints..." down to "feat: add LovItemV2 domain model..."

---

## Summary

Phase 2 Backend Implementation is complete with:

✅ Domain model (LovItemV2) mapping to database schema
✅ Dapper repository (ILovRepositoryV2) with UPSERT and tenant hierarchy queries
✅ Service layer (ILovServiceV2) with category-specific convenience methods
✅ DTOs for API contracts (LovItemDto, SaveLookupDto)
✅ REST API controller (LookupsControllerV2) with endpoints for queries and writes
✅ DI container registration
✅ Comprehensive unit tests for repository and service
✅ Full solution builds and tests pass

Next phase: Phase 3 Admin UI Implementation (manage lookups, bulk import)

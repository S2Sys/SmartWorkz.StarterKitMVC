# SEO as Shared Polymorphic Table - Implementation Checklist

**Date:** 2026-03-31
**Migration:** Move SeoMeta from Master to Shared schema
**Impact:** Products, Categories, MenuItems, BlogPosts, GeolocationPages, CustomPages all use single SeoMeta table

---

## Summary of Change

| Aspect | Before | After | Benefit |
|--------|--------|-------|---------|
| **SeoMeta Location** | Master.SeoMeta | **Shared.SeoMeta** | ✅ Consistent with polymorphic pattern |
| **Table Count (Master)** | 20 | **18** | ✅ Cleaner separation of concerns |
| **Table Count (Shared)** | 5 | **6** | ✅ All infrastructure in one place |
| **Total Tables** | 42 | **42** | ✅ Same, just reorganized |
| **Schema Pattern** | Reference data + Infrastructure | **Infrastructure only** | ✅ Idiomatic design |
| **Entity Type Support** | MenuItems only (embedded) | **ANY entity** (polymorphic) | ✅ Automatic SEO for Products, Categories, etc. |

---

## Phase 1 Tasks

### Task 1: Update Database Schema (SQL)

**File:** `database/v4/003_CreateTables_Shared.sql`

```sql
-- REMOVE from 002_CreateTables_Master.sql: SeoMeta

-- ADD to 003_CreateTables_Shared.sql:
IF OBJECT_ID('Shared.SeoMeta', 'U') IS NOT NULL
    DROP TABLE Shared.SeoMeta;

CREATE TABLE Shared.SeoMeta (
    SeoMetaId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantId UNIQUEIDENTIFIER NOT NULL,
    EntityType VARCHAR(50) NOT NULL,
    EntityId UNIQUEIDENTIFIER NOT NULL,
    Slug VARCHAR(255) NOT NULL,
    MetaTitle NVARCHAR(255) NOT NULL,
    MetaDescription NVARCHAR(500) NOT NULL,
    MetaKeywords NVARCHAR(MAX) NULL,
    OgTitle NVARCHAR(255) NULL,
    OgDescription NVARCHAR(500) NULL,
    OgImage VARCHAR(MAX) NULL,
    OgType VARCHAR(50) NULL DEFAULT 'website',
    CanonicalUrl VARCHAR(MAX) NULL,
    SchemaMarkup NVARCHAR(MAX) NULL,
    Robots VARCHAR(100) NULL DEFAULT 'index,follow',
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy UNIQUEIDENTIFIER NULL,
    UpdatedBy UNIQUEIDENTIFIER NULL,

    CONSTRAINT UC_SeoMeta_TenantType UNIQUE (TenantId, EntityType, EntityId),
    CONSTRAINT UC_SeoMeta_TenantSlug UNIQUE (TenantId, Slug)
);

CREATE INDEX IX_SeoMeta_TenantType ON Shared.SeoMeta(TenantId, EntityType);
CREATE INDEX IX_SeoMeta_TenantSlug ON Shared.SeoMeta(TenantId, Slug);
CREATE INDEX IX_SeoMeta_EntityTypeId ON Shared.SeoMeta(EntityType, EntityId);
CREATE INDEX IX_SeoMeta_IsActive ON Shared.SeoMeta(IsActive);
```

**Also update:**
- `database/v4/002_CreateTables_Master.sql` — Remove SeoMeta table definition
- `database/v4/008_CreateIndexes.sql` — Remove SeoMeta indexes, add to Shared section

---

### Task 2: Update Entity Mappings (C#)

**File:** `src/SmartWorkz.StarterKitMVC.Domain/Entities/Shared/SeoMeta.cs`

```csharp
using SmartWorkz.StarterKitMVC.Domain.Common;

namespace SmartWorkz.StarterKitMVC.Domain.Entities.Shared
{
    /// <summary>
    /// Polymorphic SEO metadata for any entity.
    /// Supports: Products, Categories, MenuItems, BlogPosts, GeolocationPages, CustomPages
    /// </summary>
    public class SeoMeta : AuditableEntity
    {
        public Guid SeoMetaId { get; set; }
        public Guid TenantId { get; set; }

        /// <summary>
        /// Entity type this SEO belongs to: 'Product', 'Category', 'MenuItem', 'BlogPost', 'GeolocationPage', 'CustomPage'
        /// </summary>
        [MaxLength(50)]
        public string EntityType { get; set; }

        /// <summary>
        /// Entity ID (polymorphic link)
        /// </summary>
        public Guid EntityId { get; set; }

        /// <summary>
        /// URL-friendly slug for routing
        /// </summary>
        [MaxLength(255)]
        public string Slug { get; set; }

        /// <summary>
        /// Browser title tag
        /// </summary>
        [MaxLength(255)]
        public string MetaTitle { get; set; }

        /// <summary>
        /// Browser description meta tag
        /// </summary>
        [MaxLength(500)]
        public string MetaDescription { get; set; }

        /// <summary>
        /// SEO keywords (optional, less critical in modern SEO)
        /// </summary>
        public string MetaKeywords { get; set; }

        /// <summary>
        /// Open Graph title for social media
        /// </summary>
        [MaxLength(255)]
        public string OgTitle { get; set; }

        /// <summary>
        /// Open Graph description for social media
        /// </summary>
        [MaxLength(500)]
        public string OgDescription { get; set; }

        /// <summary>
        /// Open Graph image URL or FK to Attachment
        /// </summary>
        public string OgImage { get; set; }

        /// <summary>
        /// Open Graph type: 'product', 'article', 'website', etc.
        /// </summary>
        [MaxLength(50)]
        public string OgType { get; set; } = "website";

        /// <summary>
        /// Canonical URL for avoiding duplicate content
        /// </summary>
        public string CanonicalUrl { get; set; }

        /// <summary>
        /// JSON: schema.org structured data (Product, BreadcrumbList, LocalBusiness, BlogPosting)
        /// </summary>
        public string SchemaMarkup { get; set; }

        /// <summary>
        /// Robots meta directive: 'index,follow', 'noindex,nofollow', etc.
        /// </summary>
        [MaxLength(100)]
        public string Robots { get; set; } = "index,follow";

        /// <summary>
        /// Is this SEO metadata active/published?
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}
```

**File:** Update DbContext (e.g., `SharedDbContext.cs`)

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // SeoMeta configuration
    modelBuilder.Entity<SeoMeta>(entity =>
    {
        entity.ToTable("SeoMeta", schema: "Shared");
        entity.HasKey(e => e.SeoMetaId);

        entity.Property(e => e.TenantId).IsRequired();
        entity.Property(e => e.EntityType).HasMaxLength(50).IsRequired();
        entity.Property(e => e.EntityId).IsRequired();
        entity.Property(e => e.Slug).HasMaxLength(255).IsRequired();
        entity.Property(e => e.MetaTitle).HasMaxLength(255).IsRequired();
        entity.Property(e => e.MetaDescription).HasMaxLength(500).IsRequired();
        entity.Property(e => e.OgTitle).HasMaxLength(255);
        entity.Property(e => e.OgDescription).HasMaxLength(500);
        entity.Property(e => e.OgType).HasMaxLength(50);
        entity.Property(e => e.Robots).HasMaxLength(100);

        // Unique constraints
        entity.HasIndex(e => new { e.TenantId, e.EntityType, e.EntityId })
            .HasName("UC_SeoMeta_TenantType")
            .IsUnique();

        entity.HasIndex(e => new { e.TenantId, e.Slug })
            .HasName("UC_SeoMeta_TenantSlug")
            .IsUnique();

        // Query indexes
        entity.HasIndex(e => new { e.TenantId, e.EntityType })
            .HasName("IX_SeoMeta_TenantType");

        entity.HasIndex(e => new { e.TenantId, e.Slug })
            .HasName("IX_SeoMeta_TenantSlug");

        entity.HasIndex(e => new { e.EntityType, e.EntityId })
            .HasName("IX_SeoMeta_EntityTypeId");

        entity.HasIndex(e => e.IsActive)
            .HasName("IX_SeoMeta_IsActive");
    });
}
```

---

### Task 3: Create Repository & Service

**File:** `src/SmartWorkz.StarterKitMVC.Application/Services/SeoMetaService.cs`

```csharp
using SmartWorkz.StarterKitMVC.Domain.Entities.Shared;

namespace SmartWorkz.StarterKitMVC.Application.Services
{
    public interface ISeoMetaService
    {
        Task<SeoMeta> GetBySlugAsync(string slug, Guid tenantId);
        Task<SeoMeta> GetByEntityAsync(string entityType, Guid entityId, Guid tenantId);
        Task<SeoMeta> CreateAsync(SeoMeta seoMeta);
        Task<SeoMeta> UpdateAsync(SeoMeta seoMeta);
        Task DeleteAsync(Guid seoMetaId);
        Task<bool> SlugExistsAsync(string slug, Guid tenantId, Guid? exceptSeoMetaId = null);
    }

    public class SeoMetaService : ISeoMetaService
    {
        private readonly IRepository<SeoMeta> _repository;
        private readonly ILogger<SeoMetaService> _logger;

        public SeoMetaService(IRepository<SeoMeta> repository, ILogger<SeoMetaService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        /// <summary>
        /// Get SEO metadata by URL slug (for route resolution)
        /// </summary>
        public async Task<SeoMeta> GetBySlugAsync(string slug, Guid tenantId)
        {
            return await _repository.FirstOrDefaultAsync(s =>
                s.Slug == slug && s.TenantId == tenantId && s.IsActive && !s.IsDeleted);
        }

        /// <summary>
        /// Get SEO metadata by entity type and ID (for entity detail pages)
        /// </summary>
        public async Task<SeoMeta> GetByEntityAsync(string entityType, Guid entityId, Guid tenantId)
        {
            return await _repository.FirstOrDefaultAsync(s =>
                s.EntityType == entityType && s.EntityId == entityId && s.TenantId == tenantId && !s.IsDeleted);
        }

        /// <summary>
        /// Create new SEO metadata
        /// </summary>
        public async Task<SeoMeta> CreateAsync(SeoMeta seoMeta)
        {
            if (await SlugExistsAsync(seoMeta.Slug, seoMeta.TenantId))
                throw new InvalidOperationException($"Slug '{seoMeta.Slug}' already exists for this tenant.");

            seoMeta.SeoMetaId = Guid.NewGuid();
            await _repository.AddAsync(seoMeta);
            await _repository.SaveChangesAsync();
            return seoMeta;
        }

        /// <summary>
        /// Update SEO metadata
        /// </summary>
        public async Task<SeoMeta> UpdateAsync(SeoMeta seoMeta)
        {
            if (await SlugExistsAsync(seoMeta.Slug, seoMeta.TenantId, seoMeta.SeoMetaId))
                throw new InvalidOperationException($"Slug '{seoMeta.Slug}' already exists for this tenant.");

            seoMeta.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(seoMeta);
            await _repository.SaveChangesAsync();
            return seoMeta;
        }

        /// <summary>
        /// Soft delete SEO metadata
        /// </summary>
        public async Task DeleteAsync(Guid seoMetaId)
        {
            var seo = await _repository.GetByIdAsync(seoMetaId);
            if (seo == null)
                throw new KeyNotFoundException($"SeoMeta with ID {seoMetaId} not found.");

            seo.IsDeleted = true;
            seo.UpdatedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(seo);
            await _repository.SaveChangesAsync();
        }

        /// <summary>
        /// Check if slug exists (for validation)
        /// </summary>
        public async Task<bool> SlugExistsAsync(string slug, Guid tenantId, Guid? exceptSeoMetaId = null)
        {
            var query = _repository.Query(s =>
                s.Slug == slug && s.TenantId == tenantId && !s.IsDeleted);

            if (exceptSeoMetaId.HasValue)
                query = query.Where(s => s.SeoMetaId != exceptSeoMetaId);

            return await query.AnyAsync();
        }
    }
}
```

---

### Task 4: Create Route Resolution Service

**File:** `src/SmartWorkz.StarterKitMVC.Application/Services/RouteResolutionService.cs`

```csharp
using SmartWorkz.StarterKitMVC.Domain.Entities.Shared;

namespace SmartWorkz.StarterKitMVC.Application.Services
{
    public class RouteResolutionResult
    {
        public string Controller { get; set; }
        public string Action { get; set; }
        public Guid EntityId { get; set; }
        public SeoMeta SeoMetadata { get; set; }
    }

    public interface IRouteResolutionService
    {
        Task<RouteResolutionResult> ResolveSlugAsync(string slug, Guid tenantId);
    }

    public class RouteResolutionService : IRouteResolutionService
    {
        private readonly ISeoMetaService _seoService;
        private readonly ILogger<RouteResolutionService> _logger;

        public RouteResolutionService(ISeoMetaService seoService, ILogger<RouteResolutionService> logger)
        {
            _seoService = seoService;
            _logger = logger;
        }

        /// <summary>
        /// Resolve slug to controller/action for ASP.NET routing
        /// </summary>
        public async Task<RouteResolutionResult> ResolveSlugAsync(string slug, Guid tenantId)
        {
            var seo = await _seoService.GetBySlugAsync(slug, tenantId);
            if (seo == null) return null;

            var result = seo.EntityType switch
            {
                "Product" => new RouteResolutionResult
                {
                    Controller = "Products",
                    Action = "Detail",
                    EntityId = seo.EntityId,
                    SeoMetadata = seo
                },
                "Category" => new RouteResolutionResult
                {
                    Controller = "Categories",
                    Action = "List",
                    EntityId = seo.EntityId,
                    SeoMetadata = seo
                },
                "BlogPost" => new RouteResolutionResult
                {
                    Controller = "Blog",
                    Action = "Post",
                    EntityId = seo.EntityId,
                    SeoMetadata = seo
                },
                "MenuItem" => new RouteResolutionResult
                {
                    Controller = "Navigation",
                    Action = "MenuItem",
                    EntityId = seo.EntityId,
                    SeoMetadata = seo
                },
                "GeolocationPage" => new RouteResolutionResult
                {
                    Controller = "LocationBased",
                    Action = "Products",
                    EntityId = seo.EntityId,
                    SeoMetadata = seo
                },
                _ => null
            };

            _logger.LogInformation($"Resolved slug '{slug}' to {result?.Controller}/{result?.Action}");
            return result;
        }
    }
}
```

---

### Task 5: Create DTOs

**File:** `src/SmartWorkz.StarterKitMVC.Shared/DTOs/SeoMetaDto.cs`

```csharp
namespace SmartWorkz.StarterKitMVC.Shared.DTOs
{
    public class SeoMetaDto
    {
        public Guid SeoMetaId { get; set; }
        public string EntityType { get; set; }
        public Guid EntityId { get; set; }
        public string Slug { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public string OgTitle { get; set; }
        public string OgDescription { get; set; }
        public string OgImage { get; set; }
        public string OgType { get; set; }
        public string CanonicalUrl { get; set; }
        public string SchemaMarkup { get; set; }
        public string Robots { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateSeoMetaDto
    {
        [Required, MaxLength(50)]
        public string EntityType { get; set; }

        [Required]
        public Guid EntityId { get; set; }

        [Required, MaxLength(255)]
        public string Slug { get; set; }

        [Required, MaxLength(255)]
        public string MetaTitle { get; set; }

        [Required, MaxLength(500)]
        public string MetaDescription { get; set; }

        public string MetaKeywords { get; set; }
        public string OgTitle { get; set; }
        public string OgDescription { get; set; }
        public string OgImage { get; set; }
        public string OgType { get; set; } = "website";
        public string CanonicalUrl { get; set; }
        public string SchemaMarkup { get; set; }
        public string Robots { get; set; } = "index,follow";
        public bool IsActive { get; set; } = true;
    }

    public class UpdateSeoMetaDto
    {
        [Required, MaxLength(255)]
        public string MetaTitle { get; set; }

        [Required, MaxLength(500)]
        public string MetaDescription { get; set; }

        public string MetaKeywords { get; set; }
        public string OgTitle { get; set; }
        public string OgDescription { get; set; }
        public string OgImage { get; set; }
        public string OgType { get; set; }
        public string CanonicalUrl { get; set; }
        public string SchemaMarkup { get; set; }
        public string Robots { get; set; }
        public bool IsActive { get; set; }
    }
}
```

---

### Task 6: Create REST API Endpoints

**File:** `src/SmartWorkz.StarterKitMVC.Web/Controllers/Api/SeoMetaController.cs`

```csharp
using SmartWorkz.StarterKitMVC.Application.Services;
using SmartWorkz.StarterKitMVC.Shared.DTOs;

namespace SmartWorkz.StarterKitMVC.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SeoMetaController : ControllerBase
    {
        private readonly ISeoMetaService _seoMetaService;
        private readonly IRouteResolutionService _routeResolution;
        private readonly IMapper _mapper;
        private readonly ILogger<SeoMetaController> _logger;

        public SeoMetaController(
            ISeoMetaService seoMetaService,
            IRouteResolutionService routeResolution,
            IMapper mapper,
            ILogger<SeoMetaController> logger)
        {
            _seoMetaService = seoMetaService;
            _routeResolution = routeResolution;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get SEO by slug (for route resolution)
        /// </summary>
        [HttpGet("by-slug/{slug}")]
        [AllowAnonymous]
        public async Task<ActionResult<SeoMetaDto>> GetBySlug(string slug)
        {
            var seo = await _seoMetaService.GetBySlugAsync(slug, CurrentUser.TenantId);
            if (seo == null) return NotFound();
            return Ok(_mapper.Map<SeoMetaDto>(seo));
        }

        /// <summary>
        /// Get SEO by entity type and ID
        /// </summary>
        [HttpGet("{entityType}/{entityId}")]
        public async Task<ActionResult<SeoMetaDto>> GetByEntity(string entityType, Guid entityId)
        {
            var seo = await _seoMetaService.GetByEntityAsync(entityType, entityId, CurrentUser.TenantId);
            if (seo == null) return NotFound();
            return Ok(_mapper.Map<SeoMetaDto>(seo));
        }

        /// <summary>
        /// Create SEO metadata
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<SeoMetaDto>> Create(CreateSeoMetaDto dto)
        {
            var seo = _mapper.Map<SeoMeta>(dto);
            seo.TenantId = CurrentUser.TenantId;
            seo.CreatedBy = CurrentUser.UserId;

            var created = await _seoMetaService.CreateAsync(seo);
            return CreatedAtAction(nameof(GetByEntity), new { entityType = created.EntityType, entityId = created.EntityId }, _mapper.Map<SeoMetaDto>(created));
        }

        /// <summary>
        /// Update SEO metadata
        /// </summary>
        [HttpPut("{seoMetaId}")]
        public async Task<ActionResult<SeoMetaDto>> Update(Guid seoMetaId, UpdateSeoMetaDto dto)
        {
            var existing = await _seoMetaService.GetByEntityAsync("", seoMetaId, CurrentUser.TenantId);
            if (existing == null) return NotFound();

            _mapper.Map(dto, existing);
            existing.UpdatedBy = CurrentUser.UserId;

            var updated = await _seoMetaService.UpdateAsync(existing);
            return Ok(_mapper.Map<SeoMetaDto>(updated));
        }

        /// <summary>
        /// Delete SEO metadata
        /// </summary>
        [HttpDelete("{seoMetaId}")]
        public async Task<IActionResult> Delete(Guid seoMetaId)
        {
            await _seoMetaService.DeleteAsync(seoMetaId);
            return NoContent();
        }

        /// <summary>
        /// Resolve slug to controller/action
        /// </summary>
        [HttpGet("resolve-slug/{slug}")]
        [AllowAnonymous]
        public async Task<ActionResult> ResolveSlug(string slug)
        {
            var result = await _routeResolution.ResolveSlugAsync(slug, CurrentUser.TenantId);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
```

---

### Task 7: Update Dependency Injection

**File:** `src/SmartWorkz.StarterKitMVC.Web/Program.cs`

```csharp
// Add to services
builder.Services.AddScoped<ISeoMetaService, SeoMetaService>();
builder.Services.AddScoped<IRouteResolutionService, RouteResolutionService>();
builder.Services.AddScoped<IRepository<SeoMeta>, Repository<SeoMeta>>();

// Add AutoMapper profile for SeoMeta
builder.Services.AddAutoMapper(typeof(SeoMetaMappingProfile));
```

---

### Task 8: Create AutoMapper Profile

**File:** `src/SmartWorkz.StarterKitMVC.Application/Mapping/SeoMetaMappingProfile.cs`

```csharp
using AutoMapper;
using SmartWorkz.StarterKitMVC.Domain.Entities.Shared;
using SmartWorkz.StarterKitMVC.Shared.DTOs;

namespace SmartWorkz.StarterKitMVC.Application.Mapping
{
    public class SeoMetaMappingProfile : Profile
    {
        public SeoMetaMappingProfile()
        {
            CreateMap<SeoMeta, SeoMetaDto>().ReverseMap();
            CreateMap<CreateSeoMetaDto, SeoMeta>();
            CreateMap<UpdateSeoMetaDto, SeoMeta>();
        }
    }
}
```

---

## Summary of Changes

| File | Change | Type |
|------|--------|------|
| `database/v4/002_CreateTables_Master.sql` | Remove SeoMeta table | SQL Migration |
| `database/v4/003_CreateTables_Shared.sql` | Add SeoMeta table | SQL Migration |
| `docs/srs/SCHEMA-REVIEW-v2.md` | Move SeoMeta to Shared, update counts | Documentation |
| `Domain/Entities/Shared/SeoMeta.cs` | Create entity class | C# Entity |
| `Infrastructure/SharedDbContext.cs` | Configure SeoMeta mapping | EF Core Config |
| `Application/Services/SeoMetaService.cs` | Create service layer | Business Logic |
| `Application/Services/RouteResolutionService.cs` | Create route resolution | Business Logic |
| `Shared/DTOs/SeoMetaDto.cs` | Create DTOs | Data Transfer |
| `Web/Controllers/Api/SeoMetaController.cs` | Create REST API | API Endpoints |
| `Web/Program.cs` | Register services | DI Configuration |
| `Application/Mapping/SeoMetaMappingProfile.cs` | Create AutoMapper profile | Mapping |

---

## Query Examples

### Get Product with SEO
```sql
SELECT p.*, s.*
FROM Core.Products p
LEFT JOIN Shared.SeoMeta s ON p.ProductId = s.EntityId
  AND s.EntityType = 'Product' AND s.TenantId = @TenantId
WHERE p.ProductId = @ProductId;
```

### Get Category Breadcrumbs with SEO
```sql
WITH CategoryHierarchy AS (
    SELECT c.*, 1 AS Level
    FROM Master.Categories c
    WHERE c.CategoryId = @CategoryId

    UNION ALL

    SELECT c.*, ch.Level + 1
    FROM Master.Categories c
    INNER JOIN CategoryHierarchy ch
        ON c.NodePath.IsDescendantOf(ch.NodePath) = 1
)
SELECT ch.*, s.MetaTitle, s.MetaDescription, s.Slug
FROM CategoryHierarchy ch
LEFT JOIN Shared.SeoMeta s ON ch.CategoryId = s.EntityId
  AND s.EntityType = 'Category' AND s.TenantId = @TenantId;
```

### Get Location-Based Products with SEO
```sql
SELECT
    c.Name AS CategoryName, c.Slug AS CategorySlug,
    g.Name AS LocationName, g.Slug AS GeoSlug,
    s.MetaTitle, s.MetaDescription,
    p.ProductId, p.Name AS ProductName
FROM Master.Categories c
CROSS JOIN Master.GeoHierarchy g
LEFT JOIN Shared.SeoMeta s ON s.EntityType = 'GeolocationPage'
    AND s.EntityId = CAST(CONCAT('category_', c.CategoryId, '_geo_', g.GeoHierarchyId) AS UNIQUEIDENTIFIER)
    AND s.TenantId = @TenantId
LEFT JOIN Core.Products p ON p.CategoryId = c.CategoryId AND p.AvailableInGeo = g.GeoHierarchyId
WHERE c.Slug = @CategorySlug AND g.Slug = @GeoSlug;
```

---

## Benefits

✅ **Consistency** — Single polymorphic pattern across all infrastructure tables (Addresses, Comments, Attachments, StateHistory, PreferenceDefinitions, SeoMeta)

✅ **Flexibility** — Products, Categories, MenuItems, BlogPosts, GeolocationPages all use same SEO table

✅ **Extensibility** — New entity types get SEO support automatically (no schema changes)

✅ **Multi-tenant** — Row-level TenantId isolation

✅ **URL Routing** — Slug-based routing with built-in route resolution

✅ **Structured Data** — schema.org markup support for Products, BreadcrumbLists, LocalBusiness, BlogPostings

---

## References

- **Schema Design:** `docs/srs/SCHEMA-REVIEW-v2.md` (Section 2.1 - Shared Schema)
- **SEO Patterns:** `docs/srs/SEO-POLYMORPHIC-DESIGN.md` (Complete examples for all use cases)
- **Menu System:** `docs/srs/MENU-SYSTEM-GUIDE.md` (Integration with menu items)

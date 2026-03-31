# SEO as Shared Polymorphic Infrastructure

**Date:** 2026-03-31
**Purpose:** Implement SEO metadata as reusable polymorphic table (like Addresses, Comments, Attachments)
**Scope:** Products, Categories, Subcategories, Menu pages, Geo-based listings, Blog posts, Custom pages

---

## Design Philosophy

SEO metadata should follow the **polymorphic linking pattern** used by:
- **Addresses** — linked to Orders, Customers, Employees, etc. via `(EntityType, EntityId)`
- **Attachments** — linked to any entity for file references
- **Comments** — linked to any entity for discussion threads
- **StateHistory** — linked to any entity for workflow tracking

**Result:** Single SeoMeta table in Shared schema serves all business domains—Products, Categories, MenuItems, BlogPosts, CustomPages, even Geo locations.

---

## Current Schema (SeoMeta in Master)

```sql
-- CURRENT: SeoMeta in Master schema (NOT ideal for polymorphism)
Master.SeoMeta
├─ SeoMetaId (GUID)
├─ EntityType (VARCHAR 50) -- 'MenuItem', 'Product', 'BlogPost'
├─ EntityId (UNIQUEIDENTIFIER) -- link to entity
├─ MetaTitle (NVARCHAR 255)
├─ MetaDescription (NVARCHAR 500)
├─ MetaKeywords (NVARCHAR MAX)
├─ OgTitle (NVARCHAR 255, nullable)
├─ OgDescription (NVARCHAR 500, nullable)
├─ OgImage (VARCHAR MAX, nullable) -- URL or FK to Attachment
├─ CanonicalUrl (VARCHAR MAX, nullable)
├─ SchemaMarkup (NVARCHAR MAX, nullable) -- JSON: structured data
├─ Slug (VARCHAR 255, unique per tenant)
├─ TenantId (GUID)
├─ IsActive (BIT)
└─ Audit: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsDeleted
```

**Problem:** While functional, SeoMeta is in Master (global/reference), not Shared. This works but is NOT the idiomatic polymorphic pattern. Moving it to Shared makes it consistent with Addresses, Comments, Attachments.

---

## RECOMMENDED: Move SeoMeta to Shared Schema

```sql
-- RECOMMENDED: Move to Shared schema (consistent with polymorphic pattern)
Shared.SeoMeta
├─ SeoMetaId (GUID, PK)
├─ TenantId (GUID, not null) -- All Shared tables require TenantId
├─ EntityType (VARCHAR 50, not null) -- 'Product', 'Category', 'MenuItem', 'BlogPost', 'GeolocationPage', 'CustomPage'
├─ EntityId (UNIQUEIDENTIFIER, not null) -- FK to entity (loose coupling)
├─ Slug (VARCHAR 255, not null) -- URL-friendly identifier
├─ MetaTitle (NVARCHAR 255, not null) -- <title> tag
├─ MetaDescription (NVARCHAR 500, not null) -- <meta name="description">
├─ MetaKeywords (NVARCHAR MAX, nullable) -- <meta name="keywords"> (optional, less critical)
├─ OgTitle (NVARCHAR 255, nullable) -- Open Graph title
├─ OgDescription (NVARCHAR 500, nullable) -- Open Graph description
├─ OgImage (VARCHAR MAX, nullable) -- Open Graph image URL or FK to Attachment.AttachmentId
├─ OgType (VARCHAR 50, nullable) -- 'product', 'article', 'website', default 'website'
├─ CanonicalUrl (VARCHAR MAX, nullable) -- For preventing duplicate content
├─ SchemaMarkup (NVARCHAR MAX, nullable) -- JSON: schema.org markup (Product, BreadcrumbList, LocalBusiness, etc.)
├─ Robots (VARCHAR 100, nullable) -- 'index,follow', 'noindex,nofollow', etc.
├─ IsActive (BIT, not null, default 1)
├─ Audit: CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
└─ Soft Delete: IsDeleted

-- Unique Constraints
UNIQUE (TenantId, EntityType, EntityId) -- One SEO per entity
UNIQUE (TenantId, Slug) -- Slug uniqueness per tenant (routing)

-- Indexes
INDEX (TenantId, EntityType) -- Query all SEO for Products, Categories, etc.
INDEX (TenantId, Slug) -- Route lookup: /products/laptop-dell-xps-13
INDEX (EntityType, EntityId) -- Reverse lookup: get SEO for specific entity
INDEX (IsActive) -- Active SEO only in queries
```

**Why Shared Schema?**
1. ✅ **Consistency** — All entities (Products, Categories, MenuItems, etc.) link SEO via `(EntityType, EntityId)`
2. ✅ **Polymorphic** — Single table serves Products, Categories, Subcategories, MenuItems, Blog posts, Geo pages
3. ✅ **TenantId isolation** — Row-level security (each tenant's SEO is separate)
4. ✅ **Reusable** — Any new entity type automatically gets SEO support (no schema changes)
5. ✅ **Clean separation** — Reference data in Master, infrastructure in Shared

---

## Use Cases: How SEO Works Across Domains

### 1. Products with SEO

```sql
-- Products table (hypothetical, added in Phase 1+)
Core.Products
├─ ProductId (GUID)
├─ TenantId (GUID)
├─ Sku (VARCHAR 100)
├─ Name (NVARCHAR 200)
├─ Description (NVARCHAR MAX)
└─ ... other product columns

-- SEO for product automatically via Shared.SeoMeta
Shared.SeoMeta
├─ EntityType = 'Product'
├─ EntityId = Products.ProductId
├─ Slug = 'laptop-dell-xps-13'
├─ MetaTitle = 'Dell XPS 13 Laptop - Premium 13" Ultrabook | Your Store'
├─ MetaDescription = 'High-performance Dell XPS 13 with Intel Core i7, 16GB RAM. Fast shipping, 30-day returns.'
├─ MetaKeywords = 'laptop, dell xps, ultrabook, 13 inch'
├─ SchemaMarkup = { "type": "Product", "name": "Dell XPS 13", "price": 1099.99, "image": "..." }

-- Query: Get product with its SEO
SELECT p.*, s.MetaTitle, s.MetaDescription, s.Slug
FROM Core.Products p
LEFT JOIN Shared.SeoMeta s ON p.ProductId = s.EntityId
  AND s.EntityType = 'Product' AND s.TenantId = @TenantId
WHERE p.ProductId = @ProductId;
```

---

### 2. Categories & Subcategories (HierarchyId Tree)

```sql
-- Categories table (HierarchyId for hierarchy)
Master.Categories
├─ CategoryId (GUID)
├─ TenantId (GUID)
├─ NodePath (HierarchyId) -- Tree: /1/ (Electronics) → /1/1/ (Computers) → /1/1/1/ (Laptops)
├─ Slug (VARCHAR 255) -- 'electronics', 'computers', 'laptops'
├─ Name (NVARCHAR 200)
└─ DisplayOrder (INT)

-- Example hierarchy
/1/                         Electronics
├─ /1/1/                    Computers
│  ├─ /1/1/1/               Laptops
│  ├─ /1/1/2/               Desktops
│  └─ /1/1/3/               Tablets
├─ /1/2/                    Audio
│  ├─ /1/2/1/               Headphones
│  └─ /1/2/2/               Speakers
└─ /1/3/                    Mobile Devices

-- SEO for each category level
Shared.SeoMeta (for Electronics)
├─ EntityType = 'Category'
├─ EntityId = CategoryId(/1/)
├─ Slug = 'electronics'
├─ MetaTitle = 'Electronics & Gadgets | Your Store'
├─ MetaDescription = 'Browse our wide selection of electronics including laptops, smartphones, and accessories.'
├─ SchemaMarkup = { "type": "CollectionPage", "name": "Electronics", "itemListElement": [...] }

Shared.SeoMeta (for Laptops subcategory)
├─ EntityType = 'Category'
├─ EntityId = CategoryId(/1/1/1/)
├─ Slug = 'electronics/computers/laptops'
├─ MetaTitle = 'Laptops & Notebook Computers | Best Brands | Your Store'
├─ MetaDescription = 'Shop premium laptops from Dell, Apple, HP. Free shipping on orders over $50.'
├─ SchemaMarkup = { "type": "CollectionPage", "name": "Laptops", "breadcrumbs": [...] }

-- Query: Get category hierarchy with breadcrumbs + SEO
WITH CategoryHierarchy AS (
    SELECT
        c.CategoryId, c.NodePath, c.Name, c.Slug,
        CAST(c.Name AS VARCHAR(MAX)) AS BreadcrumbPath,
        1 AS Level
    FROM Master.Categories c
    WHERE c.CategoryId = @CategoryId AND c.TenantId = @TenantId

    UNION ALL

    SELECT
        c.CategoryId, c.NodePath, c.Name, c.Slug,
        ch.BreadcrumbPath + ' > ' + c.Name,
        ch.Level + 1
    FROM Master.Categories c
    INNER JOIN CategoryHierarchy ch
        ON c.NodePath.IsDescendantOf(ch.NodePath) = 1
        AND c.TenantId = @TenantId
)
SELECT
    ch.*,
    s.MetaTitle, s.MetaDescription, s.Slug AS SeoSlug
FROM CategoryHierarchy ch
LEFT JOIN Shared.SeoMeta s ON ch.CategoryId = s.EntityId
  AND s.EntityType = 'Category' AND s.TenantId = @TenantId
ORDER BY ch.Level;
```

---

### 3. Geo-Based Product Listings

```sql
-- Geo-based search: /products/electronics/in/new-york
-- Example: "Laptops in New York" with location-specific SEO

-- Query structure:
-- Route: /products/{categorySlug}/in/{geoSlug}
-- Example: /products/electronics/computers/laptops/in/manhattan

Master.GeoHierarchy (NYC hierarchy)
├─ GeoHierarchyId (for USA)
├─ NodePath = /1/2/1/2/ (USA > NY > New York City > Manhattan)
├─ GeoType = 'District'
├─ Name = 'Manhattan'
├─ Slug = 'manhattan' (or 'new-york-manhattan')

-- SEO for geo-location page (composite slug)
Shared.SeoMeta (Laptops in Manhattan)
├─ EntityType = 'GeolocationPage' -- Special type for location-based pages
├─ EntityId = CONCAT('category_', CategoryId, '_geo_', GeoHierarchyId) -- Composite ID
├─ Slug = 'electronics/computers/laptops/in/manhattan'
├─ MetaTitle = 'Laptops in Manhattan, NY | Best Deals | Your Store'
├─ MetaDescription = 'Buy laptops in Manhattan. Fast delivery, local support. Shop Dell, Apple, HP.'
├─ SchemaMarkup = {
      "type": "LocalBusiness",
      "name": "Electronics Store - Manhattan",
      "areaServed": "Manhattan, NY",
      "itemListElement": [
        { "type": "Product", "name": "Dell XPS 13", ... },
        ...
      ]
    }

-- Query: Get category + geo location with SEO
SELECT
    c.CategoryId, c.Name AS CategoryName, c.Slug AS CategorySlug,
    g.GeoHierarchyId, g.Name AS LocationName, g.Slug AS GeoSlug,
    s.MetaTitle, s.MetaDescription, s.SchemaMarkup,
    -- Products available in this location
    p.ProductId, p.Name AS ProductName
FROM Master.Categories c
CROSS JOIN Master.GeoHierarchy g
LEFT JOIN Shared.SeoMeta s ON s.EntityType = 'GeolocationPage'
    AND s.EntityId = CONCAT('category_', c.CategoryId, '_geo_', g.GeoHierarchyId)
    AND s.TenantId = @TenantId
LEFT JOIN Core.Products p ON p.CategoryId = c.CategoryId
    AND p.AvailableInGeo = g.GeoHierarchyId
WHERE c.Slug = @CategorySlug
  AND g.Slug = @GeoSlug
  AND c.TenantId = @TenantId
  AND g.IsActive = 1;
```

---

### 4. MenuItems with SEO

```sql
-- Menu items (hierarchical navigation)
Master.MenuItems
├─ MenuItemId (GUID)
├─ FK → Menus (MenuId)
├─ NodePath (HierarchyId)
├─ Code = 'products'
├─ Name = 'Products'
├─ Url = '/products'
├─ Icon = 'fa-shopping-cart'
└─ TenantId (GUID)

-- SEO for menu page
Shared.SeoMeta
├─ EntityType = 'MenuItem'
├─ EntityId = MenuItemId
├─ Slug = 'products'
├─ MetaTitle = 'Products | Our Complete Catalog | Your Store'
├─ MetaDescription = 'Browse our full product catalog with thousands of items in stock.'
├─ CanonicalUrl = 'https://yourstore.com/products'

-- Query: Get menu with SEO
SELECT
    m.MenuItemId, m.Name, m.Url, m.Icon,
    s.MetaTitle, s.MetaDescription, s.CanonicalUrl
FROM Master.MenuItems m
LEFT JOIN Shared.SeoMeta s ON m.MenuItemId = s.EntityId
    AND s.EntityType = 'MenuItem' AND s.TenantId = @TenantId
WHERE m.MenuId = @MenuId
ORDER BY m.NodePath;
```

---

### 5. Blog Posts with SEO

```sql
-- Blog posts (added in Phase 1+)
Core.BlogPosts
├─ BlogPostId (GUID)
├─ TenantId (GUID)
├─ Title (NVARCHAR 500)
├─ Content (NVARCHAR MAX)
├─ PublishedDate (DATETIME)
└─ AuthorId (FK to Users)

-- SEO for blog post
Shared.SeoMeta
├─ EntityType = 'BlogPost'
├─ EntityId = BlogPostId
├─ Slug = 'how-to-choose-the-perfect-laptop'
├─ MetaTitle = 'How to Choose the Perfect Laptop in 2026 | Your Store Blog'
├─ MetaDescription = 'Expert guide on choosing laptops. Compare specs, performance, price. Updated 2026.'
├─ OgImage = 'https://storage.com/blog-cover.jpg'
├─ SchemaMarkup = {
      "type": "BlogPosting",
      "headline": "How to Choose the Perfect Laptop",
      "datePublished": "2026-03-31",
      "author": { "type": "Person", "name": "John Doe" }
    }

-- Query: Get blog post with SEO
SELECT
    bp.BlogPostId, bp.Title, bp.Content, bp.PublishedDate,
    s.MetaTitle, s.MetaDescription, s.OgImage, s.SchemaMarkup
FROM Core.BlogPosts bp
LEFT JOIN Shared.SeoMeta s ON bp.BlogPostId = s.EntityId
    AND s.EntityType = 'BlogPost' AND s.TenantId = @TenantId
WHERE bp.Slug = @Slug AND bp.TenantId = @TenantId;
```

---

## URL Routing Strategy

Single `SeoMeta` table enables sophisticated routing:

| URL | EntityType | EntityId | Slug | Example Slug |
|-----|-----------|----------|------|--------------|
| `/products` | MenuItem | MenuItem(Products) | products | products |
| `/products/electronics` | Category | CategoryId(/1/) | electronics | electronics |
| `/products/electronics/laptops` | Category | CategoryId(/1/1/1/) | electronics/laptops | electronics/laptops |
| `/products/electronics/laptops/in/manhattan` | GeolocationPage | Composite ID | geo_location | electronics/laptops/in/manhattan |
| `/products/dell-xps-13` | Product | ProductId | dell-xps-13 | dell-xps-13 |
| `/blog/how-to-choose-laptop` | BlogPost | BlogPostId | how-to-choose-laptop | how-to-choose-laptop |

**Route Handler (ASP.NET MVC example):**

```csharp
[Route("{slug}")]
public async Task<IActionResult> GetPageBySlug(string slug, [FromServices] IRepository<SeoMeta> seoRepo)
{
    var seo = await seoRepo.FirstOrDefaultAsync(s =>
        s.Slug == slug && s.TenantId == CurrentUser.TenantId && s.IsActive);

    if (seo == null) return NotFound();

    // Route based on EntityType
    return seo.EntityType switch
    {
        "Product" => await GetProductDetail(seo.EntityId),
        "Category" => await GetCategoryListing(seo.EntityId),
        "BlogPost" => await GetBlogPost(seo.EntityId),
        "MenuItem" => RedirectToMenuItemUrl(seo.EntityId),
        "GeolocationPage" => await GetLocationBasedListing(seo.EntityId),
        _ => NotFound()
    };
}
```

---

## Schema Comparison: Current vs. Recommended

### Current (SeoMeta in Master)
```
Master.SeoMeta
├─ SeoMetaId
├─ EntityType (VARCHAR 50)
├─ EntityId (UNIQUEIDENTIFIER)
├─ MetaTitle
├─ MetaDescription
├─ MetaKeywords
├─ OgTitle, OgDescription, OgImage
├─ CanonicalUrl
├─ SchemaMarkup
├─ Slug
├─ TenantId (nullable)
├─ IsActive
└─ Audit columns
```

**Pros:** Separate from MenuItems, reusable
**Cons:** In Master (reference data), not idiomatic polymorphic pattern

### Recommended (SeoMeta in Shared)
```
Shared.SeoMeta
├─ SeoMetaId
├─ TenantId (NOT NULL) ← moved to front
├─ EntityType (VARCHAR 50)
├─ EntityId (UNIQUEIDENTIFIER)
├─ Slug (VARCHAR 255)
├─ MetaTitle (NVARCHAR 255)
├─ MetaDescription (NVARCHAR 500)
├─ MetaKeywords (NVARCHAR MAX)
├─ OgTitle, OgDescription, OgImage, OgType
├─ CanonicalUrl
├─ SchemaMarkup (NVARCHAR MAX, JSON)
├─ Robots (VARCHAR 100)
├─ IsActive (BIT)
├─ Audit columns
└─ Unique: (TenantId, EntityType, EntityId), (TenantId, Slug)
```

**Pros:**
- ✅ Consistent with polymorphic pattern (like Addresses, Comments, Attachments)
- ✅ TenantId NOT NULL (required for Shared schema)
- ✅ Automatic SEO support for any new entity type
- ✅ Cleaner separation: infrastructure (Shared) vs. reference data (Master)

**Cons:** Migration from Master.SeoMeta to Shared.SeoMeta (one-time, simple)

---

## Implementation: Move SeoMeta to Shared

**SQL Migration:**

```sql
-- Step 1: Create new SeoMeta in Shared schema
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

-- Step 2: Migrate data from Master.SeoMeta
INSERT INTO Shared.SeoMeta (
    SeoMetaId, TenantId, EntityType, EntityId, Slug,
    MetaTitle, MetaDescription, MetaKeywords,
    OgTitle, OgDescription, OgImage,
    CanonicalUrl, SchemaMarkup, IsActive,
    CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsDeleted
)
SELECT
    SeoMetaId, TenantId, EntityType, EntityId, Slug,
    MetaTitle, MetaDescription, MetaKeywords,
    OgTitle, OgDescription, OgImage,
    CanonicalUrl, SchemaMarkup, IsActive,
    CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsDeleted
FROM Master.SeoMeta;

-- Step 3: Drop old table (after verification)
DROP TABLE Master.SeoMeta;
```

**C# Entity Mapping:**

```csharp
public class SeoMeta
{
    public Guid SeoMetaId { get; set; }
    public Guid TenantId { get; set; }

    [MaxLength(50)]
    public string EntityType { get; set; } // 'Product', 'Category', 'MenuItem', 'BlogPost', 'GeolocationPage'

    public Guid EntityId { get; set; } // Polymorphic link

    [MaxLength(255)]
    public string Slug { get; set; } // URL-friendly identifier

    [MaxLength(255)]
    public string MetaTitle { get; set; }

    [MaxLength(500)]
    public string MetaDescription { get; set; }

    public string MetaKeywords { get; set; } // NVARCHAR(MAX), optional

    [MaxLength(255)]
    public string OgTitle { get; set; }

    [MaxLength(500)]
    public string OgDescription { get; set; }

    public string OgImage { get; set; } // URL or FK to Attachment

    [MaxLength(50)]
    public string OgType { get; set; } = "website";

    public string CanonicalUrl { get; set; }

    public string SchemaMarkup { get; set; } // JSON: schema.org structured data

    [MaxLength(100)]
    public string Robots { get; set; } = "index,follow";

    public bool IsActive { get; set; } = true;

    // Audit columns
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
```

---

## Query Patterns: SEO Across Domains

### Pattern 1: Get Entity with SEO
```csharp
// Get Product with SEO metadata
public async Task<ProductDetailDto> GetProductWithSeoAsync(Guid productId, Guid tenantId)
{
    var product = await _context.Products
        .AsNoTracking()
        .Include(p => p.Seo) // Navigation property
        .FirstOrDefaultAsync(p => p.ProductId == productId && p.TenantId == tenantId);

    return new ProductDetailDto
    {
        ProductId = product.ProductId,
        Name = product.Name,
        Description = product.Description,
        // SEO metadata
        MetaTitle = product.Seo?.MetaTitle,
        MetaDescription = product.Seo?.MetaDescription,
        OgImage = product.Seo?.OgImage,
        SchemaMarkup = product.Seo?.SchemaMarkup
    };
}
```

### Pattern 2: Get by Slug (Route Lookup)
```csharp
// Find entity by SEO slug
public async Task<RouteResponse> ResolveSlugAsync(string slug, Guid tenantId)
{
    var seo = await _context.SeoMeta
        .AsNoTracking()
        .FirstOrDefaultAsync(s => s.Slug == slug && s.TenantId == tenantId && s.IsActive);

    if (seo == null) return null;

    return seo.EntityType switch
    {
        "Product" => new RouteResponse
        {
            Controller = "Products",
            Action = "Detail",
            Id = seo.EntityId
        },
        "Category" => new RouteResponse
        {
            Controller = "Categories",
            Action = "List",
            Id = seo.EntityId
        },
        "BlogPost" => new RouteResponse
        {
            Controller = "Blog",
            Action = "Post",
            Id = seo.EntityId
        },
        _ => null
    };
}
```

### Pattern 3: Breadcrumb Building
```csharp
// Build breadcrumbs with category hierarchy + SEO
public async Task<List<BreadcrumbDto>> GetCategoryBreadcrumbsAsync(Guid categoryId, Guid tenantId)
{
    var category = await _context.Categories
        .AsNoTracking()
        .FirstOrDefaultAsync(c => c.CategoryId == categoryId && c.TenantId == tenantId);

    if (category == null) return new();

    var ancestors = await _context.Categories
        .AsNoTracking()
        .Where(c => c.NodePath.GetAncestors().Contains(category.NodePath) && c.TenantId == tenantId)
        .OrderBy(c => c.NodePath)
        .ToListAsync();

    var seoMap = await _context.SeoMeta
        .AsNoTracking()
        .Where(s => s.EntityType == "Category" && s.TenantId == tenantId)
        .ToDictionaryAsync(s => s.EntityId);

    return ancestors.Concat(new[] { category })
        .Select(c => new BreadcrumbDto
        {
            Name = c.Name,
            Url = seoMap.TryGetValue(c.CategoryId, out var seo) ? seo.Slug : null,
            MetaTitle = seoMap.TryGetValue(c.CategoryId, out var s) ? s.MetaTitle : null
        })
        .ToList();
}
```

### Pattern 4: Geo-Location Page Listing
```csharp
// Get products available in location with location-specific SEO
public async Task<LocationListingDto> GetLocationBasedProductsAsync(
    string categorySlug, string geoSlug, Guid tenantId)
{
    // Find category
    var category = await _context.Categories
        .AsNoTracking()
        .FirstOrDefaultAsync(c => c.Slug == categorySlug && c.TenantId == tenantId);

    // Find geo location
    var geo = await _context.GeoHierarchy
        .AsNoTracking()
        .FirstOrDefaultAsync(g => g.Slug == geoSlug && g.IsActive);

    // Get location-specific SEO
    var locationSeoId = $"category_{category.CategoryId}_geo_{geo.GeoHierarchyId}";
    var seo = await _context.SeoMeta
        .AsNoTracking()
        .FirstOrDefaultAsync(s => s.EntityId.ToString() == locationSeoId &&
            s.EntityType == "GeolocationPage" && s.TenantId == tenantId);

    // Get products
    var products = await _context.Products
        .AsNoTracking()
        .Where(p => p.CategoryId == category.CategoryId && p.AvailableInGeo == geo.GeoHierarchyId)
        .ToListAsync();

    return new LocationListingDto
    {
        CategoryName = category.Name,
        GeoLocation = geo.Name,
        MetaTitle = seo?.MetaTitle,
        MetaDescription = seo?.MetaDescription,
        SchemaMarkup = seo?.SchemaMarkup,
        Products = products
    };
}
```

---

## Advantages of Shared SeoMeta

| Feature | Benefit |
|---------|---------|
| **Single Table** | All entity types (Product, Category, MenuItem, BlogPost, etc.) use same structure |
| **Polymorphic** | New entity types get SEO automatically without schema changes |
| **Slug-based Routing** | Clean URLs with built-in URL rewriting (no query params) |
| **Multi-tenant** | TenantId isolation ensures data privacy |
| **Composite Slugs** | Supports hierarchical URLs: `/electronics/computers/laptops/in/manhattan` |
| **Schema.org Integration** | JSON SchemaMarkup supports all entity types (Product, BreadcrumbList, LocalBusiness, BlogPosting) |
| **OG Tags** | Social media sharing with title, description, image |
| **Canonical URLs** | Prevents duplicate content penalties |
| **Audit Trail** | CreatedAt, UpdatedAt, CreatedBy, UpdatedBy for compliance |
| **Soft Delete** | IsDeleted for data retention |

---

## Implementation Phase

**Phase 1:**
- Move SeoMeta to Shared schema
- Update DbContext mappings
- Create repositories with slug-based lookups
- Implement route handler for slug resolution

**Phase 1+:**
- Extend for Products, Categories, BlogPosts
- Implement breadcrumb generation
- Add location-based SEO (GeolocationPage entity type)
- Build schema.org markup generators

---

## Files to Update

1. **docs/srs/SCHEMA-REVIEW-v2.md**
   - Move SeoMeta from Master (1.7) to Shared schema section
   - Update Master schema count (19 → 18 tables)
   - Update Shared schema count (5 → 6 tables) ← Wait, re-read Shared section

Actually, let me check the current Shared schema definition:

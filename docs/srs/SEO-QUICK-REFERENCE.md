# SEO Polymorphic Design - Quick Reference

**Schema Change:** Master → Shared (moved SeoMeta from Master.SeoMeta to Shared.SeoMeta)

---

## One Table, Many Entities

```
Shared.SeoMeta (EntityType + EntityId = Polymorphic Link)
├── EntityType: 'Product'
│   └── Products: Dell XPS 13, MacBook Pro, etc.
├── EntityType: 'Category'
│   └── Categories: Electronics, Computers, Laptops (with HierarchyId nesting)
├── EntityType: 'MenuItem'
│   └── Menu Items: /products, /blog, /about
├── EntityType: 'BlogPost'
│   └── Blog Posts: "How to Choose a Laptop"
├── EntityType: 'GeolocationPage'
│   └── Location pages: "Laptops in Manhattan", "Electronics in Brooklyn"
└── EntityType: 'CustomPage'
    └── Custom pages: Terms, Privacy, etc.
```

---

## URL Routing Examples

| URL | Entity Type | Slug | SEO Title | SEO Description |
|-----|-------------|------|-----------|-----------------|
| `/products` | MenuItem | products | Products \| Your Store | Browse our product catalog |
| `/products/electronics` | Category | electronics | Electronics \| Your Store | Shop electronics & gadgets |
| `/products/electronics/laptops` | Category | electronics/laptops | Laptops & Notebooks \| Best Brands \| Your Store | Shop premium laptops |
| `/products/dell-xps-13` | Product | dell-xps-13 | Dell XPS 13 \| Premium Ultrabook \| Your Store | High-performance Dell XPS 13... |
| `/products/electronics/in/manhattan` | GeolocationPage | electronics/in/manhattan | Electronics in Manhattan \| Your Store | Electronics & gadgets in Manhattan, NY |
| `/blog/how-to-choose-laptop` | BlogPost | how-to-choose-laptop | How to Choose the Perfect Laptop \| Your Store Blog | Expert guide on choosing laptops... |

---

## Query Patterns

### Get Entity with SEO
```csharp
var product = await _context.Products
    .Include(p => p.Seo) // Navigation: Product → SeoMeta
    .FirstOrDefaultAsync(p => p.ProductId == productId);

var title = product.Seo?.MetaTitle;
var description = product.Seo?.MetaDescription;
```

### Resolve Slug to Route
```csharp
var seo = await _context.SeoMeta
    .FirstOrDefaultAsync(s => s.Slug == slug && s.IsActive);

var route = seo.EntityType switch {
    "Product" => $"/products/{seo.EntityId}",
    "Category" => $"/categories/{seo.EntityId}",
    "BlogPost" => $"/blog/{seo.EntityId}",
    _ => null
};
```

### Category Breadcrumbs with SEO
```csharp
WITH CategoryHierarchy AS (
    SELECT c.*, ROW_NUMBER() OVER (ORDER BY c.NodePath) AS Level
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
  AND s.EntityType = 'Category';
```

### Location-Based Listings
```csharp
var categoryId = /* Electronics */;
var geoId = /* Manhattan */;

var products = await _context.Products
    .Where(p => p.CategoryId == categoryId && p.AvailableInGeo == geoId)
    .ToListAsync();

var locationSeo = await _context.SeoMeta
    .FirstOrDefaultAsync(s =>
        s.EntityType == "GeolocationPage" &&
        s.EntityId == CONCAT(categoryId, "_", geoId) &&
        s.IsActive);

// locationSeo.MetaTitle: "Laptops in Manhattan | Your Store"
// locationSeo.SchemaMarkup: LocalBusiness + Product array
```

---

## Open Graph (Social Media) Support

```
Product: "Dell XPS 13"
├─ OgTitle: "Dell XPS 13 - Premium Ultrabook"
├─ OgDescription: "High-performance 13-inch laptop with Intel Core i7"
├─ OgImage: "https://cdn.example.com/xps-13.jpg"
├─ OgType: "product"
└─ SchemaMarkup: { type: "Product", price: 1099.99, image: "...", rating: 4.8 }

Category: "Electronics"
├─ OgTitle: "Electronics & Gadgets"
├─ OgDescription: "Explore thousands of tech products"
├─ OgImage: "https://cdn.example.com/electronics-banner.jpg"
├─ OgType: "website"
└─ SchemaMarkup: { type: "CollectionPage", itemListElement: [...] }

BlogPost: "How to Choose a Laptop"
├─ OgTitle: "How to Choose the Perfect Laptop in 2026"
├─ OgDescription: "Expert guide comparing specs, performance, and price"
├─ OgImage: "https://cdn.example.com/blog-header.jpg"
├─ OgType: "article"
└─ SchemaMarkup: { type: "BlogPosting", datePublished: "2026-03-31", author: "John Doe" }
```

---

## Schema.org Structured Data

### Product
```json
{
  "@context": "https://schema.org",
  "@type": "Product",
  "name": "Dell XPS 13",
  "description": "Premium 13-inch ultrabook",
  "image": "https://cdn.example.com/xps-13.jpg",
  "price": "1099.99",
  "priceCurrency": "USD",
  "rating": {
    "@type": "AggregateRating",
    "ratingValue": "4.8",
    "reviewCount": "248"
  },
  "availability": "https://schema.org/InStock"
}
```

### BreadcrumbList (Category hierarchy)
```json
{
  "@context": "https://schema.org",
  "@type": "BreadcrumbList",
  "itemListElement": [
    {
      "@type": "ListItem",
      "position": 1,
      "name": "Home",
      "item": "https://example.com"
    },
    {
      "@type": "ListItem",
      "position": 2,
      "name": "Electronics",
      "item": "https://example.com/electronics"
    },
    {
      "@type": "ListItem",
      "position": 3,
      "name": "Laptops",
      "item": "https://example.com/electronics/laptops"
    }
  ]
}
```

### LocalBusiness (Location-based)
```json
{
  "@context": "https://schema.org",
  "@type": "LocalBusiness",
  "name": "Electronics Store - Manhattan",
  "description": "Shop electronics and gadgets in Manhattan, NY",
  "areaServed": {
    "@type": "City",
    "name": "Manhattan, New York, USA"
  },
  "hasOfferCatalog": {
    "@type": "OfferCatalog",
    "itemListElement": [
      {
        "@type": "Product",
        "name": "Dell XPS 13",
        "price": "1099.99"
      }
    ]
  }
}
```

### BlogPosting
```json
{
  "@context": "https://schema.org",
  "@type": "BlogPosting",
  "headline": "How to Choose the Perfect Laptop in 2026",
  "description": "Expert guide on comparing laptop specs and performance",
  "image": "https://cdn.example.com/blog-header.jpg",
  "datePublished": "2026-03-31",
  "dateModified": "2026-03-31",
  "author": {
    "@type": "Person",
    "name": "John Doe"
  },
  "articleBody": "..."
}
```

---

## Canonical URLs & Redirects

### Canonical URL (prevent duplicate content)
```
Entity: Product "Dell XPS 13"
├─ Primary URL: /products/dell-xps-13
├─ CanonicalUrl: https://example.com/products/dell-xps-13
└─ Prevents: /products?id=123, /item/xps-13, etc. from ranking separately
```

### URL Redirects (301 moved permanently)
```sql
UrlRedirects (moved from old URL structure)
├─ /old-product-page/123 → /products/dell-xps-13 (301)
├─ /electronics.html → /products/electronics (301)
├─ /blog?post=456 → /blog/how-to-choose-laptop (301)
```

---

## Benefits Summary

| Aspect | Benefit |
|--------|---------|
| **Consistency** | All entities (Products, Categories, MenuItems, Blog, Geo pages) use same SeoMeta table |
| **Flexibility** | New entity types automatically get SEO—no schema changes needed |
| **Multi-tenant** | TenantId isolation ensures each tenant's SEO is separate |
| **Performance** | Indexes on (TenantId, Slug) and (EntityType, EntityId) for fast queries |
| **Reusability** | Slug-based routing works for all entities |
| **Structured Data** | JSON SchemaMarkup supports all entity types (Product, BreadcrumbList, LocalBusiness, BlogPosting) |
| **Social Sharing** | OG tags for Twitter, Facebook, LinkedIn sharing |
| **Compliance** | Robots meta, canonical URLs, 301 redirects for SEO best practices |

---

## Implementation Checklist

- [ ] Update database schema (move SeoMeta from Master to Shared)
- [ ] Create SeoMeta entity class (C#)
- [ ] Configure EF Core mapping
- [ ] Create ISeoMetaService interface + implementation
- [ ] Create IRouteResolutionService for slug-based routing
- [ ] Create DTOs (Create, Update, Read)
- [ ] Create REST API endpoints (/api/seometa)
- [ ] Create AutoMapper profile
- [ ] Register DI services in Program.cs
- [ ] Add tests for slug lookup, entity linking, route resolution
- [ ] Document API in Swagger

---

## Phase 1+ Extensions

**Products:** Add Product entity, link SeoMeta by ProductId
**Blog:** Add BlogPost entity, link SeoMeta by BlogPostId
**Location Pages:** Composite key (CategoryId + GeoId) → SeoMeta
**Custom Pages:** Add CustomPage entity, link SeoMeta by PageId
**Reviews/Ratings:** Store in SchemaMarkup JSON for Products
**Rich Snippets:** FAQ, How-to, Event schemas via SchemaMarkup

---

## Files Changed

| File | Change |
|------|--------|
| `database/v4/002_CreateTables_Master.sql` | Remove SeoMeta definition |
| `database/v4/003_CreateTables_Shared.sql` | Add SeoMeta table |
| `docs/srs/SCHEMA-REVIEW-v2.md` | Update schema counts (Master: 20→18, Shared: 5→6) |
| `docs/srs/SEO-POLYMORPHIC-DESIGN.md` | NEW: Complete design document |
| `docs/srs/SEO-SHARED-IMPLEMENTATION-CHECKLIST.md` | NEW: Phase 1 tasks + code examples |
| `docs/srs/SEO-QUICK-REFERENCE.md` | NEW: This quick reference guide |

---

**See Also:** [SEO-POLYMORPHIC-DESIGN.md](SEO-POLYMORPHIC-DESIGN.md) for complete examples with SQL queries and C# code.

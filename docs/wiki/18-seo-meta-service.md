# SEO Meta Service

Polymorphic SEO metadata (title, description, keywords, slug, Open Graph fields) attached to any entity by `(EntityType, EntityId)`. One source of truth for server-rendered Razor views and JSON API consumers (Angular/React/MAUI/WPF).

## Purpose

- **One `SeoMeta` row per (tenant, entity type, entity id)** — products, categories, articles, pages, etc.
- **Slug resolution.** `GetBySlugAsync(tenantId, slug)` powers friendly-URL routing — look up a page by its slug, then load the content entity by its id.
- **Open Graph ready.** Populates `og:title`, `og:description`, `og:image` alongside the core SEO triplet.
- **Idempotent upsert.** `CreateOrUpdateSeoMetaAsync` does insert-or-update keyed on `(TenantId, EntityType, EntityId)`.
- **Soft delete.** Delete flips `IsDeleted`; row remains for audit.

## Architecture

| Component | Role | File |
|-----------|------|------|
| `ISeoMetaService` | Contract | [`Application/Services/ISeoMetaService.cs`](../../src/SmartWorkz.StarterKitMVC.Application/Services/ISeoMetaService.cs) |
| `SeoMetaService` | EF Core implementation against `SharedDbContext` | [`Infrastructure/Services/SeoMetaService.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/Services/SeoMetaService.cs) |
| `SeoMeta` | Shared.SeoMetas row | [`Domain/Entities/Shared/SeoMeta.cs`](../../src/SmartWorkz.StarterKitMVC.Domain/Entities/Shared/SeoMeta.cs) |

### Entity shape

| Field | Notes |
|-------|-------|
| `SeoMetaId` | `int` identity |
| `EntityType` | Free-form tag for the owning entity type (`Product`, `Article`, `Category`, …) |
| `EntityId` | `int` id of the owning entity |
| `Title`, `Description`, `Keywords` | The classic SEO triplet |
| `Slug` | Unique per tenant in practice — enforce via DB index |
| `OgTitle`, `OgDescription`, `OgImageUrl` | Open Graph fields for social sharing |
| `TenantId` | Multi-tenant scope |
| `IsActive` / `IsDeleted` | Soft toggle + soft delete |

## DI Registration

Wired by `AddApplicationServices`:

```csharp
services.AddScoped<ISeoMetaService, SeoMetaService>();
```

## Quick Start

### Save SEO metadata alongside an entity

```csharp
public class ProductsService
{
    private readonly IProductRepository _products;
    private readonly ISeoMetaService _seo;

    public async Task<Result> UpsertAsync(Product product)
    {
        await _products.UpsertAsync(product);

        await _seo.CreateOrUpdateSeoMetaAsync(new SeoMeta
        {
            TenantId     = product.TenantId,
            EntityType   = "Product",
            EntityId     = product.ProductId,
            Title        = product.Name,
            Description  = product.ShortDescription,
            Keywords     = string.Join(",", product.Tags ?? []),
            Slug         = product.Slug,
            OgTitle      = product.Name,
            OgDescription= product.ShortDescription,
            OgImageUrl   = product.PrimaryImageUrl
        });

        return Result.Ok();
    }
}
```

### Resolve a slug (pretty URLs)

```csharp
public class ProductDetailsModel : BasePage
{
    private readonly ISeoMetaService _seo;
    private readonly IProductRepository _products;

    public Product Product { get; private set; } = default!;
    public SeoMeta Meta    { get; private set; } = default!;

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        var meta = await _seo.GetBySlugAsync(TenantId, slug);
        if (meta is null || meta.EntityType != "Product")
            return NotFound();

        Meta    = meta;
        Product = (await _products.GetByIdAsync(meta.EntityId))!;
        return Page();
    }
}
```

### Render meta tags in the Razor layout

```razor
@if (Model.Meta is not null)
{
    <title>@Model.Meta.Title</title>
    <meta name="description"    content="@Model.Meta.Description" />
    <meta name="keywords"       content="@Model.Meta.Keywords" />
    <meta property="og:title"       content="@Model.Meta.OgTitle" />
    <meta property="og:description" content="@Model.Meta.OgDescription" />
    <meta property="og:image"       content="@Model.Meta.OgImageUrl" />
}
```

### Expose via REST for SPA / mobile

```csharp
[HttpGet("api/seo/{entityType}/{entityId:int}")]
public async Task<ActionResult<SeoMetaDto>> Get(string entityType, int entityId)
{
    var tenantId = HttpContext.Items["TenantId"] as string ?? "DEFAULT";
    var meta = await _seo.GetByEntityAsync(tenantId, entityType, entityId);
    return meta is null ? NotFound() : Ok(SeoMetaDto.From(meta));
}
```

Angular/React rendering a page fetches the SEO block and writes it into the document head (via Angular Meta service, React Helmet, etc.). Mobile apps use the same payload for share sheets (`og:*`) and analytics titles.

## Method Reference

```csharp
Task<SeoMeta> GetByEntityAsync(string tenantId, string entityType, int entityId);
Task<SeoMeta> GetBySlugAsync(string tenantId, string slug);
Task<SeoMeta> CreateOrUpdateSeoMetaAsync(SeoMeta seoMeta);
Task<bool>    DeleteSeoMetaAsync(int seoMetaId);
```

### `GetByEntityAsync(tenantId, entityType, entityId)` → `SeoMeta?`

Returns the non-deleted SEO row for the exact triple, or `null`. The return type in the interface is non-nullable but the implementation can return null — treat the result as `SeoMeta?`.

### `GetBySlugAsync(tenantId, slug)` → `SeoMeta?`

First non-deleted row matching the slug in the given tenant. If you allow the same slug across different entity types, this may resolve ambiguously — enforce a unique index on `(TenantId, Slug)` to keep it deterministic.

### `CreateOrUpdateSeoMetaAsync(seoMeta)` → `SeoMeta`

Upsert keyed on `(TenantId, EntityType, EntityId)`:

- If a matching row exists, it updates the content fields (`Title`, `Description`, `Keywords`, `Slug`, `Og*`) and stamps `UpdatedAt`.
- Otherwise, inserts a new row with `CreatedAt`.

Returns the persisted entity (either the updated existing row or the new one).

> **TenantId must be set on the DTO** before calling. The service does not infer it from the ambient request.

### `DeleteSeoMetaAsync(seoMetaId)` → `bool`

Soft delete. Returns `true` when a row was found, `false` otherwise. The underlying content entity is not touched.

## Cross-Client Notes

| Client | What to render |
|--------|----------------|
| **Razor Pages / MVC** | `<title>`, `<meta name="description">`, Open Graph in `_Layout.cshtml` |
| **Angular** | `Meta` + `Title` services (update on route change) |
| **React** | `react-helmet-async` with the payload from the API |
| **.NET MAUI** | Share-sheet title + image from `Og*` fields |
| **WPF / WinUI** | Share contract + toast titles |

**Slug is a public URL fragment.** Renaming a slug breaks SEO and bookmarks — consider keeping the old row with a redirect, or maintain a redirect table.

## Common Mistakes

- **Missing `TenantId` on the DTO** before upsert — the service uses `seoMeta.TenantId` to find the existing row. Empty/null → mis-scoped lookups.
- **Non-unique slugs per tenant** — allowed by the data model, not by SEO. Enforce with a DB unique index (migration) and by validating in the admin UI.
- **Keyword stuffing** — `Keywords` is stored verbatim. Google ignores it, but admins might still abuse the field. Leave validation to the UI.
- **Leaving `Og*` fields null** — social shares fall back to the full page HTML. For consistent share cards, always populate at least `OgTitle` + `OgImageUrl`.
- **Hard-deleting** via `SharedDbContext` — bypasses audit. Use `DeleteSeoMetaAsync` (soft).
- **Cross-entity reuse of a row** — don't re-point an existing row to a different `EntityId`; insert a new one. Rewriting `EntityType`/`EntityId` mid-life loses history.

## See Also

- [11 — EF Core Repository](./11-ef-core-repository.md) — the `SharedDbContext` this service uses
- [19 — Tag Service](./19-tag-service.md) — sibling polymorphic-on-entity pattern
- [00 — Getting Started](./00-getting-started.md) — DI wiring

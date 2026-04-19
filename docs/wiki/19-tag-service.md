# Tag Service

Polymorphic tagging — attach arbitrary labels to any entity by `(EntityType, EntityId)`. Used for search facets, dashboard filtering, and navigation chips across all client types.

## Design note (important)

The current `Tag` entity stores **the tag name *and* the entity association in a single row**:

```
Tag { TagId, TagName, EntityType, EntityId, TenantId, IsActive, IsDeleted, … }
```

That means "apply tag `featured` to product #42" is a `Tag` row with `TagName = "featured"`, `EntityType = "Product"`, `EntityId = 42`. Tagging the same label on another product inserts **another row** with a different `EntityId` but the same `TagName`.

This is simpler to query but has two consequences to be aware of:

1. **There is no single canonical "tag" entity.** Renaming a tag means updating every row where `TagName = "old"`.
2. **Orphan rows are possible.** `RemoveTagFromEntityAsync` clears `EntityId`/`EntityType` and leaves the row in place. If you want it gone, call `DeleteTagAsync`.

If you migrate to a `Tag` + `TagAssignment` split (recommended for large catalogues), it is a breaking change — update this wiki and the client code in the same PR.

## Purpose

- **Tag any entity.** The service doesn't care what `EntityType` means — use `"Product"`, `"Article"`, `"Order"`, whatever your code owns.
- **Per-tenant isolation.** Every tag row carries `TenantId`.
- **Fast read paths.** `GetTagsByEntityAsync` and `GetTagsByNameAsync` are plain indexed lookups.
- **Soft delete.** `DeleteTagAsync` flips `IsDeleted`; history preserved for audit.

## Architecture

| Component | Role | File |
|-----------|------|------|
| `ITagService` | Contract | [`Application/Services/ITagService.cs`](../../src/SmartWorkz.StarterKitMVC.Application/Services/ITagService.cs) |
| `TagService` | EF Core implementation against `SharedDbContext` | [`Infrastructure/Services/TagService.cs`](../../src/SmartWorkz.StarterKitMVC.Infrastructure/Services/TagService.cs) |
| `Tag` | Shared.Tags row | [`Domain/Entities/Shared/Tag.cs`](../../src/SmartWorkz.StarterKitMVC.Domain/Entities/Shared/Tag.cs) |

### Entity shape

| Field | Notes |
|-------|-------|
| `TagId` | `int` identity |
| `TagName` | The label text (`"featured"`, `"discontinued"`, …) |
| `EntityType` / `EntityId` | Which entity the tag is attached to; both can be cleared to "orphan" the tag |
| `TenantId` | Multi-tenant scope |
| `IsActive` / `IsDeleted` | Soft toggle + soft delete |

## DI Registration

Wired by `AddApplicationServices`:

```csharp
services.AddScoped<ITagService, TagService>();
```

## Quick Start

### Tag an entity

```csharp
// Create one tag row per (name, entity) pair
foreach (var name in new[] { "featured", "new-arrival", "sale" })
{
    await _tags.CreateTagAsync("acme", new Tag
    {
        TagName    = name,
        EntityType = "Product",
        EntityId   = product.ProductId,
        CreatedBy  = currentUserId
    });
}
```

### Read the tags of an entity

```csharp
var tags = await _tags.GetTagsByEntityAsync("acme", "Product", product.ProductId);
var names = tags.Select(t => t.TagName).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
```

### Find entities by tag

`GetTagsByNameAsync` returns every row with that `TagName` — read `EntityType`/`EntityId` to join back to the owning entity.

```csharp
var featured = await _tags.GetTagsByNameAsync("acme", "featured");
var productIds = featured
    .Where(t => t.EntityType == "Product")
    .Select(t => t.EntityId)
    .ToList();

var products = await _products.GetByIdsAsync(productIds);   // your own batch lookup
```

### Expose via REST for SPA / mobile

```csharp
[ApiController, Route("api/tags")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tags;

    [HttpGet("entity/{type}/{id:int}")]
    public async Task<ActionResult<IEnumerable<TagDto>>> ForEntity(string type, int id)
    {
        var tenantId = HttpContext.Items["TenantId"] as string ?? "DEFAULT";
        var tags = await _tags.GetTagsByEntityAsync(tenantId, type, id);
        return Ok(tags.Select(TagDto.From));
    }

    [HttpGet("by-name/{name}")]
    public async Task<ActionResult<IEnumerable<TagDto>>> ByName(string name)
    {
        var tenantId = HttpContext.Items["TenantId"] as string ?? "DEFAULT";
        var tags = await _tags.GetTagsByNameAsync(tenantId, name);
        return Ok(tags.Select(TagDto.From));
    }
}
```

## Method Reference

```csharp
Task<List<Tag>> GetTagsByEntityAsync(string tenantId, string entityType, int entityId);
Task<List<Tag>> GetTagsByNameAsync(string tenantId, string tagName);
Task<Tag>       CreateTagAsync(string tenantId, Tag tag);
Task<bool>      AssignTagToEntityAsync(int tagId, string entityType, int entityId);
Task<bool>      RemoveTagFromEntityAsync(int tagId);
Task<bool>      DeleteTagAsync(int tagId);
```

### `GetTagsByEntityAsync(tenantId, entityType, entityId)` → `List<Tag>`

Returns every non-deleted tag row for the given `(tenant, entity type, entity id)`. Order is not guaranteed — sort on the client if you need consistent display.

### `GetTagsByNameAsync(tenantId, tagName)` → `List<Tag>`

Exact-match lookup across all entities in the tenant. For partial-match (e.g. "fea..." autocomplete), extend the service with a dedicated method rather than scanning in memory.

### `CreateTagAsync(tenantId, tag)` → `Tag`

Inserts a new row with `TenantId` and `CreatedAt` set. Returns the saved entity (now has `TagId`).

### `AssignTagToEntityAsync(tagId, entityType, entityId)` → `bool`

Re-points an existing tag row to a different entity. **This does not create a new row.** If you want "tag `featured` now also applies to product #99", call `CreateTagAsync` with a fresh row, not `AssignTagToEntityAsync`.

Returns `false` if the tag doesn't exist.

### `RemoveTagFromEntityAsync(tagId)` → `bool`

Clears `EntityType`/`EntityId` on the row. The row remains (orphan). Combine with a nightly job if you want to purge orphans, or call `DeleteTagAsync` directly when you know you'll never reuse the label.

Returns `false` if the tag doesn't exist.

### `DeleteTagAsync(tagId)` → `bool`

Soft delete — flips `IsDeleted`. Returns `true` when a row was found, `false` otherwise.

## Cross-Client Notes

| Client | Typical use |
|--------|-------------|
| **Razor Pages / MVC** | Render chips under product/article headers; filter lists by tag facet |
| **Angular / React** | Autocomplete chip editor on admin forms; filter facet on product listing |
| **.NET MAUI** | Filter chips on catalogue drawer; offline snapshot for tag facets |
| **WPF / WinUI** | Ribbon filters keyed on tag list |

`TagName` is a user-visible label — treat it as a public string. If you introduce a canonical name vs display label (`slug` vs `displayName`), call that out here and update every client editor in the same PR.

## Common Mistakes

- **Treating `Tag` as an upsert by name** — it isn't. `CreateTagAsync` inserts unconditionally. To avoid duplicates for the same `(TagName, EntityType, EntityId)`, check first or add a DB unique index.
- **Orphaning instead of deleting** — `RemoveTagFromEntityAsync` leaves the row with null/zero entity id. Fine for audit, bad for search indexes that don't filter on entity presence.
- **Renaming a tag globally** — there's no helper for this. A single UPDATE on `(TenantId, TagName = old) SET TagName = new` covers the rename; wrap it in a method if you do it often.
- **Cross-tenant lookups** — never call `GetTagsByNameAsync` without `tenantId`. The method signature enforces it; don't add overloads that skip it.
- **Hard delete via `SharedDbContext`** — bypasses audit. Always use `DeleteTagAsync`.
- **Joining to owning entity without checking `EntityType`** — same `EntityId` (e.g. `42`) may refer to different entities. Always filter on both.

## See Also

- [18 — SEO Meta Service](./18-seo-meta-service.md) — sibling polymorphic-on-entity pattern
- [11 — EF Core Repository](./11-ef-core-repository.md) — the `SharedDbContext` this service uses
- [00 — Getting Started](./00-getting-started.md) — DI wiring

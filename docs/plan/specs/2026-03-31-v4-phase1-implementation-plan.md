# SmartWorkz v4 Phase 1 - Implementation Plan (UPDATED)

**Date:** 2026-03-31
**Status:** Ready for Development
**Total Effort:** 34-45 hours
**Schemas:** 5 (Master 18, Shared 7, Transaction 1, Report 4, Auth 13)
**Total Tables:** 43 (down from original 62)
**DbContexts:** 3-4 (ReferenceDbContext, TransactionDbContext, ReportDbContext, AuthDbContext)

---

## Key Changes in This Update

✅ **Schema Consolidation:**
- Removed Core schema (was: Menus, MenuItems, TenantFeatures moved to Master)
- Finalized 5 schemas instead of 6
- Master now: 18 tables (Geo, i18n, Hierarchies, Notifications, Tenants, SEO, Config, **Navigation**)
- Shared now: 7 tables (Addresses, Attachments, Comments, StateHistory, PreferenceDefinitions, **SeoMeta, Tags**)

✅ **SEO & Tags Architecture:**
- **SeoMeta** moved from Master → Shared (polymorphic linking via EntityType+EntityId)
- **Tags** moved from Core → Shared (polymorphic linking for any entity)
- Single table serves: Products, Categories, MenuItems, BlogPosts, GeolocationPages, CustomPages
- Single table serves: any entity (Product tags, Order tags, Customer tags, etc.)

✅ **Menu Management:**
- **Menus & MenuItems** in Master schema (Navigation section)
- HierarchyId trees for unlimited nesting
- Per-tenant menus (TenantId on both tables)
- Role-based visibility (RequiredRole, RequiredPermission)
- Auto-generates sitemap.xml

---

## Phase 1 Implementation Tasks (34-45 hours)

### Step 1: Database Scripts (12-15 hours)

Create 7 SQL migration files:

#### 001_InitializeDatabase.sql (0.5h)
```sql
CREATE DATABASE StarterKitMVC;
USE StarterKitMVC;

-- Enable HierarchyId for all databases
EXEC sp_executesql N'ALTER DATABASE StarterKitMVC ADD FILEGROUP fg_data CONTAINS FILESTREAM;'

-- Create schemas
CREATE SCHEMA Master;
CREATE SCHEMA Shared;
CREATE SCHEMA Transaction;
CREATE SCHEMA Report;
CREATE SCHEMA Auth;
```

#### 002_CreateTables_Master.sql (3h)
**18 tables: Geo, i18n, Hierarchies, Notifications, Tenants, SEO, Config, Navigation**

**Geo (2 tables):**
```sql
Master.Countries (CountryId, CountryCode, CountryName, CurrencyCode, TimeZoneId, PhoneCode, IsActive, Audit)
Master.GeoHierarchy (GeoHierarchyId, CountryId, NodePath HierarchyId, Type, Name, Code, Audit)
```

**i18n (2 tables):**
```sql
Master.Languages (LanguageId, LanguageCode, LanguageName, IsRtl, IsActive, Audit)
Master.Translations (TranslationId, TenantId, Namespace, EntityType, EntityId, Key, Value, LanguageId, Audit)
```

**Hierarchies (4 tables):**
```sql
Master.Lookups (LookupId, NodePath HierarchyId, LookupCode, DisplayText, DisplayOrder, TenantId, Audit)
Master.Categories (CategoryId, NodePath HierarchyId, CategoryType, Name, Slug, Description, TenantId, Audit)
Master.EntityStates (EntityStateId, NodePath HierarchyId, EntityType, StateCode, StateName, IsTerminal, Audit)
Master.EntityStateTransitions (TransitionId, FromStateId, ToStateId, RequiredPermission, Audit)
```

**Notifications (3 tables):**
```sql
Master.NotificationChannels (ChannelId, Code, Name, IsActive, Audit)
Master.TemplateGroups (GroupId, Code, Name, IsActive, Audit)
Master.Templates (TemplateId, GroupId, ChannelId, Code, Name, Subject, Body, TenantId, IsActive, Audit)
```

**Tenants (1 table):**
```sql
Master.Tenants (TenantId, NodePath HierarchyId, Code, Name, IsActive, Audit)
```

**SEO (1 table):**
```sql
Master.UrlRedirects (RedirectId, OldUrl, NewUrl, StatusCode, HitCount, LastHitDate, Audit)
```

**Config (3 tables):**
```sql
Master.TenantSubscriptions (SubId, TenantId, PlanCode, StartDate, EndDate, Status, Audit)
Master.TenantSettings (SettingId, TenantId, SettingKey, SettingValue, IsEncrypted, Audit)
Master.FeatureFlags (FlagId, TenantId, FeatureCode, IsEnabled, Audit)
```

**Navigation (2 tables):**
```sql
Master.Menus (MenuId, Code, Name, Description, TenantId, IsActive, DisplayOrder, Audit)
Master.MenuItems (MenuItemId, MenuId, NodePath HierarchyId, Code, Name, Url, Icon, DisplayOrder, ParentMenuItemId, IsVisible, RequiredRole, RequiredPermission, OpenInNewTab, CssClass, BadgeText, BadgeColor, TenantId, Audit)
```

#### 003_CreateTables_Shared.sql (2.5h)
**7 tables: Polymorphic infrastructure (EntityType + EntityId linking)**

```sql
Shared.Addresses (AddressId, EntityType, EntityId, Type, Street1, Street2, City, GeoHierarchyId, CountryId, PostalCode, Latitude, Longitude, IsDefault, TenantId, Audit)
Shared.Attachments (AttachmentId, EntityType, EntityId, FileName, FileUrl, FileSizeBytes, FileType, UploadedBy, TenantId, Audit)
Shared.Comments (CommentId, EntityType, EntityId, ParentCommentId, AuthorId, Content, IsApproved, TenantId, Audit)
Shared.StateHistory (HistoryId, EntityType, EntityId, OldState, NewState, ChangedBy, ChangeReason, TenantId, Audit)
Shared.PreferenceDefinitions (PrefDefId, Scope, ScopeId, PrefCode, DisplayName, PrefType, DefaultValue, IsRequired, TenantId, Audit)
Shared.SeoMeta (SeoMetaId, EntityType, EntityId, Slug, Title, Description, Keywords, OgTitle, OgDescription, OgImage, OgType, CanonicalUrl, Robots, SchemaMarkup, IsActive, TenantId, Audit)
Shared.Tags (TagId, EntityType, EntityId, TagCode, TagName, TagColor, DisplayOrder, TenantId, Audit)
```

#### 004_CreateTables_Transaction.sql (0.5h)
```sql
Transaction.Orders (OrderId, OrderNumber, TenantId, OrderDate, TotalAmount, Status, Audit)
```

#### 005_CreateTables_Report.sql (1.5h)
```sql
Report.ReportDefinitions (ReportDefId, Code, Name, Description, SqlQuery, DashboardUrl, TenantId, IsActive, Audit)
Report.ReportSchedules (ScheduleId, ReportDefId, CronExpression, IsActive, Audit)
Report.ReportExecutions (ExecutionId, ScheduleId, ExecutionTime, Duration, ResultCount, Status, ErrorMessage, Audit)
Report.ReportMetadata (MetadataId, ReportDefId, FilterDefinitions, DrillDownConfig, FormattingRules, Audit)
```

#### 006_CreateTables_Auth.sql (4h)
**13 tables: Identity, RBAC, Sessions, Logging**

```sql
Auth.Users (UserId, Email, PasswordHash, FirstName, LastName, IsActive, EmailConfirmed, TenantId, Audit)
Auth.UserProfiles (ProfileId, UserId, ProfilePictureUrl, TimeZone, PreferredLanguage, Audit)
Auth.UserPreferences (PrefId, UserId, PrefKey, PrefValue, Audit)

Auth.Roles (RoleId, Code, Name, TenantId, IsActive, Audit)
Auth.Permissions (PermId, Code, Name, Category, Audit)
Auth.RolePermissions (RolePermId, RoleId, PermId, Audit)
Auth.UserRoles (UserRoleId, UserId, RoleId, Audit)

Auth.RefreshTokens (TokenId, UserId, Token, ExpiryDate, IsRevoked, Audit)
Auth.VerificationCodes (CodeId, UserId, CodeType, CodeValue, ExpiryDate, IsUsed, Audit)
Auth.ExternalLogins (LoginId, UserId, Provider, ProviderUserId, ProviderDisplayName, Audit)

Auth.AuditLogs (AuditId, UserId, Action, EntityType, EntityId, Changes, IpAddress, UserAgent, TenantId, Audit)
Auth.ActivityLogs (ActivityId, UserId, ActivityType, Details, TenantId, Audit)
Auth.NotificationLogs (NotifLogId, UserId, NotificationType, RecipientEmail, Status, SentAt, Audit)
```

#### 007_SeedData.sql (2h)
```sql
-- Insert base data for all tenants
INSERT INTO Master.Countries ...
INSERT INTO Master.Languages ...
INSERT INTO Master.Lookups ...
INSERT INTO Master.Menus ... (Main, Admin, Footer, Sidebar)
INSERT INTO Master.MenuItems ... (hierarchical structure)
INSERT INTO Master.Templates ...
INSERT INTO Auth.Roles ... (Admin, Manager, Customer, etc.)
INSERT INTO Auth.Permissions ... (full RBAC matrix)
```

#### 008_CreateIndexes.sql (1h)
```sql
-- Performance indexes
CREATE INDEX idx_Menu_TenantId ON Master.Menus(TenantId);
CREATE INDEX idx_MenuItem_MenuId_NodePath ON Master.MenuItems(MenuId, NodePath);
CREATE INDEX idx_SeoMeta_EntityType_EntityId ON Shared.SeoMeta(EntityType, EntityId);
CREATE INDEX idx_SeoMeta_Slug ON Shared.SeoMeta(Slug) WHERE IsActive = 1;
CREATE INDEX idx_Tags_EntityType_EntityId ON Shared.Tags(EntityType, EntityId);
CREATE INDEX idx_Address_EntityType_EntityId ON Shared.Addresses(EntityType, EntityId);
CREATE INDEX idx_Comments_EntityType_EntityId ON Shared.Comments(EntityType, EntityId);
CREATE INDEX idx_StateHistory_EntityType_EntityId ON Shared.StateHistory(EntityType, EntityId);
CREATE INDEX idx_User_TenantId ON Auth.Users(TenantId);
CREATE INDEX idx_AuditLog_TenantId_UserId ON Auth.AuditLogs(TenantId, UserId);
```

---

### Step 2: Domain Entities (5-7 hours)

**43 C# entity classes** following Clean Architecture pattern.

#### Master Schema Entities (18)

**Geo:**
```csharp
Domain/Entities/Master/Country.cs
Domain/Entities/Master/GeoHierarchy.cs
```

**i18n:**
```csharp
Domain/Entities/Master/Language.cs
Domain/Entities/Master/Translation.cs
```

**Hierarchies:**
```csharp
Domain/Entities/Master/Lookup.cs
Domain/Entities/Master/Category.cs
Domain/Entities/Master/EntityState.cs
Domain/Entities/Master/EntityStateTransition.cs
```

**Notifications:**
```csharp
Domain/Entities/Master/NotificationChannel.cs
Domain/Entities/Master/TemplateGroup.cs
Domain/Entities/Master/Template.cs
```

**Tenants:**
```csharp
Domain/Entities/Master/Tenant.cs
```

**SEO:**
```csharp
Domain/Entities/Master/UrlRedirect.cs
```

**Config:**
```csharp
Domain/Entities/Master/TenantSubscription.cs
Domain/Entities/Master/TenantSetting.cs
Domain/Entities/Master/FeatureFlag.cs
```

**Navigation (NEW):**
```csharp
Domain/Entities/Master/Menu.cs
Domain/Entities/Master/MenuItem.cs
```

Example (Menu.cs):
```csharp
public class Menu : AuditedEntity
{
    public Guid MenuId { get; set; }
    public string Code { get; set; } // 'Main', 'Admin', 'Footer'
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid TenantId { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }

    // Navigation
    public virtual Tenant Tenant { get; set; }
    public virtual ICollection<MenuItem> MenuItems { get; set; }
}

public class MenuItem : AuditedEntity
{
    public Guid MenuItemId { get; set; }
    public Guid MenuId { get; set; }
    public HierarchyId NodePath { get; set; } // /1/, /1/1/, /1/1/1/
    public string Code { get; set; }
    public string Name { get; set; }
    public string Url { get; set; }
    public string Icon { get; set; }
    public int DisplayOrder { get; set; }
    public Guid? ParentMenuItemId { get; set; }
    public bool IsVisible { get; set; }
    public string RequiredRole { get; set; }
    public string RequiredPermission { get; set; }
    public bool OpenInNewTab { get; set; }
    public string CssClass { get; set; }
    public string BadgeText { get; set; }
    public string BadgeColor { get; set; }
    public Guid TenantId { get; set; }

    // Navigation
    public virtual Menu Menu { get; set; }
    public virtual MenuItem ParentMenuItem { get; set; }
    public virtual ICollection<MenuItem> ChildMenuItems { get; set; }
    public virtual Tenant Tenant { get; set; }
}
```

#### Shared Schema Entities (7)

```csharp
Domain/Entities/Shared/Address.cs
Domain/Entities/Shared/Attachment.cs
Domain/Entities/Shared/Comment.cs
Domain/Entities/Shared/StateHistory.cs
Domain/Entities/Shared/PreferenceDefinition.cs
Domain/Entities/Shared/SeoMeta.cs (NEW - from Master)
Domain/Entities/Shared/Tag.cs (NEW - from Core)
```

Example (SeoMeta.cs):
```csharp
public class SeoMeta : AuditedEntity
{
    public Guid SeoMetaId { get; set; }
    public string EntityType { get; set; } // 'Product', 'Category', 'MenuItem', 'BlogPost', 'GeolocationPage'
    public Guid EntityId { get; set; }
    public string Slug { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Keywords { get; set; }
    public string OgTitle { get; set; }
    public string OgDescription { get; set; }
    public string OgImage { get; set; }
    public string OgType { get; set; }
    public string CanonicalUrl { get; set; }
    public string Robots { get; set; } // 'index, follow' or 'noindex, nofollow'
    public string SchemaMarkup { get; set; } // JSON for schema.org
    public bool IsActive { get; set; }
    public Guid TenantId { get; set; } // NOT NULL - per-tenant isolation

    // Audit inherited from AuditedEntity
}
```

#### Transaction Schema Entities (1)

```csharp
Domain/Entities/Transaction/Order.cs
```

#### Report Schema Entities (4)

```csharp
Domain/Entities/Report/ReportDefinition.cs
Domain/Entities/Report/ReportSchedule.cs
Domain/Entities/Report/ReportExecution.cs
Domain/Entities/Report/ReportMetadata.cs
```

#### Auth Schema Entities (13)

```csharp
Domain/Entities/Auth/User.cs
Domain/Entities/Auth/UserProfile.cs
Domain/Entities/Auth/UserPreference.cs
Domain/Entities/Auth/Role.cs
Domain/Entities/Auth/Permission.cs
Domain/Entities/Auth/RolePermission.cs
Domain/Entities/Auth/UserRole.cs
Domain/Entities/Auth/RefreshToken.cs
Domain/Entities/Auth/VerificationCode.cs
Domain/Entities/Auth/ExternalLogin.cs
Domain/Entities/Auth/AuditLog.cs
Domain/Entities/Auth/ActivityLog.cs
Domain/Entities/Auth/NotificationLog.cs
```

---

### Step 3: EF Core DbContexts (6-8 hours)

**Recommended: 4 DbContexts** for clean separation & performance isolation

#### ReferenceDbContext (Master 18 + Shared 7 = 25 tables)

```csharp
Infrastructure/Data/ReferenceDbContext.cs

public class ReferenceDbContext : DbContext
{
    public ReferenceDbContext(DbContextOptions<ReferenceDbContext> options) : base(options) { }

    // Master Schema (18)
    public DbSet<Country> Countries { get; set; }
    public DbSet<GeoHierarchy> GeoHierarchies { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<Translation> Translations { get; set; }
    public DbSet<Lookup> Lookups { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<EntityState> EntityStates { get; set; }
    public DbSet<EntityStateTransition> EntityStateTransitions { get; set; }
    public DbSet<NotificationChannel> NotificationChannels { get; set; }
    public DbSet<TemplateGroup> TemplateGroups { get; set; }
    public DbSet<Template> Templates { get; set; }
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<UrlRedirect> UrlRedirects { get; set; }
    public DbSet<TenantSubscription> TenantSubscriptions { get; set; }
    public DbSet<TenantSetting> TenantSettings { get; set; }
    public DbSet<FeatureFlag> FeatureFlags { get; set; }
    public DbSet<Menu> Menus { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }

    // Shared Schema (7)
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<StateHistory> StateHistories { get; set; }
    public DbSet<PreferenceDefinition> PreferenceDefinitions { get; set; }
    public DbSet<SeoMeta> SeoMeta { get; set; }
    public DbSet<Tag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");

        // Configure Master schema
        modelBuilder.Entity<Country>().ToTable("Countries", "Master");
        modelBuilder.Entity<GeoHierarchy>().ToTable("GeoHierarchies", "Master");
        // ... (all 18 Master entities)

        // Configure Shared schema
        modelBuilder.Entity<Address>().ToTable("Addresses", "Shared");
        modelBuilder.Entity<SeoMeta>().ToTable("SeoMeta", "Shared");
        modelBuilder.Entity<Tag>().ToTable("Tags", "Shared");
        // ... (all 7 Shared entities)

        // HierarchyId configuration
        modelBuilder.Entity<MenuItem>()
            .Property(m => m.NodePath)
            .HasColumnType("hierarchyid");

        // Polymorphic configuration
        modelBuilder.Entity<SeoMeta>()
            .HasIndex(s => new { s.EntityType, s.EntityId });
        modelBuilder.Entity<Tag>()
            .HasIndex(t => new { t.EntityType, t.EntityId });

        // Soft delete filter
        modelBuilder.Entity<Menu>().HasQueryFilter(m => !m.IsDeleted);
        modelBuilder.Entity<MenuItem>().HasQueryFilter(m => !m.IsDeleted);

        base.OnModelCreating(modelBuilder);
    }
}
```

#### TransactionDbContext (1 table)

```csharp
Infrastructure/Data/TransactionDbContext.cs

public class TransactionDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>().ToTable("Orders", "Transaction");
        base.OnModelCreating(modelBuilder);
    }
}
```

#### ReportDbContext (4 tables)

```csharp
Infrastructure/Data/ReportDbContext.cs

public class ReportDbContext : DbContext
{
    public DbSet<ReportDefinition> ReportDefinitions { get; set; }
    public DbSet<ReportSchedule> ReportSchedules { get; set; }
    public DbSet<ReportExecution> ReportExecutions { get; set; }
    public DbSet<ReportMetadata> ReportMetadata { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReportDefinition>().ToTable("ReportDefinitions", "Report");
        modelBuilder.Entity<ReportSchedule>().ToTable("ReportSchedules", "Report");
        modelBuilder.Entity<ReportExecution>().ToTable("ReportExecutions", "Report");
        modelBuilder.Entity<ReportMetadata>().ToTable("ReportMetadata", "Report");
        base.OnModelCreating(modelBuilder);
    }
}
```

#### AuthDbContext (13 tables)

```csharp
Infrastructure/Data/AuthDbContext.cs

public class AuthDbContext : DbContext
{
    // Identity (3)
    public DbSet<User> Users { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<UserPreference> UserPreferences { get; set; }

    // RBAC (4)
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    // Sessions (3)
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<VerificationCode> VerificationCodes { get; set; }
    public DbSet<ExternalLogin> ExternalLogins { get; set; }

    // Logging (3)
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<NotificationLog> NotificationLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure all 13 Auth tables with schema
        modelBuilder.Entity<User>().ToTable("Users", "Auth");
        // ... (all 13 Auth entities)
        base.OnModelCreating(modelBuilder);
    }
}
```

**DI Configuration (Program.cs):**
```csharp
services.AddDbContext<ReferenceDbContext>(opts =>
    opts.UseSqlServer(connStr, opt => opt.UseHierarchyId()));
services.AddDbContext<TransactionDbContext>(opts =>
    opts.UseSqlServer(connStr));
services.AddDbContext<ReportDbContext>(opts =>
    opts.UseSqlServer(connStr));
services.AddDbContext<AuthDbContext>(opts =>
    opts.UseSqlServer(connStr));
```

---

### Step 4: Application Services (5-7 hours)

Create service layer with repository pattern + business logic.

#### Core Services

**IMenuService & MenuService**
```csharp
Application/Services/IMenuService.cs
Application/Services/Implementations/MenuService.cs

public interface IMenuService
{
    Task<MenuDto> GetMenuAsync(string code, Guid tenantId);
    Task<IEnumerable<MenuItemDto>> GetMenuItemsAsync(Guid menuId);
    Task<IEnumerable<MenuItemDto>> GetMenuTreeAsync(Guid menuId, string userRole);
    Task<IEnumerable<BreadcrumbDto>> GetBreadcrumbAsync(Guid menuItemId);
    Task<string> GenerateSitemapAsync(Guid tenantId);
    Task CreateMenuAsync(CreateMenuDto dto);
    Task UpdateMenuAsync(Guid menuId, UpdateMenuDto dto);
    Task DeleteMenuAsync(Guid menuId);
    Task CreateMenuItemAsync(CreateMenuItemDto dto);
    Task UpdateMenuItemAsync(Guid menuItemId, UpdateMenuItemDto dto);
    Task DeleteMenuItemAsync(Guid menuItemId);
}

public class MenuService : IMenuService
{
    private readonly ReferenceDbContext _db;
    private readonly ILogger<MenuService> _logger;

    public MenuService(ReferenceDbContext db, ILogger<MenuService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<MenuItemDto>> GetMenuTreeAsync(Guid menuId, string userRole)
    {
        var items = await _db.MenuItems
            .Where(m => m.MenuId == menuId
                && (m.RequiredRole == null || m.RequiredRole == userRole)
                && m.IsVisible
                && !m.IsDeleted)
            .OrderBy(m => m.NodePath)
            .ToListAsync();

        return MapToTree(items);
    }

    public async Task<IEnumerable<BreadcrumbDto>> GetBreadcrumbAsync(Guid menuItemId)
    {
        var item = await _db.MenuItems.FindAsync(menuItemId);
        var ancestors = await _db.MenuItems
            .Where(m => m.MenuId == item.MenuId
                && item.NodePath.IsDescendantOf(m.NodePath))
            .OrderBy(m => m.NodePath.GetLevel())
            .ToListAsync();

        return ancestors.Select(a => new BreadcrumbDto
        {
            Code = a.Code,
            Name = a.Name,
            Url = a.Url
        });
    }

    public async Task<string> GenerateSitemapAsync(Guid tenantId)
    {
        var items = await _db.MenuItems
            .Where(m => m.TenantId == tenantId && m.Url != null && !m.IsDeleted)
            .ToListAsync();

        return BuildSitemapXml(items);
    }
}
```

**ISeoMetaService & SeoMetaService (NEW)**
```csharp
Application/Services/ISeoMetaService.cs
Application/Services/Implementations/SeoMetaService.cs

public interface ISeoMetaService
{
    Task<SeoMetaDto> GetSeoMetaAsync(string entityType, Guid entityId);
    Task<SeoMetaDto> GetBySlugAsync(string slug, Guid tenantId);
    Task<RouteResolutionDto> ResolveSlugAsync(string slug, Guid tenantId);
    Task CreateSeoMetaAsync(CreateSeoMetaDto dto);
    Task UpdateSeoMetaAsync(Guid seoMetaId, UpdateSeoMetaDto dto);
    Task DeleteSeoMetaAsync(Guid seoMetaId);
}

public class SeoMetaService : ISeoMetaService
{
    private readonly ReferenceDbContext _db;

    public async Task<RouteResolutionDto> ResolveSlugAsync(string slug, Guid tenantId)
    {
        var seo = await _db.SeoMeta
            .FirstOrDefaultAsync(s => s.Slug == slug
                && s.TenantId == tenantId
                && s.IsActive);

        if (seo == null) return null;

        return seo.EntityType switch
        {
            "Product" => new RouteResolutionDto { Route = $"/products/{seo.EntityId}" },
            "Category" => new RouteResolutionDto { Route = $"/categories/{seo.EntityId}" },
            "MenuItem" => new RouteResolutionDto { Route = $"/navigation/{seo.EntityId}" },
            "BlogPost" => new RouteResolutionDto { Route = $"/blog/{seo.EntityId}" },
            _ => null
        };
    }
}
```

**ITagService & TagService (NEW)**
```csharp
Application/Services/ITagService.cs
Application/Services/Implementations/TagService.cs

public interface ITagService
{
    Task<IEnumerable<TagDto>> GetTagsAsync(string entityType, Guid entityId);
    Task<IEnumerable<TagDto>> GetTagsByTypeAsync(string entityType, Guid tenantId);
    Task CreateTagAsync(CreateTagDto dto);
    Task UpdateTagAsync(Guid tagId, UpdateTagDto dto);
    Task DeleteTagAsync(Guid tagId);
}

public class TagService : ITagService
{
    private readonly ReferenceDbContext _db;

    public async Task<IEnumerable<TagDto>> GetTagsAsync(string entityType, Guid entityId)
    {
        return await _db.Tags
            .Where(t => t.EntityType == entityType && t.EntityId == entityId && !t.IsDeleted)
            .OrderBy(t => t.DisplayOrder)
            .Select(t => new TagDto { TagId = t.TagId, TagCode = t.TagCode, TagName = t.TagName })
            .ToListAsync();
    }
}
```

**Other Services:**
- TenantService
- UserService, RoleService, PermissionService
- TemplateService, NotificationService
- ReportService
- LookupsService

#### Repository Pattern

```csharp
Application/Repositories/IRepository.cs

public interface IRepository<T> where T : Entity
{
    Task<T> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(Guid id);
    Task<int> SaveChangesAsync();
}

public class Repository<T> : IRepository<T> where T : Entity
{
    protected readonly ReferenceDbContext _db;

    public Repository(ReferenceDbContext db) => _db = db;

    public async Task<T> GetByIdAsync(Guid id) => await _db.Set<T>().FindAsync(id);
    // ... (implement all methods)
}
```

---

### Step 5: REST API Endpoints (6-8 hours)

**25+ endpoints** across 5 controllers

#### MenusController
```csharp
[ApiController]
[Route("api/v1/menus")]
public class MenusController : ControllerBase
{
    private readonly IMenuService _menuService;

    [HttpGet("{code}")]
    public async Task<IActionResult> GetMenu(string code, [FromQuery] Guid tenantId)
    {
        var menu = await _menuService.GetMenuAsync(code, tenantId);
        return Ok(menu);
    }

    [HttpGet("{menuId}/items")]
    public async Task<IActionResult> GetMenuItems(Guid menuId)
    {
        var items = await _menuService.GetMenuItemsAsync(menuId);
        return Ok(items);
    }

    [HttpGet("{menuId}/tree")]
    public async Task<IActionResult> GetMenuTree(Guid menuId)
    {
        var userRole = User.GetRole();
        var tree = await _menuService.GetMenuTreeAsync(menuId, userRole);
        return Ok(tree);
    }

    [HttpGet("{menuItemId}/breadcrumb")]
    public async Task<IActionResult> GetBreadcrumb(Guid menuItemId)
    {
        var breadcrumb = await _menuService.GetBreadcrumbAsync(menuItemId);
        return Ok(breadcrumb);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateMenu(CreateMenuDto dto)
    {
        await _menuService.CreateMenuAsync(dto);
        return Created("", null);
    }

    [HttpPut("{menuId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateMenu(Guid menuId, UpdateMenuDto dto)
    {
        await _menuService.UpdateMenuAsync(menuId, dto);
        return NoContent();
    }

    [HttpDelete("{menuId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteMenu(Guid menuId)
    {
        await _menuService.DeleteMenuAsync(menuId);
        return NoContent();
    }
}
```

#### SeoMetaController (NEW)
```csharp
[ApiController]
[Route("api/v1/seometa")]
public class SeoMetaController : ControllerBase
{
    private readonly ISeoMetaService _seoService;

    [HttpGet("by-entity")]
    public async Task<IActionResult> GetByEntity([FromQuery] string entityType, [FromQuery] Guid entityId)
    {
        var seo = await _seoService.GetSeoMetaAsync(entityType, entityId);
        return Ok(seo);
    }

    [HttpGet("by-slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, [FromQuery] Guid tenantId)
    {
        var seo = await _seoService.GetBySlugAsync(slug, tenantId);
        return Ok(seo);
    }

    [HttpGet("resolve/{slug}")]
    public async Task<IActionResult> ResolveSlug(string slug, [FromQuery] Guid tenantId)
    {
        var route = await _seoService.ResolveSlugAsync(slug, tenantId);
        return Ok(route);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateSeoMeta(CreateSeoMetaDto dto)
    {
        await _seoService.CreateSeoMetaAsync(dto);
        return Created("", null);
    }
}
```

#### TagsController (NEW)
```csharp
[ApiController]
[Route("api/v1/tags")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;

    [HttpGet]
    public async Task<IActionResult> GetTags([FromQuery] string entityType, [FromQuery] Guid entityId)
    {
        var tags = await _tagService.GetTagsAsync(entityType, entityId);
        return Ok(tags);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateTag(CreateTagDto dto)
    {
        await _tagService.CreateTagAsync(dto);
        return Created("", null);
    }
}
```

**Other Controllers:**
- AuthController (login, register, refresh token)
- UsersController
- RolesController
- TenantsController
- LookupsController
- ReportsController

---

### Step 6: DTOs & AutoMapper (2-3 hours)

```csharp
Application/DTOs/Menu/MenuDto.cs
Application/DTOs/Menu/CreateMenuDto.cs
Application/DTOs/Menu/UpdateMenuDto.cs
Application/DTOs/Menu/MenuItemDto.cs
Application/DTOs/Menu/CreateMenuItemDto.cs
Application/DTOs/Menu/BreadcrumbDto.cs

Application/DTOs/SEO/SeoMetaDto.cs
Application/DTOs/SEO/CreateSeoMetaDto.cs
Application/DTOs/SEO/RouteResolutionDto.cs

Application/DTOs/Tags/TagDto.cs
Application/DTOs/Tags/CreateTagDto.cs

// AutoMapper Profile
public class MenuMappingProfile : Profile
{
    public MenuMappingProfile()
    {
        CreateMap<Menu, MenuDto>();
        CreateMap<CreateMenuDto, Menu>();
        CreateMap<MenuItem, MenuItemDto>();
        CreateMap<SeoMeta, SeoMetaDto>();
        CreateMap<Tag, TagDto>();
    }
}
```

---

### Step 7: Configuration & DI (1-2 hours)

```csharp
Program.cs

// Database
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ReferenceDbContext>(opts =>
    opts.UseSqlServer(connStr, opt => opt.UseHierarchyId()));
builder.Services.AddDbContext<TransactionDbContext>(opts =>
    opts.UseSqlServer(connStr));
builder.Services.AddDbContext<ReportDbContext>(opts =>
    opts.UseSqlServer(connStr));
builder.Services.AddDbContext<AuthDbContext>(opts =>
    opts.UseSqlServer(connStr));

// Services
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<ISeoMetaService, SeoMetaService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITenantService, TenantService>();
// ... (all services)

// AutoMapper
builder.Services.AddAutoMapper(typeof(MenuMappingProfile));

// Swagger
builder.Services.AddSwaggerGen(opts =>
{
    opts.SwaggerDoc("v1", new OpenApiInfo { Title = "SmartWorkz v4 API", Version = "v1.0" });
    opts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { ... });
});
```

---

## Testing Strategy (8-10 hours - Phase 1+)

### Unit Tests
- Service logic (MenuService.GetMenuTreeAsync, SeoMetaService.ResolveSlugAsync)
- Repository CRUD operations
- Validation rules

### Integration Tests
- Menu creation with hierarchical structure
- SEO slug lookup with polymorphic resolution
- Tag association across entity types
- Multi-tenant isolation
- HierarchyId queries (ancestors, descendants, breadcrumbs)

### API Tests
- Endpoint contracts (200, 400, 401, 403, 404)
- Authentication/Authorization
- Request/response format validation

---

## Success Criteria

✅ Database schema deployed (43 tables, 5 schemas)
✅ All 43 domain entities created
✅ 4 DbContexts configured & working
✅ 25+ API endpoints operational
✅ Menu system fully functional (hierarchical, role-based, sitemap)
✅ SEO system fully functional (polymorphic, slug lookup, route resolution)
✅ Tag system fully functional (polymorphic, any entity)
✅ Multi-tenancy working (TenantId isolation)
✅ Authentication/Authorization working
✅ Swagger documentation complete
✅ Ready for Blazor/MAUI clients

---

## Timeline

| Phase | Tasks | Hours | Status |
|-------|-------|-------|--------|
| 1.1 | Database scripts | 12-15 | 🟡 Ready to start |
| 1.2 | Domain entities | 5-7 | 🟡 Ready to start |
| 1.3 | DbContexts | 6-8 | 🟡 Ready to start |
| 1.4 | Services | 5-7 | 🟡 Ready to start |
| 1.5 | API endpoints | 6-8 | 🟡 Ready to start |
| 1.6 | DTOs & AutoMapper | 2-3 | 🟡 Ready to start |
| 1.7 | Config & DI | 1-2 | 🟡 Ready to start |
| **TOTAL** | **Phase 1** | **34-45** | 🟡 Ready to start |

---

## Phase 1+ Enhancements (Future)

- Add 8 critical missing tables (workflows, API keys, audit trail enhancements)
- Add 13 e-commerce tables (wishlists, reviews, coupons, inventory)
- Integration tests (full coverage)
- Performance optimization (Dapper for high-throughput queries)
- Caching strategy (Redis for menus, SEO, tags)

---

## Key Files Reference

**Documentation:**
- `docs/srs/V4-ARCHITECTURE-FINAL.md` — Complete architecture overview
- `docs/srs/SCHEMA-REVIEW-v2.md` — Full schema documentation
- `docs/srs/MENU-MANAGEMENT-QUICK-GUIDE.md` — Menu system specification
- `docs/srs/SEO-QUICK-REFERENCE.md` — SEO system specification
- `docs/srs/SEO-SHARED-IMPLEMENTATION-CHECKLIST.md` — SEO implementation tasks

**SQL Scripts Location:**
- `src/SmartWorkz.StarterKitMVC.Infrastructure/Data/Migrations/001-008*.sql`

**C# Project Structure:**
- `Domain/Entities/Master/` — Master schema entities (18)
- `Domain/Entities/Shared/` — Shared schema entities (7)
- `Domain/Entities/Transaction/` — Transaction schema entities (1)
- `Domain/Entities/Report/` — Report schema entities (4)
- `Domain/Entities/Auth/` — Auth schema entities (13)
- `Infrastructure/Data/` — DbContext classes (4)
- `Application/Services/` — Service implementations
- `Application/DTOs/` — Data transfer objects
- `Web/Controllers/Api/V1/` — REST API controllers

---

**Status:** ✅ Ready for Phase 1 Implementation

Next: Create database migration scripts (001-008).

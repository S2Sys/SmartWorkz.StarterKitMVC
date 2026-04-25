# Pages API Reference

## Classes & Interfaces

### BaseListPage`1

- **Namespace:** `SmartWorkz.StarterKitMVC.Admin.Pages.BaseListPage`1`
- **Summary:** Base for all Admin list pages backed by IDapperRepository<T>.
             Wires search, sort, pagination, and HTMX partial response automatically.
            
             Usage:
               public class IndexModel : BaseListPage<User>
               {
                   public IndexModel(IDapperRepository<User> repo) : base(repo) { }
                   // Override BuildFilter() to add entity-specific WHERE conditions
               }

#### Methods & Properties

- **LoadAsync** - Call from OnGetAsync / OnGetTableAsync.
            Pass htmxTarget/htmxHandler when list page uses HTMX partial updates.
- **BuildFilter** - Override to supply entity-specific filter object.
            Default uses TenantId only.
- **PageOrPartial** - Returns a partial for HTMX requests, full page otherwise.
            Call this instead of Page() in OnGetAsync.

### BasePage

- **Namespace:** `SmartWorkz.StarterKitMVC.Admin.Pages.BasePage`
- **Summary:** Admin portal base PageModel extending Shared.BasePage.
            Adds translation and service access specific to Admin.

### BasePage`1

- **Namespace:** `SmartWorkz.StarterKitMVC.Admin.Pages.BasePage`1`
- **Summary:** Generic variant of Admin.BasePage. Adds a typed Model property for detail/form pages.
            T is the data model type used on the page (e.g., UserDto, ProductDto).

### BaseListPage`1

- **Namespace:** `SmartWorkz.StarterKitMVC.Public.Pages.BaseListPage`1`
- **Summary:** Base for all Public list pages backed by IDapperRepository<T>.
            Wires search, sort, pagination, and HTMX partial response automatically.

### BasePage

- **Namespace:** `SmartWorkz.StarterKitMVC.Public.Pages.BasePage`
- **Summary:** Public portal base PageModel extending Shared.BasePage.
            Adds translation and service access specific to Public.

### BasePage`1

- **Namespace:** `SmartWorkz.StarterKitMVC.Public.Pages.BasePage`1`
- **Summary:** Generic variant of Public.BasePage. Adds a typed Model property for detail/form pages.
            T is the data model type used on the page (e.g., UserDto, ProductDto).

### SeoBasePage

- **Namespace:** `SmartWorkz.StarterKitMVC.Public.Pages.SeoBasePage`
- **Summary:** Extends BasePage with automatic SEO meta loading from DB.
            Sets ViewData Title, Description, Keywords, OG tags.
            Usage: call LoadSeoAsync(entityType, entityId) in OnGetAsync.
            For static pages: call SetSeo(title, description).

### BaseListPage`1

- **Namespace:** `SmartWorkz.StarterKitMVC.Public.Pages.BaseListPage`1`
- **Summary:** Base for all Public list pages backed by IDapperRepository<T>.
            Wires search, sort, pagination, and HTMX partial response automatically.

### BasePage

- **Namespace:** `SmartWorkz.StarterKitMVC.Public.Pages.BasePage`
- **Summary:** Public portal base PageModel extending Shared.BasePage.
            Adds translation and service access specific to Public.

### BasePage`1

- **Namespace:** `SmartWorkz.StarterKitMVC.Public.Pages.BasePage`1`
- **Summary:** Generic variant of Public.BasePage. Adds a typed Model property for detail/form pages.
            T is the data model type used on the page (e.g., UserDto, ProductDto).

### SeoBasePage

- **Namespace:** `SmartWorkz.StarterKitMVC.Public.Pages.SeoBasePage`
- **Summary:** Extends BasePage with automatic SEO meta loading from DB.
            Sets ViewData Title, Description, Keywords, OG tags.
            Usage: call LoadSeoAsync(entityType, entityId) in OnGetAsync.
            For static pages: call SetSeo(title, description).

### BaseListPage`1

- **Namespace:** `SmartWorkz.StarterKitMVC.Admin.Pages.BaseListPage`1`
- **Summary:** Base for all Admin list pages backed by IDapperRepository<T>.
             Wires search, sort, pagination, and HTMX partial response automatically.
            
             Usage:
               public class IndexModel : BaseListPage<User>
               {
                   public IndexModel(IDapperRepository<User> repo) : base(repo) { }
                   // Override BuildFilter() to add entity-specific WHERE conditions
               }

#### Methods & Properties

- **LoadAsync** - Call from OnGetAsync / OnGetTableAsync.
            Pass htmxTarget/htmxHandler when list page uses HTMX partial updates.
- **BuildFilter** - Override to supply entity-specific filter object.
            Default uses TenantId only.
- **PageOrPartial** - Returns a partial for HTMX requests, full page otherwise.
            Call this instead of Page() in OnGetAsync.

### BasePage

- **Namespace:** `SmartWorkz.StarterKitMVC.Admin.Pages.BasePage`
- **Summary:** Admin portal base PageModel extending Shared.BasePage.
            Adds translation and service access specific to Admin.

### BaseListPage`1

- **Namespace:** `SmartWorkz.StarterKitMVC.Admin.Pages.BaseListPage`1`
- **Summary:** Base for all Admin list pages backed by IDapperRepository<T>.
             Wires search, sort, pagination, and HTMX partial response automatically.
            
             Usage:
               public class IndexModel : BaseListPage<User>
               {
                   public IndexModel(IDapperRepository<User> repo) : base(repo) { }
                   // Override BuildFilter() to add entity-specific WHERE conditions
               }

#### Methods & Properties

- **LoadAsync** - Call from OnGetAsync / OnGetTableAsync.
            Pass htmxTarget/htmxHandler when list page uses HTMX partial updates.
- **BuildFilter** - Override to supply entity-specific filter object.
            Default uses TenantId only.
- **PageOrPartial** - Returns a partial for HTMX requests, full page otherwise.
            Call this instead of Page() in OnGetAsync.

### BasePage

- **Namespace:** `SmartWorkz.StarterKitMVC.Admin.Pages.BasePage`
- **Summary:** Admin portal base PageModel extending Shared.BasePage.
            Adds translation and service access specific to Admin.

### BaseListPage`1

- **Namespace:** `SmartWorkz.StarterKitMVC.Public.Pages.BaseListPage`1`
- **Summary:** Base for all Public list pages backed by IDapperRepository<T>.
            Wires search, sort, pagination, and HTMX partial response automatically.

### BasePage

- **Namespace:** `SmartWorkz.StarterKitMVC.Public.Pages.BasePage`
- **Summary:** Public portal base PageModel extending Shared.BasePage.
            Adds translation and service access specific to Public.

### SeoBasePage

- **Namespace:** `SmartWorkz.StarterKitMVC.Public.Pages.SeoBasePage`
- **Summary:** Extends BasePage with automatic SEO meta loading from DB.
            Sets ViewData Title, Description, Keywords, OG tags.
            Usage: call LoadSeoAsync(entityType, entityId) in OnGetAsync.
            For static pages: call SetSeo(title, description).


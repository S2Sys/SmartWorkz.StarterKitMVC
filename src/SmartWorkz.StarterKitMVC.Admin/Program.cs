using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using SmartWorkz.StarterKitMVC.Infrastructure.Authorization;
using SmartWorkz.StarterKitMVC.Infrastructure.Extensions;
using SmartWorkz.StarterKitMVC.Admin.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure logging to console (will be written to file via EventLog in appsettings)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// ─── Services ─────────────────────────────────────────────────────────────
builder.Services.AddRazorPages(options =>
{
    // Require authentication for everything by default
    // options.Conventions.AuthorizeFolder("/");

    // Public exceptions — no auth required
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/Error");

    // Section-level policies (role-based)
    options.Conventions.AuthorizePage("/Dashboard/Index", "RequireAdmin");
    options.Conventions.AuthorizeFolder("/Users",       "RequireAdmin");
    options.Conventions.AuthorizeFolder("/Tenants",     "RequireAdmin");
    options.Conventions.AuthorizeFolder("/Permissions", "RequireAdmin");
    options.Conventions.AuthorizeFolder("/Settings",    "RequireAdmin");

    // Profile diagnostic page — any authenticated user
    options.Conventions.AuthorizePage("/Account/Profile");
});

// Infrastructure stack: DbContexts, repositories, application services, JWT auth
builder.Services.AddApplicationStack(builder.Configuration);

// Override authentication scheme: use Cookie instead of Bearer for UI
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme          = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme    = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath        = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan   = TimeSpan.FromHours(4);
    options.SlidingExpiration = false;
    options.Cookie.Name       = ".Admin.Auth";
    options.Cookie.HttpOnly   = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite   = SameSiteMode.Lax; // Lax allows cookies on POST redirects; Strict would block the redirect
});

// RBAC policies
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, TenantAdminRequirementHandler>();
builder.Services.AddAuthorization(options =>
{
    // ─── Role-Based Policies ──────────────────────────────────────────────────

    // Admin role: full access to all admin sections (tenant-scoped or super admin)
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("admin", "super_admin"));

    // Super admin only: access to system-wide operations
    options.AddPolicy("RequireSuperAdmin", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("super_admin"));

    // ─── Tenant-Scoped Policies ───────────────────────────────────────────────

    // Tenant admin (admin for current tenant OR super admin)
    options.AddPolicy("RequireTenantAdmin", policy =>
        policy.RequireAuthenticatedUser()
              .AddRequirements(new TenantAdminRequirement()));

    // ─── Permission-Based Policies ─────────────────────────────────────────────

    // Manage users within tenant
    options.AddPolicy("CanManageUsers", policy =>
        policy.RequireAuthenticatedUser()
              .AddRequirements(new PermissionRequirement("Manage Users")));

    // Manage menus within tenant
    options.AddPolicy("CanManageMenus", policy =>
        policy.RequireAuthenticatedUser()
              .AddRequirements(new PermissionRequirement("Manage Menu")));

    // View reports within tenant
    options.AddPolicy("CanViewReports", policy =>
        policy.RequireAuthenticatedUser()
              .AddRequirements(new PermissionRequirement("View Report")));
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");

// ─── App Pipeline ─────────────────────────────────────────────────────────
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");
app.UseHttpsRedirection();
app.UseStaticFiles();

// Add file logging middleware (logs all requests/responses to ~/Logs/)
app.UseFileLogging();

app.UseRouting();
app.UseAuthentication();
app.UseTenantResolution();

// Validate and enrich authorization (roles, claims, permissions)
app.UseAuthorizationValidation();

// Inject permission claims from DB into the cookie identity BEFORE authorization runs
app.UseMiddleware<PermissionMiddleware>();

app.UseAuthorization();

app.MapRazorPages();
app.Run();

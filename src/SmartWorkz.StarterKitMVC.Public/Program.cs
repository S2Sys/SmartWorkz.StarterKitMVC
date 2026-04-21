using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using SmartWorkz.StarterKitMVC.Infrastructure.Authorization;
using SmartWorkz.StarterKitMVC.Infrastructure.Extensions;
using SmartWorkz.StarterKitMVC.Public.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure logging to use Serilog (initialized via AddStructuredLogging in AddApplicationStack)
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

// ─── Services ─────────────────────────────────────────────────────────────
builder.Services.AddRazorPages(options =>
{
    // Default: authorize all pages
    options.Conventions.AuthorizeFolder("/");
    // Allow anonymous access to account and public pages
    options.Conventions.AllowAnonymousToFolder("/Account");
    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToPage("/Error");
    options.Conventions.AllowAnonymousToPage("/Products/Index");
    options.Conventions.AllowAnonymousToPage("/Products/Details");
});

// Infrastructure stack: DbContexts, repositories, application services, JWT auth
builder.Services.AddApplicationStack(builder.Configuration);

// Override authentication scheme: use Cookie instead of Bearer for UI
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.Name = ".Public.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax; // Lax allows cookies on POST redirects; Strict would block the redirect
});

// Permission-based authorization
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder(CookieAuthenticationDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddHttpContextAccessor();

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
app.UseRouting();
app.UseAuthentication();
app.UseTenantResolution();
app.UseAuthorization();

// Inject permission claims from roles into cookie identity
app.UseMiddleware<PermissionMiddleware>();

app.MapRazorPages();
app.Run();

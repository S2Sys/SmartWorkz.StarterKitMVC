using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Abstractions;
using SmartWorkz.StarterKitMVC.Infrastructure.AI;
using SmartWorkz.StarterKitMVC.Infrastructure.Auditing;
using SmartWorkz.StarterKitMVC.Infrastructure.BackgroundJobs;
using SmartWorkz.StarterKitMVC.Infrastructure.Configuration;
using SmartWorkz.StarterKitMVC.Infrastructure.Features;
using SmartWorkz.StarterKitMVC.Infrastructure.Http;
using SmartWorkz.StarterKitMVC.Infrastructure.Storage;
using SmartWorkz.StarterKitMVC.Infrastructure.Logging;
using SmartWorkz.StarterKitMVC.Infrastructure.Resilience;
using SmartWorkz.StarterKitMVC.Infrastructure.Telemetry;
using SmartWorkz.StarterKitMVC.Infrastructure.EmailTemplates;
using SmartWorkz.StarterKitMVC.Infrastructure.Notifications;
using SmartWorkz.StarterKitMVC.Application.Notifications;
using SmartWorkz.StarterKitMVC.Application.Authorization;
using SmartWorkz.StarterKitMVC.Application.Localization;
using SmartWorkz.StarterKitMVC.Infrastructure.Authorization;
using SmartWorkz.StarterKitMVC.Infrastructure.Localization;
using SmartWorkz.StarterKitMVC.Infrastructure.Extensions;
using SmartWorkz.StarterKitMVC.Shared.Primitives;
using SmartWorkz.StarterKitMVC.Web.Configuration;
using SmartWorkz.StarterKitMVC.Web.Middleware;
using SmartWorkz.StarterKitMVC.Web.Localization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

// Application Stack (DbContexts, Repositories, Services, AutoMapper)
builder.Services.AddApplicationStack(builder.Configuration);

// UI Settings
builder.Services.Configure<UISettings>(builder.Configuration.GetSection(UISettings.SectionName));

builder.Services.AddLocalization();

builder.Services.AddScoped<ICorrelationContext, CorrelationContext>();
builder.Services.AddScoped(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>));

builder.Services.AddSingleton<ITelemetryConfigurator, NoOpTelemetryConfigurator>();
builder.Services.AddSingleton<IResiliencePolicyProvider, SimpleResiliencePolicyProvider>();

builder.Services.AddScoped<SmartWorkz.StarterKitMVC.Application.Abstractions.IConfigurationProvider, AppConfigurationProvider>();
builder.Services.AddSingleton<IFeatureFlagService, InMemoryFeatureFlagService>();
builder.Services.AddSingleton<IAuditLogger, NoOpAuditLogger>();
builder.Services.AddSingleton<IBackgroundJobScheduler, InMemoryBackgroundJobScheduler>();
builder.Services.AddSingleton<ILocalStorage, JsonFileLocalStorage>();
builder.Services.AddSingleton<IAiClient, NoOpAiClient>();

builder.Services.AddHttpClient<IHttpService, HttpService>();

// Notifications
builder.Services.AddSingleton<INotificationQueue, InMemoryNotificationQueue>();

// Email Templates
builder.Services.AddEmailTemplates();

// Authorization & Permissions
builder.Services.AddSingleton<IPermissionService, PermissionService>();
builder.Services.AddSingleton<IClaimService, ClaimService>();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorization(options =>
{
    // Dynamic policy registration for permissions
    // Usage: [Authorize(Policy = "Permission:PRODUCT_READ")]
    options.DefaultPolicy = new AuthorizationPolicy(
        new[] { new PermissionRequirement("default") },
        new[] { "Bearer" });
});

// Localization Resources
builder.Services.AddSingleton<IResourceService, ResourceService>();
builder.Services.AddScoped<IViewLocalizer, ViewLocalizer>();
builder.Services.AddHttpContextAccessor();

// API Versioning - uncomment and add Asp.Versioning.Mvc package to enable
// builder.Services.AddApiVersioning(options =>
// {
//     options.DefaultApiVersion = new ApiVersion(1, 0);
//     options.AssumeDefaultVersionWhenUnspecified = true;
// });
    
var app = builder.Build();

// Seed default email templates
await app.Services.SeedDefaultEmailTemplatesAsync();

// Sync default localization resources (adds new resources without overwriting existing)
await app.Services.SyncDefaultResourcesAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error/500");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/error/{0}");

app.UseHttpsRedirection();

app.UseRequestLocalization();

// Global exception handling middleware
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseRouting();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Permission claims middleware (adds permission claims based on roles)
app.UsePermissions();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// API routes
app.MapControllers();

app.Run();

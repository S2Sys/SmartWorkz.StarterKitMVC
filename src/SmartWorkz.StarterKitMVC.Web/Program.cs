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
using SmartWorkz.StarterKitMVC.Shared.Primitives;
using SmartWorkz.StarterKitMVC.Web.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

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

// API Versioning - uncomment and add Asp.Versioning.Mvc package to enable
// builder.Services.AddApiVersioning(options =>
// {
//     options.DefaultApiVersion = new ApiVersion(1, 0);
//     options.AssumeDefaultVersionWhenUnspecified = true;
// });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRequestLocalization();

app.UseRouting();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

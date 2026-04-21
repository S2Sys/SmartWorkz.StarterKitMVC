using SmartWorkz.Sample.ECommerce;
using SmartWorkz.Sample.ECommerce.Infrastructure.Data;
using SmartWorkz.Sample.ECommerce.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddECommerceServices(builder.Configuration);

var app = builder.Build();

// Exception handler middleware FIRST (outermost catch)
app.UseMiddleware<ExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
// Remove else UseExceptionHandler — handled by middleware

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// MVC convention routes (existing)
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

// API attribute routes (NEW)
app.MapControllers();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
    db.Database.EnsureCreated();
    await SeedData.SeedAsync(db);
}

app.Run();

public partial class Program { }

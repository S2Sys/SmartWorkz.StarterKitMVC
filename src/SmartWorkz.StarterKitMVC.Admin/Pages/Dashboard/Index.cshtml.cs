using Microsoft.AspNetCore.Authorization;
using SmartWorkz.StarterKitMVC.Admin.Pages;

namespace SmartWorkz.StarterKitMVC.Admin.Pages.Dashboard
{
    [Authorize(Policy = "RequireAdmin")]
    public class IndexModel : BasePage
    {
        private readonly ILogger<IndexModel> _logger;

        public int TotalUsers { get; set; }
        public int TotalTenants { get; set; }
        public int TotalPermissions { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            _logger.LogInformation("Dashboard.OnGet called. User authenticated: {IsAuthenticated}, User: {User}, TenantId: {TenantId}",
                User?.Identity?.IsAuthenticated ?? false, User?.Identity?.Name ?? "N/A", TenantId);

            if (User?.Identity?.IsAuthenticated == true)
            {
                var claimsDebug = string.Join("; ", User.Claims.Select(c => $"{c.Type}={c.Value}"));
                _logger.LogInformation("User claims: {Claims}, Roles: {Roles}", claimsDebug, string.Join(",", CurrentUserRoles));
            }

            // Placeholder statistics
            TotalUsers = 42;
            TotalTenants = 5;
            TotalPermissions = 28;
        }
    }
}

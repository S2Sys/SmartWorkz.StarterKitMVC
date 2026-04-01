using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SmartWorkz.StarterKitMVC.Admin.Pages.Dashboard
{
    [Authorize(Policy = "RequireAdmin")]
    public class IndexModel : PageModel
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
            // Placeholder statistics
            TotalUsers = 42;
            TotalTenants = 5;
            TotalPermissions = 28;
        }
    }
}

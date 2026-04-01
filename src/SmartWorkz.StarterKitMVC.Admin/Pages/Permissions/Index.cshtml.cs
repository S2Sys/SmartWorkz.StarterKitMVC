using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SmartWorkz.StarterKitMVC.Admin.Pages.Permissions
{
    [Authorize(Policy = "RequireAdmin")]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SmartWorkz.StarterKitMVC.Admin.Pages.Account
{
    public class AccessDeniedModel : PageModel
    {
        public string? ReturnUrl { get; set; }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }
    }
}

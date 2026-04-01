using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace SmartWorkz.StarterKitMVC.Admin.Pages.Account
{
    public class ProfileModel : PageModel
    {
        public bool IsAuthenticated { get; private set; }
        public string UserId { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string DisplayName { get; private set; } = string.Empty;
        public List<string> Roles { get; private set; } = new();
        public List<string> Permissions { get; private set; } = new();
        public List<(string Type, string Value)> AllClaims { get; private set; } = new();
        public string AuthScheme { get; private set; } = string.Empty;

        public void OnGet()
        {
            var user = HttpContext.User;
            IsAuthenticated = user.Identity?.IsAuthenticated ?? false;

            if (!IsAuthenticated) return;

            AuthScheme  = user.Identity?.AuthenticationType ?? "-";
            UserId      = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "-";
            Email       = user.FindFirstValue(ClaimTypes.Email) ?? "-";
            DisplayName = user.FindFirstValue(ClaimTypes.Name) ?? "-";

            Roles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            Permissions = user.Claims
                .Where(c => c.Type == "permission")
                .Select(c => c.Value)
                .ToList();

            AllClaims = user.Claims
                .Select(c => (ShortType(c.Type), c.Value))
                .ToList();
        }

        private static string ShortType(string claimType) => claimType switch
        {
            ClaimTypes.NameIdentifier => "nameidentifier",
            ClaimTypes.Email          => "email",
            ClaimTypes.Name           => "name",
            ClaimTypes.Role           => "role",
            _                         => claimType.Contains('/') ? claimType.Split('/').Last() : claimType
        };
    }
}

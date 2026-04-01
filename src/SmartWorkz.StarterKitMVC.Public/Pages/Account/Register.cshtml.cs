using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Services;
using SmartWorkz.StarterKitMVC.Shared.DTOs;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace SmartWorkz.StarterKitMVC.Public.Pages.Account;

[AllowAnonymous]
public class RegisterModel : BasePage
{
    private readonly IAuthService           _authService;
    private readonly ILogger<RegisterModel> _logger;

    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required][Display(Name = "Display Name")][StringLength(100, MinimumLength = 2)]
        public string DisplayName { get; set; } = string.Empty;

        [Required][StringLength(50, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers and underscores.")]
        public string Username { get; set; } = string.Empty;

        [Required][EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required][DataType(DataType.Password)][StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required][DataType(DataType.Password)][Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public RegisterModel(IAuthService authService, ILogger<RegisterModel> logger)
    {
        _authService = authService;
        _logger      = logger;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var result = await _authService.RegisterAsync(new RegisterRequest(
            Input.Email, Input.Username, Input.Password, Input.DisplayName, TenantId));

        if (!result.Succeeded)
        {
            AddErrors(result);
            return Page();
        }

        await SignInAsync(result.Data!.User);
        _logger.LogInformation("New user registered: {Email}", Input.Email);
        return RedirectToPage("/Index");
    }

    private async Task SignInAsync(UserProfileDto user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId),
            new(ClaimTypes.Email,          user.Email),
            new(ClaimTypes.Name,           user.DisplayName ?? user.Username ?? ""),
        };
        foreach (var role in user.Roles ?? [])
            claims.Add(new(ClaimTypes.Role, role));
        foreach (var perm in user.Permissions ?? [])
            claims.Add(new("permission", perm));

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = false, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8) });
    }
}

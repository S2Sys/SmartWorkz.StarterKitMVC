using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Services;
using SmartWorkz.StarterKitMVC.Shared.Constants;
using SmartWorkz.StarterKitMVC.Shared.DTOs;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace SmartWorkz.StarterKitMVC.Public.Pages.Account;

[AllowAnonymous]
public class LoginModel : BasePage
{
    private readonly IAuthService         _authService;
    private readonly ILogger<LoginModel>  _logger;

    [BindProperty] public InputModel Input      { get; set; } = new();
    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required][EmailAddress]
        public string Email    { get; set; } = string.Empty;

        [Required][DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public LoginModel(IAuthService authService, ILogger<LoginModel> logger)
    {
        _authService = authService;
        _logger      = logger;
    }

    public void OnGet(string? returnUrl = null) => ReturnUrl = returnUrl;

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;

        if (!ModelState.IsValid) return Page();

        var result = await _authService.LoginAsync(
            new LoginRequest(Input.Email, Input.Password, TenantId));

        if (!result.Succeeded)
        {
            AddErrors(result);
            _logger.LogWarning("Failed login for {Email}", Input.Email);
            return Page();
        }

        await SignInAsync(result.Data!.User, Input.RememberMe, TimeSpan.FromHours(8));
        _logger.LogInformation("User {Email} logged in", Input.Email);
        return LocalRedirect(returnUrl ?? Url.Content("~/"));
    }

    private async Task SignInAsync(UserProfileDto user, bool isPersistent, TimeSpan expiry)
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
            new AuthenticationProperties { IsPersistent = isPersistent, ExpiresUtc = DateTimeOffset.UtcNow.Add(expiry) });
    }
}

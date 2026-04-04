using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Services;
using SmartWorkz.StarterKitMVC.Shared.Constants;
using SmartWorkz.StarterKitMVC.Shared.DTOs;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace SmartWorkz.StarterKitMVC.Admin.Pages.Account;

[AllowAnonymous]
public class LoginModel : BasePage
{
    private readonly IAuthService        _authService;
    private readonly ILogger<LoginModel> _logger;

    [BindProperty] public InputModel Input { get; set; } = new();
    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required][EmailAddress] public string Email    { get; set; } = string.Empty;
        [Required][DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
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
            _logger.LogWarning("Failed admin login for {Email}", Input.Email);
            return Page();
        }

        var user = result.Data!.User;

        // Admin portal requires admin role
        if (!user.Roles.Contains("admin", StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(string.Empty, T(MessageKeys.Auth.AccessDenied));
            _logger.LogWarning("Non-admin login attempt for {Email}", Input.Email);
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId),
            new(ClaimTypes.Email,          user.Email),
            new(ClaimTypes.Name,           user.DisplayName ?? user.Username ?? ""),
            new("TenantId",                user.TenantId),
        };

        // Add roles (both ClaimTypes.Role and lowercase for compatibility)
        if (user.Roles != null)
        {
            foreach (var role in user.Roles)
            {
                claims.Add(new(ClaimTypes.Role, role));
                claims.Add(new("role", role)); // Add lowercase for compatibility
            }
        }

        // Add permissions
        if (user.Permissions != null)
        {
            foreach (var perm in user.Permissions)
                claims.Add(new("permission", perm));
        }

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        try
        {
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties { IsPersistent = false, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(4) });

            _logger.LogInformation("Admin {Email} logged in successfully. UserId={UserId}, TenantId={TenantId}, Roles={Roles}",
                Input.Email, user.UserId, user.TenantId, string.Join(",", user.Roles ?? []));

            var redirectUrl = returnUrl ?? "/Dashboard";
            _logger.LogInformation("Redirecting to {Url}. Authentication complete", redirectUrl);

            return LocalRedirect(redirectUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sign-in for {Email}", Input.Email);
            ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
            return Page();
        }
    }
}

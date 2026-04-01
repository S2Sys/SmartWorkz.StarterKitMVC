using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Services;
using SmartWorkz.StarterKitMVC.Shared.DTOs;
using System.ComponentModel.DataAnnotations;

namespace SmartWorkz.StarterKitMVC.Public.Pages.Account;

[AllowAnonymous]
public class ForgotPasswordModel : BasePage
{
    private readonly IAuthService                _authService;
    private readonly ILogger<ForgotPasswordModel> _logger;

    [BindProperty] public InputModel Input     { get; set; } = new();
    public bool EmailSent { get; set; }

    public class InputModel
    {
        [Required][EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public ForgotPasswordModel(IAuthService authService, ILogger<ForgotPasswordModel> logger)
    {
        _authService = authService;
        _logger      = logger;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        // Result always succeeds — prevents email enumeration
        await _authService.ForgotPasswordAsync(new ForgotPasswordRequest(Input.Email, TenantId));
        _logger.LogInformation("Password reset requested for {Email}", Input.Email);

        EmailSent = true;
        return Page();
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Application.Services;
using SmartWorkz.StarterKitMVC.Shared.DTOs;
using System.ComponentModel.DataAnnotations;

namespace SmartWorkz.StarterKitMVC.Public.Pages.Account;

[AllowAnonymous]
public class ResetPasswordModel : BasePage
{
    private readonly IAuthService                _authService;
    private readonly ILogger<ResetPasswordModel> _logger;

    [BindProperty] public InputModel Input { get; set; } = new();
    public bool ResetSucceeded { get; set; }

    public class InputModel
    {
        [Required] public string Token { get; set; } = string.Empty;
        [Required][EmailAddress] public string Email { get; set; } = string.Empty;

        [Required][DataType(DataType.Password)][Display(Name = "New Password")]
        [StringLength(100, MinimumLength = 8)]
        public string NewPassword { get; set; } = string.Empty;

        [Required][DataType(DataType.Password)][Display(Name = "Confirm Password")]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public ResetPasswordModel(IAuthService authService, ILogger<ResetPasswordModel> logger)
    {
        _authService = authService;
        _logger      = logger;
    }

    public IActionResult OnGet(string? token, string? email)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            return RedirectToPage("/Account/ForgotPassword");

        Input.Token = token;
        Input.Email = email;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var result = await _authService.ResetPasswordAsync(new ResetPasswordRequest(
            Input.Token, Input.Email, Input.NewPassword, TenantId));

        if (!result.Succeeded)
        {
            AddErrors(result);
            return Page();
        }

        _logger.LogInformation("Password reset succeeded for {Email}", Input.Email);
        ResetSucceeded = true;
        return Page();
    }
}

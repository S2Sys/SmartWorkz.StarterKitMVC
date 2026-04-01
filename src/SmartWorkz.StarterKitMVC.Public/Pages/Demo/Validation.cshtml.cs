using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Shared.Constants;

namespace SmartWorkz.StarterKitMVC.Public.Pages.Demo;

/// <summary>
/// Demo page showcasing the simple validation approach using MessageKey error messages.
/// Demonstrates required, email, string length, format, and custom validation.
/// </summary>
[AllowAnonymous]
public class ValidationModel : BasePage
{
    [BindProperty]
    public FormInput Input { get; set; } = new();

    public void OnGet()
    {
        // Nothing to initialize - form loads empty
    }

    public IActionResult OnPost()
    {
        // ModelState validation automatically runs
        // Validation attributes use MessageKey error messages that get translated via T()
        if (!ModelState.IsValid)
        {
            // Re-render the form with error messages
            return Page();
        }

        // Form is valid
        ModelState.Clear();
        Input = new();
        TempData["SuccessMessage"] = T(MessageKeys.Validation.Required); // Just for demo
        return Page();
    }

    public class FormInput
    {
        [Display(Name = "Full Name")]
        [Required(ErrorMessage = MessageKeys.Validation.Required)]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = MessageKeys.Validation.MaxLength)]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Email Address")]
        [Required(ErrorMessage = MessageKeys.Validation.Required)]
        [EmailAddress(ErrorMessage = MessageKeys.Validation.EmailInvalid)]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Age")]
        [Range(18, 120, ErrorMessage = MessageKeys.Validation.InvalidFormat)]
        public int? Age { get; set; }

        [Display(Name = "Website URL")]
        [RegularExpression(
            @"^(https?://)?([\da-z\.-]+)\.([a-z\.]{2,6})([/\w \.-]*)*/?$",
            ErrorMessage = MessageKeys.Validation.InvalidFormat)]
        public string? WebsiteUrl { get; set; }

        [Display(Name = "Password")]
        [Required(ErrorMessage = MessageKeys.Validation.Required)]
        [StringLength(100, MinimumLength = 8,
            ErrorMessage = MessageKeys.Validation.MaxLength)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Confirm Password")]
        [Required(ErrorMessage = MessageKeys.Validation.Required)]
        [Compare("Password", ErrorMessage = MessageKeys.Validation.InvalidFormat)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}

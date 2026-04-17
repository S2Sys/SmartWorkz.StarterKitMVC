using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartWorkz.StarterKitMVC.Public.Controllers;

/// <summary>
/// User profile controller
/// </summary>
[Authorize]
[Route("[controller]")]
public class ProfileController : Controller
{
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(ILogger<ProfileController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// View user profile
    /// </summary>
    [HttpGet("")]
    [HttpGet("Index")]
    public IActionResult Index()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("Profile view accessed by user: {UserId}", userId);

        var model = new ProfileViewModel
        {
            Email = "john.doe@example.com",
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+1 (555) 123-4567",
            DateOfBirth = new DateTime(1990, 5, 15),
            JoinedDate = DateTime.UtcNow.AddYears(-2),
            Bio = "Software developer and technology enthusiast",
            Avatar = "/images/avatars/john.jpg",
            IsEmailVerified = true,
            IsTwoFactorEnabled = false
        };

        return View(model);
    }

    /// <summary>
    /// Edit profile form
    /// </summary>
    [HttpGet("Edit")]
    public IActionResult Edit()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("Profile edit accessed by user: {UserId}", userId);

        var model = new EditProfileViewModel
        {
            FirstName = "John",
            LastName = "Doe",
            PhoneNumber = "+1 (555) 123-4567",
            DateOfBirth = new DateTime(1990, 5, 15),
            Bio = "Software developer and technology enthusiast"
        };

        return View(model);
    }

    /// <summary>
    /// Save profile changes
    /// </summary>
    [HttpPost("Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("Profile updated for user: {UserId}", userId);

        TempData["SuccessMessage"] = "Profile updated successfully";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Change password form
    /// </summary>
    [HttpGet("ChangePassword")]
    public IActionResult ChangePassword()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("Change password accessed by user: {UserId}", userId);

        return View();
    }

    /// <summary>
    /// Process password change
    /// </summary>
    [HttpPost("ChangePassword")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("Password changed for user: {UserId}", userId);

        TempData["SuccessMessage"] = "Password changed successfully";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Two-factor authentication setup form
    /// </summary>
    [HttpGet("Enable2FA")]
    public IActionResult Enable2FA()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("2FA setup accessed by user: {UserId}", userId);

        var model = new Enable2FAViewModel
        {
            QrCodeUrl = "https://api.qrserver.com/v1/create-qr-code/?size=300x300&data=example",
            ManualEntryKey = "JBSWY3DPEBLW64TMMQ======",
            BackupCodes = new[] { "ABC123", "DEF456", "GHI789", "JKL012", "MNO345" }
        };

        return View(model);
    }

    /// <summary>
    /// Enable two-factor authentication
    /// </summary>
    [HttpPost("Enable2FA")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enable2FA(Enable2FAViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("2FA enabled for user: {UserId}", userId);

        TempData["SuccessMessage"] = "Two-factor authentication has been enabled";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Disable two-factor authentication
    /// </summary>
    [HttpPost("Disable2FA")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Disable2FA()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("2FA disabled for user: {UserId}", userId);

        TempData["SuccessMessage"] = "Two-factor authentication has been disabled";
        return RedirectToAction(nameof(Index));
    }
}

public class ProfileViewModel
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime JoinedDate { get; set; }
    public string Bio { get; set; }
    public string Avatar { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
}

public class EditProfileViewModel
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Bio { get; set; }
    public string Avatar { get; set; }
}

public class ChangePasswordViewModel
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmPassword { get; set; }
}

public class Enable2FAViewModel
{
    public string QrCodeUrl { get; set; }
    public string ManualEntryKey { get; set; }
    public string[] BackupCodes { get; set; }
    public string VerificationCode { get; set; }
}

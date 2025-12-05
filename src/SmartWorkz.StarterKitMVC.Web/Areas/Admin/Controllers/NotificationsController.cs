using Microsoft.AspNetCore.Mvc;
using SmartWorkz.StarterKitMVC.Web.Areas.Admin.Models;

namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class NotificationsController : Controller
{
    public IActionResult Index(string? type = null, string? channel = null, bool? isRead = null, int page = 1)
    {
        ViewData["Title"] = "Notifications";
        var model = new NotificationsPageViewModel
        {
            Notifications = GetSampleNotifications(),
            TotalCount = 50,
            UnreadCount = 12,
            Page = page,
            PageSize = 20,
            FilterType = type,
            FilterChannel = channel,
            FilterIsRead = isRead
        };
        return View(model);
    }

    public IActionResult Create()
    {
        ViewData["Title"] = "Send Notification";
        var model = new NotificationFormViewModel
        {
            AvailableTemplates = GetSampleTemplateSelect(),
            AvailableUsers = GetSampleUserSelect()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(NotificationFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableTemplates = GetSampleTemplateSelect();
            model.AvailableUsers = GetSampleUserSelect();
            return View(model);
        }

        // TODO: Send notification via service
        TempData["Success"] = "Notification sent successfully.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Details(Guid id)
    {
        ViewData["Title"] = "Notification Details";
        var notification = new NotificationListViewModel
        {
            Id = id,
            Title = "Welcome to the system",
            Message = "Your account has been created successfully.",
            Type = "Info",
            Channel = "InApp",
            IsRead = false,
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            UserName = "john.doe"
        };
        return View(notification);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MarkAsRead(Guid id)
    {
        // TODO: Mark as read via service
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MarkAllAsRead()
    {
        // TODO: Mark all as read via service
        TempData["Success"] = "All notifications marked as read.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(Guid id)
    {
        // TODO: Delete notification via service
        TempData["Success"] = "Notification deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    #region Templates

    public IActionResult Templates()
    {
        ViewData["Title"] = "Notification Templates";
        var model = new NotificationTemplatesPageViewModel
        {
            Templates = GetSampleTemplates(),
            TotalCount = 4
        };
        return View(model);
    }

    public IActionResult TemplateCreate()
    {
        ViewData["Title"] = "Create Template";
        return View(new NotificationTemplateFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult TemplateCreate(NotificationTemplateFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // TODO: Save template via service
        TempData["Success"] = "Template created successfully.";
        return RedirectToAction(nameof(Templates));
    }

    public IActionResult TemplateEdit(Guid id)
    {
        ViewData["Title"] = "Edit Template";
        // TODO: Get template from service
        var model = new NotificationTemplateFormViewModel
        {
            Id = id,
            Key = "welcome",
            Name = "Welcome Email",
            Description = "Sent to new users after registration",
            Subject = "Welcome to {{AppName}}!",
            Body = "Hello {{UserName}},\n\nWelcome to {{AppName}}!",
            BodyHtml = "<h1>Welcome to {{AppName}}!</h1><p>Hello {{UserName}},</p>",
            Channel = "Email",
            IsActive = true
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult TemplateEdit(Guid id, NotificationTemplateFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // TODO: Update template via service
        TempData["Success"] = "Template updated successfully.";
        return RedirectToAction(nameof(Templates));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult TemplateDelete(Guid id)
    {
        // TODO: Delete template via service
        TempData["Success"] = "Template deleted successfully.";
        return RedirectToAction(nameof(Templates));
    }

    public IActionResult TemplatePreview(Guid id)
    {
        // TODO: Get template and render preview
        var html = "<h1>Welcome to SmartWorkz StarterKitMVC!</h1><p>Hello John Doe,</p><p>Your account has been created successfully.</p>";
        return Content(html, "text/html");
    }

    #endregion

    #region Sample Data

    private static List<NotificationListViewModel> GetSampleNotifications() =>
    [
        new() { Id = Guid.NewGuid(), Title = "Welcome to the system", Message = "Your account has been created successfully.", Type = "Info", Channel = "InApp", IsRead = false, CreatedAt = DateTime.UtcNow.AddHours(-2), UserName = "john.doe" },
        new() { Id = Guid.NewGuid(), Title = "Password changed", Message = "Your password has been changed successfully.", Type = "Success", Channel = "Email", IsRead = true, ReadAt = DateTime.UtcNow.AddHours(-1), IsSent = true, SentAt = DateTime.UtcNow.AddHours(-3), CreatedAt = DateTime.UtcNow.AddHours(-3), UserName = "jane.smith" },
        new() { Id = Guid.NewGuid(), Title = "New login detected", Message = "A new login was detected from Chrome on Windows.", Type = "Warning", Channel = "Email", IsRead = false, IsSent = true, SentAt = DateTime.UtcNow.AddDays(-1), CreatedAt = DateTime.UtcNow.AddDays(-1), UserName = "admin" },
        new() { Id = Guid.NewGuid(), Title = "Account locked", Message = "Your account has been locked due to multiple failed login attempts.", Type = "Error", Channel = "Email", IsRead = true, ReadAt = DateTime.UtcNow.AddDays(-2), IsSent = true, SentAt = DateTime.UtcNow.AddDays(-3), CreatedAt = DateTime.UtcNow.AddDays(-3), UserName = "bob.wilson" },
    ];

    private static List<NotificationTemplateListViewModel> GetSampleTemplates() =>
    [
        new() { Id = Guid.NewGuid(), Key = "welcome", Name = "Welcome Email", Description = "Sent to new users after registration", Subject = "Welcome to {{AppName}}!", Channel = "Email", IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-90) },
        new() { Id = Guid.NewGuid(), Key = "password_reset", Name = "Password Reset", Description = "Sent when user requests password reset", Subject = "Reset Your Password - {{AppName}}", Channel = "Email", IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-90) },
        new() { Id = Guid.NewGuid(), Key = "account_locked", Name = "Account Locked", Description = "Sent when account is locked", Subject = "Account Locked - {{AppName}}", Channel = "Email", IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-90) },
        new() { Id = Guid.NewGuid(), Key = "new_login", Name = "New Login Alert", Description = "Sent when user logs in from new device", Subject = "New Login Detected - {{AppName}}", Channel = "Email", IsActive = true, CreatedAt = DateTime.UtcNow.AddDays(-90) },
    ];

    private static List<NotificationTemplateSelectItem> GetSampleTemplateSelect() =>
    [
        new() { Id = Guid.NewGuid(), Key = "welcome", Name = "Welcome Email" },
        new() { Id = Guid.NewGuid(), Key = "password_reset", Name = "Password Reset" },
        new() { Id = Guid.NewGuid(), Key = "account_locked", Name = "Account Locked" },
        new() { Id = Guid.NewGuid(), Key = "new_login", Name = "New Login Alert" },
    ];

    private static List<UserSelectItem> GetSampleUserSelect() =>
    [
        new() { Id = Guid.NewGuid(), UserName = "admin", DisplayName = "Administrator", Email = "admin@example.com" },
        new() { Id = Guid.NewGuid(), UserName = "john.doe", DisplayName = "John Doe", Email = "john@example.com" },
        new() { Id = Guid.NewGuid(), UserName = "jane.smith", DisplayName = "Jane Smith", Email = "jane@example.com" },
    ];

    #endregion
}

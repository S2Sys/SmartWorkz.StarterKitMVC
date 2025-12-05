using System.ComponentModel.DataAnnotations;

namespace SmartWorkz.StarterKitMVC.Web.Areas.Admin.Models;

/// <summary>
/// View model for notification list
/// </summary>
public class NotificationListViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "Info";
    public string Channel { get; set; } = "InApp";
    public string? ActionUrl { get; set; }
    public string? Icon { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? UserName { get; set; }
}

/// <summary>
/// View model for creating notification
/// </summary>
public class NotificationFormViewModel
{
    public Guid? Id { get; set; }

    public Guid? UserId { get; set; }

    // For sending to multiple users
    public List<Guid> UserIds { get; set; } = [];

    // Or send to all users in tenant
    public bool SendToAllUsers { get; set; }

    [StringLength(128)]
    public string? TenantId { get; set; }

    public Guid? TemplateId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(256, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required")]
    public string Message { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = "Info"; // Info, Success, Warning, Error

    [Required]
    public string Channel { get; set; } = "InApp"; // Email, SMS, Push, InApp

    [StringLength(500)]
    [Url(ErrorMessage = "Invalid URL format")]
    public string? ActionUrl { get; set; }

    [StringLength(100)]
    public string? ActionText { get; set; }

    [StringLength(50)]
    public string? Icon { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public string? Metadata { get; set; } // JSON

    // For template selection
    public List<NotificationTemplateSelectItem> AvailableTemplates { get; set; } = [];
    public List<UserSelectItem> AvailableUsers { get; set; } = [];
}

public class NotificationTemplateSelectItem
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class UserSelectItem
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// View model for notification template list
/// </summary>
public class NotificationTemplateListViewModel
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Subject { get; set; }
    public string Channel { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// View model for creating/editing notification template
/// </summary>
public class NotificationTemplateFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Key is required")]
    [StringLength(128, MinimumLength = 2)]
    [RegularExpression(@"^[a-z0-9_]+$", ErrorMessage = "Key can only contain lowercase letters, numbers, and underscores")]
    public string Key { get; set; } = string.Empty;

    [Required(ErrorMessage = "Name is required")]
    [StringLength(256, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(500)]
    public string? Subject { get; set; }

    [Required(ErrorMessage = "Body is required")]
    public string Body { get; set; } = string.Empty;

    public string? BodyHtml { get; set; }

    [Required]
    public string Channel { get; set; } = "Email"; // Email, SMS, Push, InApp

    public bool IsActive { get; set; } = true;

    // Available placeholders for the template
    public List<string> AvailablePlaceholders { get; set; } = [
        "{{AppName}}",
        "{{UserName}}",
        "{{Email}}",
        "{{DisplayName}}",
        "{{TenantName}}",
        "{{CurrentDate}}",
        "{{CurrentTime}}"
    ];
}

/// <summary>
/// View model for notifications page
/// </summary>
public class NotificationsPageViewModel
{
    public List<NotificationListViewModel> Notifications { get; set; } = [];
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? FilterType { get; set; }
    public string? FilterChannel { get; set; }
    public bool? FilterIsRead { get; set; }
}

/// <summary>
/// View model for notification templates page
/// </summary>
public class NotificationTemplatesPageViewModel
{
    public List<NotificationTemplateListViewModel> Templates { get; set; } = [];
    public int TotalCount { get; set; }
}

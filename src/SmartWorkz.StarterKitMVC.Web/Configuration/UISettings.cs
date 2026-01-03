namespace SmartWorkz.StarterKitMVC.Web.Configuration;

/// <summary>
/// UI configuration settings
/// </summary>
public class UISettings
{
    public const string SectionName = "UI";
    
    public AdminUISettings Admin { get; set; } = new();
}

/// <summary>
/// Admin panel UI settings
/// </summary>
public class AdminUISettings
{
    /// <summary>
    /// Form display mode: "modal" or "offcanvas"
    /// </summary>
    public string FormDisplayMode { get; set; } = "offcanvas";
    
    /// <summary>
    /// Default page size for tables
    /// </summary>
    public int TablePageSize { get; set; } = 20;
    
    /// <summary>
    /// Show breadcrumbs in admin pages
    /// </summary>
    public bool ShowBreadcrumbs { get; set; } = true;
    
    /// <summary>
    /// Start with sidebar collapsed
    /// </summary>
    public bool SidebarCollapsed { get; set; } = false;
}

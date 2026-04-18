namespace SmartWorkz.Core.Web.Services.Components;

/// <summary>
/// Provides form component styling configuration management.
/// Manages Bootstrap CSS classes used throughout the form system.
/// Allows customization of default Bootstrap styling.
/// </summary>
public class FormComponentProvider : IFormComponentProvider
{
    private FormComponentConfig _config = new();

    /// <summary>
    /// Get current form component configuration
    /// </summary>
    /// <returns>The current FormComponentConfig instance containing all CSS class configurations.</returns>
    public FormComponentConfig GetConfiguration()
    {
        return _config;
    }

    /// <summary>
    /// Update configuration with new values
    /// </summary>
    /// <param name="config">The new FormComponentConfig to apply. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when config is null.</exception>
    public void UpdateConfiguration(FormComponentConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }
}

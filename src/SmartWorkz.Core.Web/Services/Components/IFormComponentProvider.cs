namespace SmartWorkz.Core.Web.Services.Components;

/// <summary>
/// Provides access to form component styling configuration.
/// Allows retrieval and updating of Bootstrap CSS classes used throughout the form system.
/// </summary>
public interface IFormComponentProvider
{
    /// <summary>
    /// Get current form component configuration
    /// </summary>
    /// <returns>The current FormComponentConfig instance containing all CSS class configurations.</returns>
    FormComponentConfig GetConfiguration();

    /// <summary>
    /// Update configuration
    /// </summary>
    /// <param name="config">The new FormComponentConfig to apply. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when config is null.</exception>
    void UpdateConfiguration(FormComponentConfig config);
}

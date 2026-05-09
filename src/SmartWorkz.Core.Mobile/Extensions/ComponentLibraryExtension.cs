namespace SmartWorkz.Mobile.Extensions;

using Microsoft.Maui.Controls.Hosting;
using SmartWorkz.Mobile.Components;

/// <summary>
/// Extension methods for configuring SmartWorkz component library in MAUI applications.
/// </summary>
public static class ComponentLibraryExtension
{
    /// <summary>
    /// Adds the SmartWorkz component library to the MAUI app builder.
    ///
    /// Registers all custom components and configures fonts and resource dictionaries.
    /// Call this method during app initialization in MauiProgram.CreateMauiApp().
    ///
    /// Example:
    /// <code>
    /// var builder = MauiApp.CreateBuilder()
    ///     .UseMauiApp&lt;App&gt;()
    ///     .AddSmartWorkzComponentLibrary();
    /// </code>
    /// </summary>
    /// <param name="builder">The MAUI app builder.</param>
    /// <returns>The MAUI app builder for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null.</exception>
    public static MauiAppBuilder AddSmartWorkzComponentLibrary(this MauiAppBuilder builder)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        return builder
            .ConfigureFonts(fonts =>
            {
                // Register component library fonts
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .RegisterComponentLibraryResources();
    }

    /// <summary>
    /// Registers component library resources and styles.
    /// </summary>
    private static MauiAppBuilder RegisterComponentLibraryResources(this MauiAppBuilder builder)
    {
        // Additional resource registration can be added here if needed
        // This is a placeholder for future resource dictionary and style registration
        return builder;
    }
}

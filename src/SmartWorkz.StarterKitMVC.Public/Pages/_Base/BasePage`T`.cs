namespace SmartWorkz.StarterKitMVC.Public.Pages;

/// <summary>
/// Generic variant of Public.BasePage. Adds a typed Model property for detail/form pages.
/// T is the data model type used on the page (e.g., UserDto, ProductDto).
/// </summary>
public abstract class BasePage<T> : BasePage where T : class
{
    public T? Model { get; protected set; }
}

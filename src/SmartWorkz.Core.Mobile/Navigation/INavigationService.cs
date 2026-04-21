namespace SmartWorkz.Mobile;

public interface INavigationService
{
    Task NavigateToAsync(string route, NavigationParameters? parameters = null, CancellationToken ct = default);
    Task GoBackAsync(CancellationToken ct = default);
    Task GoBackToRootAsync(CancellationToken ct = default);
    string GetCurrentRoute();
}

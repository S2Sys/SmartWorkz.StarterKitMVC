namespace SmartWorkz.ECommerce.Mobile.Services;

using SmartWorkz.Mobile;

public sealed class NavigationService : INavigationService
{
    public async Task NavigateToAsync(string route, NavigationParameters? parameters = null, CancellationToken ct = default)
    {
        var qs = parameters?.ToQueryString() ?? string.Empty;
        await Shell.Current.GoToAsync($"{route}{qs}", animate: true);
    }

    public async Task GoBackAsync(CancellationToken ct = default) =>
        await Shell.Current.GoToAsync("..", animate: true);

    public async Task GoBackToRootAsync(CancellationToken ct = default) =>
        await Shell.Current.GoToAsync("//home", animate: true);

    public string GetCurrentRoute() =>
        Shell.Current.CurrentState?.Location.OriginalString ?? string.Empty;
}

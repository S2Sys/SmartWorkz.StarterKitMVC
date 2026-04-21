namespace SmartWorkz.ECommerce.Mobile.Services;

using SmartWorkz.Mobile;

public sealed class NavigationService : INavigationService
{
    public async Task NavigateToAsync(string route, NavigationParameters? parameters = null, CancellationToken ct = default)
    {
        var qs = parameters?.ToQueryString() ?? string.Empty;
        await Shell.Current.GoToAsync($"{route}{qs}");
    }

    public async Task GoBackAsync(CancellationToken ct = default) =>
        await Shell.Current.GoToAsync("..");

    public async Task GoBackToRootAsync(CancellationToken ct = default) =>
        await Shell.Current.GoToAsync("//home");

    public string GetCurrentRoute() =>
        Shell.Current.CurrentState.Location.OriginalString;
}

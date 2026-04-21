namespace SmartWorkz.ECommerce.Mobile.Pages;

public partial class ProductDetailPage : ContentPage
{
    private readonly ProductDetailViewModel _vm;

    public ProductDetailPage(ProductDetailViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Extract productId from navigation parameters
        if (Shell.Current.CurrentState is ShellNavigationState state &&
            state.Location.OriginalString.Contains("productId=") &&
            int.TryParse(ExtractQueryParam(state.Location.OriginalString, "productId"), out var productId))
        {
            _vm.ProductId = productId;
        }
    }

    private static string? ExtractQueryParam(string url, string paramName)
    {
        var query = url.Substring(url.IndexOf('?') + 1);
        var pairs = query.Split('&');
        foreach (var pair in pairs)
        {
            var keyValue = pair.Split('=');
            if (keyValue[0] == paramName && keyValue.Length > 1)
                return Uri.UnescapeDataString(keyValue[1]);
        }
        return null;
    }
}

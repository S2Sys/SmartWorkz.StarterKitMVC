namespace SmartWorkz.ECommerce.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("register", typeof(Pages.RegisterPage));
        Routing.RegisterRoute("product-detail", typeof(Pages.ProductDetailPage));
        Routing.RegisterRoute("checkout", typeof(Pages.CheckoutPage));
    }
}

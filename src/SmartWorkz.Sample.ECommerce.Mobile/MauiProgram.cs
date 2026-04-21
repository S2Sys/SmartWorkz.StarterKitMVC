namespace SmartWorkz.ECommerce.Mobile;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Extensions.Logging;
using SmartWorkz.ECommerce.Mobile.Services;
using SmartWorkz.ECommerce.Mobile.Repositories;
using SmartWorkz.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        // NOTE: UseMaui() extension will be called in platform-specific code
        // builder.UseMaui();

        builder.ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf",   "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf",  "OpenSansSemibold");
        });

        // Register SmartWorkz.Core.Mobile services
        builder.Services.AddSmartWorkzCoreMobile(cfg =>
        {
            cfg.BaseUrl     = "https://localhost:7000"; // override in production via appsettings
            cfg.RetryCount  = 3;
        });

        // Register app-specific services
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<ProductRepository>();
        builder.Services.AddSingleton<OrderRepository>();

        // Register ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<ProductDetailViewModel>();
        builder.Services.AddSingleton<CartViewModel>();
        builder.Services.AddTransient<CheckoutViewModel>();
        builder.Services.AddTransient<OrdersViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();

        // Register Pages
        builder.Services.AddTransient<Pages.LoginPage>();
        builder.Services.AddTransient<Pages.RegisterPage>();
        builder.Services.AddTransient<Pages.HomePage>();
        builder.Services.AddTransient<Pages.ProductDetailPage>();
        builder.Services.AddTransient<Pages.CartPage>();
        builder.Services.AddTransient<Pages.CheckoutPage>();
        builder.Services.AddTransient<Pages.OrdersPage>();
        builder.Services.AddTransient<Pages.ProfilePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

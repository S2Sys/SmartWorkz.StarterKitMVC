namespace SmartWorkz.ECommerce.Mobile;

using Android.App;
using Android.Content.PM;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
          ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode)]
public class MainActivity : MauiAppCompatActivity { }

namespace SmartWorkz.ECommerce.Mobile;

class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            var mauiApp = MauiProgram.CreateMauiApp();
            // Platform-specific startup - to be implemented per platform
            // mauiApp.Run();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
        }
    }
}

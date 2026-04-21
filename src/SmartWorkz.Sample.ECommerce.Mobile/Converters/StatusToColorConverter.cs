namespace SmartWorkz.ECommerce.Mobile.Converters;

using Microsoft.Maui.Controls;

public sealed class StatusToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        return value?.ToString()?.ToLowerInvariant() switch
        {
            "pending"    => Colors.Orange,
            "confirmed"  => Colors.Blue,
            "shipped"    => Colors.DodgerBlue,
            "delivered"  => Colors.Green,
            "cancelled"  => Colors.Red,
            _            => Colors.Gray,
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) =>
        throw new NotImplementedException();
}

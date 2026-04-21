namespace SmartWorkz.ECommerce.Mobile.Converters;

using System.Globalization;
using SmartWorkz.Sample.ECommerce.Application.DTOs;

public sealed class CartLineTotalConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is CartItemDto item)
            return (item.UnitPrice * item.Quantity).ToString("C");
        return "0.00";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

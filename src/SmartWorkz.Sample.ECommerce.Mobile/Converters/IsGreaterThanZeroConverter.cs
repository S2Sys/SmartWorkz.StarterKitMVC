namespace SmartWorkz.ECommerce.Mobile.Converters;

using System.Globalization;

public sealed class IsGreaterThanZeroConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is int i ? i > 0 : value is decimal d ? d > 0 : false;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}

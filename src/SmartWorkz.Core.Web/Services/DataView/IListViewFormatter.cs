namespace SmartWorkz.Core.Web.Services.DataView;

public interface IListViewFormatter
{
    string FormatDate(DateTime? date, string format = "MMM dd, yyyy");
    string FormatCurrency(decimal? value, string currencySymbol = "$");
    string TruncateText(string? text, int maxLength = 100);
    string FormatBoolean(bool? value);
    string FormatValue(object? value);
}

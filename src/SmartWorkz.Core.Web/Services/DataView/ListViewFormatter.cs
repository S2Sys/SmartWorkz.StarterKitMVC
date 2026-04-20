namespace SmartWorkz.Core.Web.Services.DataView;

public class ListViewFormatter : IListViewFormatter
{
    public string FormatDate(DateTime? date, string format = "MMM dd, yyyy")
    {
        return date?.ToString(format) ?? "-";
    }

    public string FormatCurrency(decimal? value, string currencySymbol = "$")
    {
        if (value == null)
            return "-";

        return $"{currencySymbol}{value:N2}";
    }

    public string TruncateText(string? text, int maxLength = 100)
    {
        if (string.IsNullOrEmpty(text))
            return "-";

        if (text.Length <= maxLength)
            return text;

        return text[..maxLength] + "...";
    }

    public string FormatBoolean(bool? value)
    {
        return value switch
        {
            true => "Yes",
            false => "No",
            null => "-"
        };
    }

    public string FormatValue(object? value)
    {
        return value switch
        {
            null => "-",
            DateTime dateTime => FormatDate(dateTime),
            decimal decimalValue => FormatCurrency(decimalValue),
            bool boolValue => FormatBoolean(boolValue),
            string stringValue => TruncateText(stringValue),
            _ => value.ToString() ?? "-"
        };
    }
}

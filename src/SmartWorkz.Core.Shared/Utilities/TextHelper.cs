namespace SmartWorkz.Core.Shared.Utilities;

using System.Text;
using System.Text.RegularExpressions;
using SmartWorkz.Core.Shared.Results;

/// <summary>
/// Sealed class providing advanced text processing and formatting utilities.
/// All methods return Result&lt;string&gt; for consistent error handling.
/// </summary>
public sealed class TextHelper
{
    /// <summary>
    /// Truncates text to a maximum length and appends a suffix (default "...").
    /// </summary>
    /// <param name="text">The input text to truncate.</param>
    /// <param name="maxLength">The maximum length including the suffix.</param>
    /// <param name="suffix">The suffix to append when truncating. Defaults to "...".</param>
    /// <returns>A Result containing the truncated text or an error.</returns>
    public static Result<string> Truncate(string text, int maxLength, string suffix = "...")
    {
        // Validate inputs
        if (string.IsNullOrEmpty(text))
        {
            return Result.Fail<string>("Text.InputEmpty", "Input text cannot be null or empty");
        }

        if (maxLength <= 0)
        {
            return Result.Fail<string>("Text.InvalidLength", "Maximum length must be greater than 0");
        }

        if (suffix == null)
        {
            return Result.Fail<string>("Text.InvalidSuffix", "Suffix cannot be null");
        }

        try
        {
            // If text is shorter than max length, return as-is
            if (text.Length <= maxLength)
            {
                return Result.Ok(text);
            }

            // If suffix is longer than maxLength, return error
            if (suffix.Length >= maxLength)
            {
                return Result.Fail<string>("Text.SuffixTooLong", "Suffix length exceeds maximum length");
            }

            // Truncate and append suffix
            var truncated = text.Substring(0, maxLength - suffix.Length) + suffix;
            return Result.Ok(truncated);
        }
        catch (Exception ex)
        {
            return Result.Fail<string>("Text.TruncateFailed", $"Failed to truncate text: {ex.Message}");
        }
    }

    /// <summary>
    /// Capitalizes the first letter of the text while preserving the rest.
    /// </summary>
    /// <param name="text">The input text to capitalize.</param>
    /// <returns>A Result containing the capitalized text or an error.</returns>
    public static Result<string> Capitalize(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Result.Fail<string>("Text.InputEmpty", "Input text cannot be null or empty");
        }

        try
        {
            if (text.Length == 0)
            {
                return Result.Ok(text);
            }

            var capitalized = char.ToUpperInvariant(text[0]) + text.Substring(1);
            return Result.Ok(capitalized);
        }
        catch (Exception ex)
        {
            return Result.Fail<string>("Text.CapitalizeFailed", $"Failed to capitalize text: {ex.Message}");
        }
    }

    /// <summary>
    /// Decapitalizes the first letter of the text while preserving the rest.
    /// </summary>
    /// <param name="text">The input text to decapitalize.</param>
    /// <returns>A Result containing the decapitalized text or an error.</returns>
    public static Result<string> Decapitalize(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Result.Fail<string>("Text.InputEmpty", "Input text cannot be null or empty");
        }

        try
        {
            if (text.Length == 0)
            {
                return Result.Ok(text);
            }

            var decapitalized = char.ToLowerInvariant(text[0]) + text.Substring(1);
            return Result.Ok(decapitalized);
        }
        catch (Exception ex)
        {
            return Result.Fail<string>("Text.DecapitalizeFailed", $"Failed to decapitalize text: {ex.Message}");
        }
    }

    /// <summary>
    /// Removes HTML tags from the input string using regex.
    /// </summary>
    /// <param name="html">The HTML string to process.</param>
    /// <returns>A Result containing the plain text with HTML tags removed or an error.</returns>
    public static Result<string> StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html))
        {
            return Result.Fail<string>("Text.InputEmpty", "Input HTML cannot be null or empty");
        }

        try
        {
            // Remove HTML tags using regex
            var regex = new Regex(@"<[^>]*>");
            var plainText = regex.Replace(html, string.Empty);

            // Decode HTML entities
            plainText = System.Web.HttpUtility.HtmlDecode(plainText);

            // Remove extra whitespace
            plainText = System.Text.RegularExpressions.Regex.Replace(plainText, @"\s+", " ").Trim();

            return Result.Ok(plainText);
        }
        catch (Exception ex)
        {
            return Result.Fail<string>("Text.StripHtmlFailed", $"Failed to strip HTML: {ex.Message}");
        }
    }

    /// <summary>
    /// Pluralizes a word based on count using a simple heuristic.
    /// If count == 1, returns singular form. Otherwise appends 's'.
    /// </summary>
    /// <param name="singular">The singular form of the word.</param>
    /// <param name="count">The count to determine plural form.</param>
    /// <returns>A Result containing the appropriately pluralized word or an error.</returns>
    public static Result<string> Pluralize(string singular, int count)
    {
        if (string.IsNullOrEmpty(singular))
        {
            return Result.Fail<string>("Text.InputEmpty", "Singular form cannot be null or empty");
        }

        try
        {
            // If count is 1, return singular
            if (count == 1)
            {
                return Result.Ok(singular);
            }

            // Simple pluralization: add 's'
            var plural = singular + "s";
            return Result.Ok(plural);
        }
        catch (Exception ex)
        {
            return Result.Fail<string>("Text.PluralizeFailed", $"Failed to pluralize text: {ex.Message}");
        }
    }

    /// <summary>
    /// Converts text to title case by capitalizing the first letter of each word.
    /// </summary>
    /// <param name="text">The input text to convert.</param>
    /// <returns>A Result containing the title-cased text or an error.</returns>
    public static Result<string> TitleCase(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Result.Fail<string>("Text.InputEmpty", "Input text cannot be null or empty");
        }

        try
        {
            var words = text.Split(' ');
            var titleCased = new StringBuilder();

            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    titleCased.Append(char.ToUpperInvariant(words[i][0]));
                    if (words[i].Length > 1)
                    {
                        titleCased.Append(words[i].Substring(1));
                    }
                }

                if (i < words.Length - 1)
                {
                    titleCased.Append(' ');
                }
            }

            return Result.Ok(titleCased.ToString());
        }
        catch (Exception ex)
        {
            return Result.Fail<string>("Text.TitleCaseFailed", $"Failed to convert to title case: {ex.Message}");
        }
    }

    /// <summary>
    /// Reverses the input string.
    /// </summary>
    /// <param name="text">The input text to reverse.</param>
    /// <returns>A Result containing the reversed text or an error.</returns>
    public static Result<string> Reverse(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Result.Fail<string>("Text.InputEmpty", "Input text cannot be null or empty");
        }

        try
        {
            var chars = text.ToCharArray();
            System.Array.Reverse(chars);
            return Result.Ok(new string(chars));
        }
        catch (Exception ex)
        {
            return Result.Fail<string>("Text.ReverseFailed", $"Failed to reverse text: {ex.Message}");
        }
    }

    /// <summary>
    /// Removes all whitespace characters from the input string.
    /// </summary>
    /// <param name="text">The input text to process.</param>
    /// <returns>A Result containing the text with all whitespace removed or an error.</returns>
    public static Result<string> RemoveWhitespace(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Result.Fail<string>("Text.InputEmpty", "Input text cannot be null or empty");
        }

        try
        {
            // Use regex to remove all whitespace
            var result = Regex.Replace(text, @"\s", string.Empty);
            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            return Result.Fail<string>("Text.RemoveWhitespaceFailed", $"Failed to remove whitespace: {ex.Message}");
        }
    }

    /// <summary>
    /// Wraps text at a specified line length while preserving word boundaries.
    /// </summary>
    /// <param name="text">The input text to wrap.</param>
    /// <param name="lineLength">The maximum length of each line.</param>
    /// <param name="newline">The newline character(s) to use. Defaults to "\n".</param>
    /// <returns>A Result containing the word-wrapped text or an error.</returns>
    public static Result<string> WordWrap(string text, int lineLength, string newline = "\n")
    {
        if (string.IsNullOrEmpty(text))
        {
            return Result.Fail<string>("Text.InputEmpty", "Input text cannot be null or empty");
        }

        if (lineLength <= 0)
        {
            return Result.Fail<string>("Text.InvalidLength", "Line length must be greater than 0");
        }

        if (newline == null)
        {
            return Result.Fail<string>("Text.InvalidNewline", "Newline cannot be null");
        }

        try
        {
            var words = text.Split(' ');
            var lines = new List<string>();
            var currentLine = new StringBuilder();

            foreach (var word in words)
            {
                // If word is longer than lineLength, it must be on its own line
                if (word.Length > lineLength)
                {
                    // Add current line if it has content
                    if (currentLine.Length > 0)
                    {
                        lines.Add(currentLine.ToString().TrimEnd());
                        currentLine.Clear();
                    }

                    // Add the long word on its own line
                    lines.Add(word);
                }
                // If adding the word exceeds line length, start a new line
                else if (currentLine.Length + word.Length + (currentLine.Length > 0 ? 1 : 0) > lineLength)
                {
                    if (currentLine.Length > 0)
                    {
                        lines.Add(currentLine.ToString().TrimEnd());
                        currentLine.Clear();
                    }

                    currentLine.Append(word);
                }
                // Otherwise, add the word to the current line
                else
                {
                    if (currentLine.Length > 0)
                    {
                        currentLine.Append(' ');
                    }

                    currentLine.Append(word);
                }
            }

            // Add any remaining content
            if (currentLine.Length > 0)
            {
                lines.Add(currentLine.ToString().TrimEnd());
            }

            var result = string.Join(newline, lines);
            return Result.Ok(result);
        }
        catch (Exception ex)
        {
            return Result.Fail<string>("Text.WordWrapFailed", $"Failed to wrap text: {ex.Message}");
        }
    }

    /// <summary>
    /// Repeats the input string the specified number of times.
    /// </summary>
    /// <param name="text">The input text to repeat.</param>
    /// <param name="count">The number of times to repeat the text.</param>
    /// <returns>A Result containing the repeated text or an error.</returns>
    public static Result<string> Repeat(string text, int count)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Result.Fail<string>("Text.InputEmpty", "Input text cannot be null or empty");
        }

        if (count < 0)
        {
            return Result.Fail<string>("Text.InvalidCount", "Count cannot be negative");
        }

        try
        {
            // If count is 0, return empty string
            if (count == 0)
            {
                return Result.Ok(string.Empty);
            }

            var result = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                result.Append(text);
            }

            return Result.Ok(result.ToString());
        }
        catch (Exception ex)
        {
            return Result.Fail<string>("Text.RepeatFailed", $"Failed to repeat text: {ex.Message}");
        }
    }
}

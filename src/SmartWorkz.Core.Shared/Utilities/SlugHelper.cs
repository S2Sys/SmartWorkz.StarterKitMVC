namespace SmartWorkz.Core.Shared.Utilities;

using System.Globalization;
using System.Text;
using SmartWorkz.Core.Shared.Results;

/// <summary>
/// Helper for generating URL-friendly slugs from text input.
/// </summary>
public sealed class SlugHelper
{
    /// <summary>
    /// Generates a URL-friendly slug from the given text with optional configuration.
    /// </summary>
    /// <param name="text">The input text to convert to a slug.</param>
    /// <param name="options">Configuration options. If null, default options are used.</param>
    /// <returns>A Result containing the generated slug or an error.</returns>
    public static Result<string> GenerateSlug(string text, SlugOptions? options = null)
    {
        // Handle null or empty input
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result.Fail<string>("Slug.InputEmpty", "Input text cannot be null or empty");
        }

        // Use default options if none provided
        options ??= new SlugOptions();

        // Validate options
        if (string.IsNullOrEmpty(options.Separator))
        {
            return Result.Fail<string>("Slug.InvalidSeparator", "Separator cannot be null or empty");
        }

        try
        {
            var slug = text.Trim();

            // Convert to lowercase if enabled
            if (options.Lowercase)
            {
                slug = slug.ToLowerInvariant();
            }

            // Remove accented characters if enabled
            if (options.RemoveAccents)
            {
                slug = RemoveAccents(slug);
            }

            // Replace spaces and special characters with separator if enabled
            if (options.RemoveSpecialChars)
            {
                slug = ReplaceSpecialCharacters(slug, options.Separator);
            }
            else
            {
                // Just replace spaces with separator, keep special characters
                slug = slug.Replace(" ", options.Separator);
            }

            // Remove leading and trailing separators
            slug = slug.Trim(options.Separator.ToCharArray());

            // Collapse multiple consecutive separators into a single separator
            while (slug.Contains(options.Separator + options.Separator))
            {
                slug = slug.Replace(options.Separator + options.Separator, options.Separator);
            }

            // Trim to MaxLength if specified (and greater than 0)
            if (options.MaxLength > 0 && slug.Length > options.MaxLength)
            {
                slug = slug.Substring(0, options.MaxLength).TrimEnd(options.Separator.ToCharArray());
            }

            // Return empty string error if slug ends up empty after processing
            if (string.IsNullOrWhiteSpace(slug))
            {
                return Result.Fail<string>("Slug.ResultEmpty", "Generated slug is empty after processing");
            }

            return Result.Ok(slug);
        }
        catch (Exception ex)
        {
            return Result.Fail<string>("Slug.GenerationFailed", $"Failed to generate slug: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates a URL-friendly slug from the given text using default options.
    /// Convenience method equivalent to GenerateSlug(text, null).
    /// </summary>
    /// <param name="text">The input text to convert to a slug.</param>
    /// <returns>A Result containing the generated slug or an error.</returns>
    public static Result<string> ToSlug(string text)
    {
        return GenerateSlug(text);
    }

    /// <summary>
    /// Removes accented characters from text by decomposing them and filtering out combining marks.
    /// For example: "café" → "cafe", "naïve" → "naive", "Señor" → "Senor".
    /// </summary>
    /// <param name="input">The input text potentially containing accented characters.</param>
    /// <returns>The text with accented characters converted to their base forms.</returns>
    private static string RemoveAccents(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        // Normalize to decomposed form (FormD)
        var normalized = input.Normalize(NormalizationForm.FormD);
        var result = new StringBuilder();

        foreach (var c in normalized)
        {
            // Skip combining marks (accents)
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                result.Append(c);
            }
        }

        // Normalize back to composed form (FormC)
        return result.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// Replaces special characters and spaces with the specified separator.
    /// Keeps only alphanumeric characters and the separator.
    /// </summary>
    /// <param name="input">The input text.</param>
    /// <param name="separator">The separator to use for special characters and spaces.</param>
    /// <returns>The text with special characters replaced by the separator.</returns>
    private static string ReplaceSpecialCharacters(string input, string separator)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        var result = new StringBuilder();

        foreach (var c in input)
        {
            if (char.IsLetterOrDigit(c))
            {
                result.Append(c);
            }
            else if (char.IsWhiteSpace(c) || char.IsPunctuation(c) || char.IsSymbol(c) || char.IsSeparator(c))
            {
                // Replace with separator only if the last character wasn't already the separator
                if (result.Length > 0 && !result.ToString().EndsWith(separator))
                {
                    result.Append(separator);
                }
            }
        }

        return result.ToString();
    }
}

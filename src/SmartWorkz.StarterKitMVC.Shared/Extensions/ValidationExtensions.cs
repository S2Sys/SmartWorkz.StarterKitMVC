namespace SmartWorkz.StarterKitMVC.Shared.Extensions;

/// <summary>
/// Extension methods for validation and guard clauses.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Throws <see cref="ArgumentNullException"/> if the value is null.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="paramName">The parameter name for the exception.</param>
    /// <exception cref="ArgumentNullException">Thrown when value is null.</exception>
    /// <example>
    /// <code>
    /// public void ProcessUser(User? user)
    /// {
    ///     user.EnsureNotNull(nameof(user));
    ///     // Safe to use user here
    ///     Console.WriteLine(user.Name);
    /// }
    /// </code>
    /// </example>
    public static void EnsureNotNull<T>(this T? value, string paramName) where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }
    }
}

namespace SmartWorkz.ECommerce.Mobile;

using Microsoft.Extensions.Logging;
using SmartWorkz.Mobile;

public sealed class LoginViewModel : ViewModelBase
{
    private readonly IAuthenticationHandler _auth;
    private readonly INavigationService     _nav;
    private readonly ILogger<LoginViewModel> _logger;

    private string _email    = string.Empty;
    private string _password = string.Empty;

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public AsyncCommand LoginCommand { get; }
    public AsyncCommand GoToRegisterCommand { get; }

    public LoginViewModel(
        IAuthenticationHandler auth,
        INavigationService nav,
        ILogger<LoginViewModel> logger)
    {
        _auth   = Guard.NotNull(auth,   nameof(auth));
        _nav    = Guard.NotNull(nav,    nameof(nav));
        _logger = Guard.NotNull(logger, nameof(logger));

        LoginCommand         = CreateCommand(ExecuteLoginAsync);
        GoToRegisterCommand  = CreateCommand(() => _nav.NavigateToAsync("register"));
    }

    private async Task ExecuteLoginAsync()
    {
        // Validation
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Email and password are required.";
            return;
        }

        // Authenticate
        await RunBusyAsync(async () =>
        {
            var result = await _auth.LoginAsync(Email, Password);
            if (!result.Succeeded)
            {
                ErrorMessage = result.Error?.Message ?? "Login failed.";
                _logger.LogWarning("Login failed for {Email}", Email);
                return;
            }

            _logger.LogInformation("Login successful for {Email}", Email);
            // Clear sensitive data
            Email    = string.Empty;
            Password = string.Empty;
            await _nav.NavigateToAsync("//home");
        });
    }
}

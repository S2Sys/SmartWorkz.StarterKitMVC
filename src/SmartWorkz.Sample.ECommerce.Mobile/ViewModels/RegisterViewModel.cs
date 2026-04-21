namespace SmartWorkz.ECommerce.Mobile;

using Microsoft.Extensions.Logging;
using SmartWorkz.Mobile;
using SmartWorkz.Sample.ECommerce.Application.DTOs;

public sealed class RegisterViewModel : ViewModelBase
{
    private readonly IAuthenticationHandler _auth;
    private readonly INavigationService     _nav;
    private readonly ILogger<RegisterViewModel> _logger;

    private string _firstName       = string.Empty;
    private string _lastName        = string.Empty;
    private string _email           = string.Empty;
    private string _password        = string.Empty;
    private string _confirmPassword = string.Empty;

    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value);
    }

    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value);
    }

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

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    public AsyncCommand RegisterCommand { get; }
    public AsyncCommand GoToLoginCommand { get; }

    public RegisterViewModel(
        IAuthenticationHandler auth,
        INavigationService nav,
        ILogger<RegisterViewModel> logger)
    {
        _auth   = Guard.NotNull(auth,   nameof(auth));
        _nav    = Guard.NotNull(nav,    nameof(nav));
        _logger = Guard.NotNull(logger, nameof(logger));

        RegisterCommand  = CreateCommand(ExecuteRegisterAsync);
        GoToLoginCommand = CreateCommand(() => _nav.NavigateToAsync("login"));
    }

    private async Task ExecuteRegisterAsync()
    {
        // Validation
        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "First name, email, and password are required.";
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return;
        }

        if (Password.Length < 8)
        {
            ErrorMessage = "Password must be at least 8 characters.";
            return;
        }

        // Register
        await RunBusyAsync(async () =>
        {
            var dto = new RegisterDto(FirstName, LastName, Email, Password, ConfirmPassword);
            var result = await _auth.RegisterAsync(dto);
            if (!result.Succeeded)
            {
                ErrorMessage = result.Error?.Message ?? "Registration failed.";
                _logger.LogWarning("Registration failed for {Email}", Email);
                return;
            }

            _logger.LogInformation("Registration successful for {Email}", Email);
            // Clear form
            FirstName = LastName = Email = Password = ConfirmPassword = string.Empty;
            // Navigate to login
            await _nav.NavigateToAsync("login");
        });
    }
}

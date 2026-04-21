namespace SmartWorkz.Mobile;

public interface IBiometricService
{
    Task<bool> IsAvailableAsync(CancellationToken ct = default);
    Task<BiometricType> GetBiometricTypeAsync(CancellationToken ct = default);
    Task<Result<bool>> AuthenticateAsync(string reason, CancellationToken ct = default);
}

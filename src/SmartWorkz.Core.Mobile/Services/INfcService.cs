// src/SmartWorkz.Core.Mobile/Services/INfcService.cs
namespace SmartWorkz.Mobile;

public interface INfcService
{
    Task<Result<NfcMessage>> ReadAsync(CancellationToken ct = default);
    Task<bool> IsAvailableAsync(CancellationToken ct = default);
    Task<bool> IsEnabledAsync(CancellationToken ct = default);
}

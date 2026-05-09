namespace SmartWorkz.Mobile;

#if !WINDOWS
using System.Reactive.Linq;
using System.Reactive.Subjects;
#endif

public sealed class ResponsiveService : IResponsiveService
#if !WINDOWS
    , IDisposable
#endif
{
    private readonly IMobileService _mobileService;
#if !WINDOWS
    private readonly Subject<DeviceProfile> _subject = new();
#endif

    public ResponsiveService(IMobileService mobileService)
    {
        _mobileService = Guard.NotNull(mobileService, nameof(mobileService));
    }

    public DeviceProfile GetProfile()
    {
        var type = _mobileService.GetDeviceType();
        return type switch
        {
            DeviceType.Tablet  => new DeviceProfile(type, 3, 24.0, true),
            DeviceType.Desktop => new DeviceProfile(type, 4, 32.0, true),
            _                  => new DeviceProfile(type, 2, 16.0, false),
        };
    }

#if !WINDOWS
    public IObservable<DeviceProfile> OnProfileChanged() => _subject.AsObservable();

    public void Dispose()
    {
        _subject.OnCompleted();
        _subject.Dispose();
    }
#endif
}

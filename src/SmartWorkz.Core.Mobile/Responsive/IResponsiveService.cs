namespace SmartWorkz.Mobile;

public interface IResponsiveService
{
    DeviceProfile GetProfile();
#if !WINDOWS
    IObservable<DeviceProfile> OnProfileChanged();
#endif
}

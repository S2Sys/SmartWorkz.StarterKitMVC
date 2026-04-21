namespace SmartWorkz.Mobile;

public sealed record DeviceProfile(
    DeviceType Type,
    int ColumnCount,
    double SideMargin,
    bool IsTabletOrDesktop);

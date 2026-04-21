namespace SmartWorkz.Mobile.Tests.Responsive;

using Moq;

public class ResponsiveServiceTests
{
    private static Mock<IMobileService> MockMobileService(DeviceType type)
    {
        var mock = new Mock<IMobileService>();
        mock.Setup(m => m.GetDeviceType()).Returns(type);
        mock.Setup(m => m.IsTablet()).Returns(type == DeviceType.Tablet);
        return mock;
    }

    [Fact]
    public void GetProfile_Phone_ReturnsTwoColumns()
    {
        var svc = new ResponsiveService(MockMobileService(DeviceType.Phone).Object);

        var profile = svc.GetProfile();

        Assert.Equal(2, profile.ColumnCount);
        Assert.Equal(DeviceType.Phone, profile.Type);
        Assert.False(profile.IsTabletOrDesktop);
    }

    [Fact]
    public void GetProfile_Tablet_ReturnsThreeColumns()
    {
        var svc = new ResponsiveService(MockMobileService(DeviceType.Tablet).Object);

        var profile = svc.GetProfile();

        Assert.Equal(3, profile.ColumnCount);
        Assert.True(profile.IsTabletOrDesktop);
    }

    [Fact]
    public void GetProfile_Desktop_ReturnsFourColumns()
    {
        var svc = new ResponsiveService(MockMobileService(DeviceType.Desktop).Object);

        var profile = svc.GetProfile();

        Assert.Equal(4, profile.ColumnCount);
        Assert.True(profile.IsTabletOrDesktop);
    }

    [Fact]
    public void GetProfile_Unknown_ReturnsTwoColumns()
    {
        var svc = new ResponsiveService(MockMobileService(DeviceType.Unknown).Object);

        var profile = svc.GetProfile();

        Assert.Equal(2, profile.ColumnCount);
    }
}

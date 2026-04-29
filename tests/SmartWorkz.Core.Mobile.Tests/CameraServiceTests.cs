namespace SmartWorkz.Mobile.Tests;

using SmartWorkz.Mobile;
using Xunit;

[Collection("CameraService Tests")]
public class CameraServiceTests
{
    [Fact]
    public void Photo_Constructor_InitializesWithDefaults()
    {
        var photo = new Photo();

        Assert.Equal(string.Empty, photo.FilePath);
        Assert.Null(photo.ImageBytes);
        Assert.Equal(0, photo.Width);
        Assert.Equal(0, photo.Height);
        Assert.NotEqual(default(DateTime), photo.CaptureTime);
    }

    [Fact]
    public void Photo_Base64_ReturnsEncodedImageBytes()
    {
        var imageBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; // JPEG header
        var photo = new Photo { ImageBytes = imageBytes };

        var base64 = photo.Base64;

        Assert.NotNull(base64);
        Assert.Equal(Convert.ToBase64String(imageBytes), base64);
    }

    [Fact]
    public void Photo_Base64_ReturnsNullWhenNoImageBytes()
    {
        var photo = new Photo { ImageBytes = null };

        var base64 = photo.Base64;

        Assert.Null(base64);
    }

    [Fact]
    public void Video_Constructor_InitializesWithDefaults()
    {
        var video = new Video();

        Assert.Equal(string.Empty, video.FilePath);
        Assert.Equal(string.Empty, video.Resolution);
        Assert.Equal(TimeSpan.Zero, video.Duration);
        Assert.NotEqual(default(DateTime), video.CaptureTime);
    }

    [Fact]
    public void Video_WithResolution_StoresCorrectly()
    {
        var video = new Video { Resolution = "1920x1080", Duration = TimeSpan.FromMinutes(5) };

        Assert.Equal("1920x1080", video.Resolution);
        Assert.Equal(TimeSpan.FromMinutes(5), video.Duration);
    }
}

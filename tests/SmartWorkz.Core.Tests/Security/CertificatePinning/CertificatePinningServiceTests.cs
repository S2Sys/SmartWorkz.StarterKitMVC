namespace SmartWorkz.Core.Tests.Security.CertificatePinning;

using System.Security.Cryptography.X509Certificates;
using SmartWorkz.Shared.Security.CertificatePinning;
using Xunit;

public class CertificatePinningServiceTests
{
    private readonly CertificatePinningService _sut = new();

    [Fact]
    public void AddPin_ValidPin_StoresSuccessfully()
    {
        // Arrange
        var hostName = "api.smartworkz.com";
        var spkiPin = "sha256/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";

        // Act
        _sut.AddPin(hostName, spkiPin);

        // Assert
        var pins = _sut.GetPins(hostName);
        Assert.Contains(spkiPin, pins);
    }

    [Fact]
    public void GetPins_NonExistentHost_ReturnsEmptyList()
    {
        // Act
        var pins = _sut.GetPins("nonexistent.com");

        // Assert
        Assert.Empty(pins);
    }

    [Fact]
    public void RemovePins_ExistingPins_Removes()
    {
        // Arrange
        var hostName = "api.smartworkz.com";
        var spkiPin = "sha256/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";
        _sut.AddPin(hostName, spkiPin);

        // Act
        _sut.RemovePins(hostName);

        // Assert
        var pins = _sut.GetPins(hostName);
        Assert.Empty(pins);
    }

    [Fact]
    public void ValidateCertificatePin_NoPinsConfigured_ReturnsFalse()
    {
        // Arrange
        // Create an empty certificate (no pins configured for the host)
        X509Certificate2? cert = null;
        try
        {
            cert = new X509Certificate2();
            // Act
            var result = _sut.ValidateCertificatePin(cert, "api.smartworkz.com");

            // Assert
            Assert.False(result);
        }
        finally
        {
            cert?.Dispose();
        }
    }

    [Fact]
    public void AddPin_MultipleHosts_IsolatesPins()
    {
        // Arrange
        var host1 = "api1.smartworkz.com";
        var host2 = "api2.smartworkz.com";
        var pin1 = "sha256/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";
        var pin2 = "sha256/BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB=";

        // Act
        _sut.AddPin(host1, pin1);
        _sut.AddPin(host2, pin2);

        // Assert
        var pinsHost1 = _sut.GetPins(host1);
        var pinsHost2 = _sut.GetPins(host2);

        Assert.Contains(pin1, pinsHost1);
        Assert.DoesNotContain(pin2, pinsHost1);
        Assert.Contains(pin2, pinsHost2);
        Assert.DoesNotContain(pin1, pinsHost2);
    }

    [Fact]
    public void AddPin_DuplicatePin_DoesNotAddDuplicate()
    {
        // Arrange
        var hostName = "api.smartworkz.com";
        var spkiPin = "sha256/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";

        // Act
        _sut.AddPin(hostName, spkiPin);
        _sut.AddPin(hostName, spkiPin);

        // Assert
        var pins = _sut.GetPins(hostName);
        Assert.Single(pins);
    }

    [Fact]
    public void AddPin_NullHostName_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => _sut.AddPin(null!, "pin"));
    }

    [Fact]
    public void AddPin_NullPin_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => _sut.AddPin("host.com", null!));
    }

    [Fact]
    public void AddPin_EmptyHostName_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => _sut.AddPin("", "pin"));
    }

    [Fact]
    public void GetPins_NullHostName_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => _sut.GetPins(null!));
    }

    [Fact]
    public void RemovePins_NullHostName_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => _sut.RemovePins(null!));
    }

    [Fact]
    public void ValidateCertificatePin_NullCertificate_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => _sut.ValidateCertificatePin(null!, "host.com"));
    }

    [Fact]
    public void ValidateCertificatePin_NullHostName_ThrowsArgumentException()
    {
        // Arrange
        X509Certificate2? cert = null;
        try
        {
            cert = new X509Certificate2();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _sut.ValidateCertificatePin(cert, null!));
        }
        finally
        {
            cert?.Dispose();
        }
    }

    [Fact]
    public void GetPins_HostNameIsCaseInsensitive()
    {
        // Arrange
        var hostName = "API.SmartWorkz.COM";
        var spkiPin = "sha256/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";
        _sut.AddPin(hostName, spkiPin);

        // Act
        var pins = _sut.GetPins("api.smartworkz.com");

        // Assert
        Assert.Contains(spkiPin, pins);
    }

    [Fact]
    public void AddPin_MultipleValidPins_StoresAll()
    {
        // Arrange
        var hostName = "api.smartworkz.com";
        var pin1 = "sha256/AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";
        var pin2 = "sha256/BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB=";
        var pin3 = "sha256/CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC=";

        // Act
        _sut.AddPin(hostName, pin1);
        _sut.AddPin(hostName, pin2);
        _sut.AddPin(hostName, pin3);

        // Assert
        var pins = _sut.GetPins(hostName);
        Assert.Equal(3, pins.Count);
        Assert.Contains(pin1, pins);
        Assert.Contains(pin2, pins);
        Assert.Contains(pin3, pins);
    }
}

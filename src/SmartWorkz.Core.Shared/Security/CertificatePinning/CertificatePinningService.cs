namespace SmartWorkz.Shared.Security.CertificatePinning;

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

/// <summary>
/// Implements certificate pinning using SPKI (Subject Public Key Info) hashing.
/// This service manages and validates certificate pins for specific hostnames,
/// supporting multiple pins per hostname for certificate rotation scenarios.
/// </summary>
public sealed class CertificatePinningService : ICertificatePinningService
{
    private readonly Dictionary<string, List<string>> _pins = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lockObject = new();

    /// <summary>
    /// Adds a certificate pin for the specified hostname.
    /// Thread-safe operation that supports multiple pins per hostname.
    /// </summary>
    /// <param name="hostName">The hostname to add the pin for.</param>
    /// <param name="spkiPin">The SPKI pin in sha256/BASE64 format.</param>
    /// <exception cref="ArgumentException">Thrown if hostName or spkiPin is null or whitespace.</exception>
    public void AddPin(string hostName, string spkiPin)
    {
        Guard.NotEmpty(hostName, nameof(hostName));
        Guard.NotEmpty(spkiPin, nameof(spkiPin));

        lock (_lockObject)
        {
            if (!_pins.ContainsKey(hostName))
                _pins[hostName] = new List<string>();

            if (!_pins[hostName].Contains(spkiPin, StringComparer.Ordinal))
                _pins[hostName].Add(spkiPin);
        }
    }

    /// <summary>
    /// Retrieves all pins for the specified hostname.
    /// Returns an empty list if no pins are configured.
    /// </summary>
    /// <param name="hostName">The hostname to retrieve pins for.</param>
    /// <returns>A read-only list of SPKI pins for the hostname.</returns>
    /// <exception cref="ArgumentException">Thrown if hostName is null or whitespace.</exception>
    public IReadOnlyList<string> GetPins(string hostName)
    {
        Guard.NotEmpty(hostName, nameof(hostName));

        lock (_lockObject)
        {
            if (_pins.TryGetValue(hostName, out var pins))
                return pins.AsReadOnly();

            return Array.Empty<string>();
        }
    }

    /// <summary>
    /// Removes all pins for the specified hostname.
    /// Thread-safe operation.
    /// </summary>
    /// <param name="hostName">The hostname to remove pins for.</param>
    /// <exception cref="ArgumentException">Thrown if hostName is null or whitespace.</exception>
    public void RemovePins(string hostName)
    {
        Guard.NotEmpty(hostName, nameof(hostName));

        lock (_lockObject)
        {
            _pins.Remove(hostName);
        }
    }

    /// <summary>
    /// Validates if the provided certificate's public key matches any pinned certificate for the hostname.
    /// Returns false if no pins are configured for the hostname or if extraction fails.
    /// </summary>
    /// <param name="certificate">The X.509 certificate to validate.</param>
    /// <param name="hostName">The hostname to validate against.</param>
    /// <returns>True if the certificate's SPKI matches a pinned certificate; otherwise false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if certificate is null.</exception>
    /// <exception cref="ArgumentException">Thrown if hostName is null or whitespace.</exception>
    public bool ValidateCertificatePin(X509Certificate2 certificate, string hostName)
    {
        Guard.NotNull(certificate, nameof(certificate));
        Guard.NotEmpty(hostName, nameof(hostName));

        var pins = GetPins(hostName);
        if (pins.Count == 0)
            return false;

        try
        {
            var spkiPin = ExtractPublicKeyPin(certificate);
            return pins.Contains(spkiPin, StringComparer.Ordinal);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Extracts the SPKI (Subject Public Key Info) pin from an X.509 certificate.
    /// The pin is computed as SHA-256 hash of the public key bytes, encoded as "sha256/BASE64".
    /// </summary>
    /// <param name="certificate">The certificate to extract the pin from.</param>
    /// <returns>The SPKI pin in sha256/BASE64 format.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the public key cannot be extracted.</exception>
    private string ExtractPublicKeyPin(X509Certificate2 certificate)
    {
        var publicKey = certificate.PublicKey;
        if (publicKey?.EncodedKeyValue == null)
            throw new InvalidOperationException("Unable to extract public key from certificate.");

        var spkiBytes = publicKey.EncodedKeyValue.RawData;

        using (var sha256 = SHA256.Create())
        {
            var hash = sha256.ComputeHash(spkiBytes);
            return "sha256/" + Convert.ToBase64String(hash);
        }
    }
}

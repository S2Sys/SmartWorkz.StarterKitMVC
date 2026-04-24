namespace SmartWorkz.Windows.Security;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// HttpClientCertificatePinningHandler extends HttpClientHandler to perform certificate pinning
/// using SPKI (Subject Public Key Info) pins for Windows applications.
///
/// Certificate pinning enhances security by validating that the server's certificate
/// matches a pinned public key, preventing man-in-the-middle attacks even if a CA is compromised.
///
/// Usage:
///     var handler = new HttpClientCertificatePinningHandler();
///     handler.AddPin("api.example.com", "sha256/...");
///     using var httpClient = new HttpClient(handler);
/// </summary>
public sealed class HttpClientCertificatePinningHandler : HttpClientHandler
{
    private readonly Dictionary<string, List<string>> _pinnedCertificates =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly ReaderWriterLockSlim _lock = new();

    /// <summary>
    /// Initializes a new instance of the HttpClientCertificatePinningHandler class.
    /// </summary>
    public HttpClientCertificatePinningHandler()
    {
        ServerCertificateCustomValidationCallback = ValidateServerCertificate;
    }

    /// <summary>
    /// Adds a certificate pin for the specified host.
    /// Thread-safe operation that supports multiple pins per host.
    /// </summary>
    /// <param name="host">The hostname to add the pin for</param>
    /// <param name="spkiPin">The SPKI pin in sha256/BASE64 format</param>
    /// <exception cref="ArgumentException">Thrown if host or spkiPin is null or whitespace</exception>
    public void AddPin(string host, string spkiPin)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("Host cannot be null or whitespace.", nameof(host));
        if (string.IsNullOrWhiteSpace(spkiPin))
            throw new ArgumentException("SPKI pin cannot be null or whitespace.", nameof(spkiPin));

        _lock.EnterWriteLock();
        try
        {
            if (!_pinnedCertificates.ContainsKey(host))
                _pinnedCertificates[host] = new List<string>();

            if (!_pinnedCertificates[host].Contains(spkiPin, StringComparer.Ordinal))
                _pinnedCertificates[host].Add(spkiPin);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Retrieves all pins for the specified host.
    /// </summary>
    /// <param name="host">The hostname to retrieve pins for</param>
    /// <returns>A read-only list of SPKI pins for the host, or empty if no pins exist</returns>
    /// <exception cref="ArgumentException">Thrown if host is null or whitespace</exception>
    public IReadOnlyList<string> GetPins(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("Host cannot be null or whitespace.", nameof(host));

        _lock.EnterReadLock();
        try
        {
            if (_pinnedCertificates.TryGetValue(host, out var pins))
                return pins.AsReadOnly();

            return Array.Empty<string>();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Removes all pins for the specified host.
    /// </summary>
    /// <param name="host">The hostname to remove pins for</param>
    /// <exception cref="ArgumentException">Thrown if host is null or whitespace</exception>
    public void RemovePins(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException("Host cannot be null or whitespace.", nameof(host));

        _lock.EnterWriteLock();
        try
        {
            _pinnedCertificates.Remove(host);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Validates the server certificate against pinned certificates.
    /// This method is called during the SSL/TLS handshake to validate the server certificate.
    /// </summary>
    private bool ValidateServerCertificate(
        HttpRequestMessage request,
        X509Certificate2? certificate,
        X509Chain? chain,
        System.Net.Security.SslPolicyErrors sslPolicyErrors)
    {
        if (certificate == null)
            return false;

        var host = request.RequestUri?.Host ?? string.Empty;
        var pins = GetPins(host);

        // If no pins are configured for this host, use default validation
        if (pins.Count == 0)
            return sslPolicyErrors == System.Net.Security.SslPolicyErrors.None;

        // Validate certificate chain first
        if (sslPolicyErrors != System.Net.Security.SslPolicyErrors.None)
            return false;

        try
        {
            var pin = ExtractPublicKeyPin(certificate);
            return pins.Contains(pin, StringComparer.Ordinal);
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
    private static string ExtractPublicKeyPin(X509Certificate2 certificate)
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

    /// <summary>
    /// Disposes the handler and releases the ReaderWriterLockSlim resource.
    /// </summary>
    public override void Dispose()
    {
        _lock?.Dispose();
        base.Dispose();
    }
}

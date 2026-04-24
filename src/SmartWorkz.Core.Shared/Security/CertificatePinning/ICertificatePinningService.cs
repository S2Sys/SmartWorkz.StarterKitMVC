namespace SmartWorkz.Shared.Security.CertificatePinning;

using System.Security.Cryptography.X509Certificates;

/// <summary>
/// Defines the contract for certificate pinning services.
/// Certificate pinning enhances security by validating that the server's certificate
/// matches a pinned public key, preventing man-in-the-middle attacks even if a CA is compromised.
/// </summary>
public interface ICertificatePinningService
{
    /// <summary>
    /// Validates if the provided certificate matches any pinned certificate for the given hostname.
    /// </summary>
    /// <param name="certificate">The X.509 certificate to validate.</param>
    /// <param name="hostName">The hostname to validate against.</param>
    /// <returns>True if the certificate matches a pinned certificate; otherwise false.</returns>
    bool ValidateCertificatePin(X509Certificate2 certificate, string hostName);

    /// <summary>
    /// Adds a certificate pin for the specified hostname.
    /// </summary>
    /// <param name="hostName">The hostname to add the pin for.</param>
    /// <param name="spkiPin">The SPKI (Subject Public Key Info) pin in sha256/BASE64 format.</param>
    void AddPin(string hostName, string spkiPin);

    /// <summary>
    /// Retrieves all pins for the specified hostname.
    /// </summary>
    /// <param name="hostName">The hostname to retrieve pins for.</param>
    /// <returns>A read-only list of pins for the hostname, or empty if no pins exist.</returns>
    IReadOnlyList<string> GetPins(string hostName);

    /// <summary>
    /// Removes all pins for the specified hostname.
    /// </summary>
    /// <param name="hostName">The hostname to remove pins for.</param>
    void RemovePins(string hostName);
}

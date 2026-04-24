import Foundation
import Security

/// URLSessionCertificatePinningDelegate implements URLSessionDelegate to perform certificate pinning
/// using SPKI (Subject Public Key Info) pins for iOS applications.
///
/// Certificate pinning enhances security by validating that the server's certificate
/// matches a pinned public key, preventing man-in-the-middle attacks even if a CA is compromised.
///
/// Usage:
///     let delegate = URLSessionCertificatePinningDelegate()
///     delegate.addPin(host: "api.example.com", spkiPin: "sha256/...")
///     let session = URLSession(configuration: .default, delegate: delegate, delegateQueue: nil)
class URLSessionCertificatePinningDelegate: NSObject, URLSessionDelegate {
    private let queue = DispatchQueue(label: "com.smartworkz.certificate.pinning.ios", attributes: .concurrent)
    private var pinnedCertificates: [String: [String]] = [:]

    /// Adds a certificate pin for the specified host.
    /// Thread-safe operation that supports multiple pins per host.
    ///
    /// - Parameters:
    ///   - host: The hostname to add the pin for
    ///   - spkiPin: The SPKI pin in sha256/BASE64 format
    func addPin(host: String, spkiPin: String) {
        queue.async(flags: .barrier) {
            if self.pinnedCertificates[host] == nil {
                self.pinnedCertificates[host] = []
            }
            if !self.pinnedCertificates[host]!.contains(spkiPin) {
                self.pinnedCertificates[host]?.append(spkiPin)
            }
        }
    }

    /// Retrieves all pins for the specified host.
    ///
    /// - Parameter host: The hostname to retrieve pins for
    /// - Returns: An array of SPKI pins for the host, or empty array if no pins exist
    func getPins(host: String) -> [String] {
        var pins: [String] = []
        queue.sync {
            pins = self.pinnedCertificates[host] ?? []
        }
        return pins
    }

    /// Removes all pins for the specified host.
    ///
    /// - Parameter host: The hostname to remove pins for
    func removePins(host: String) {
        queue.async(flags: .barrier) {
            self.pinnedCertificates.removeValue(forKey: host)
        }
    }

    /// URLSessionDelegate method that handles server trust evaluation.
    /// Validates the certificate against pinned certificates for the host.
    func urlSession(
        _ session: URLSession,
        didReceive challenge: URLAuthenticationChallenge,
        completionHandler: @escaping (URLSession.AuthChallengeDisposition, URLCredential?) -> Void
    ) {
        guard challenge.protectionSpace.authenticationMethod == NSURLAuthenticationMethodServerTrust,
              let serverTrust = challenge.protectionSpace.serverTrust else {
            completionHandler(.performDefaultHandling, nil)
            return
        }

        let host = challenge.protectionSpace.host
        let pins = getPins(host: host)

        // If no pins are configured for this host, use default handling
        guard !pins.isEmpty else {
            completionHandler(.performDefaultHandling, nil)
            return
        }

        // Validate certificate chain
        var secResult = SecTrustResultType.invalid
        let status = SecTrustEvaluate(serverTrust, &secResult)

        guard status == errSecSuccess else {
            completionHandler(.cancelAuthenticationChallenge, nil)
            return
        }

        // Extract certificate from trust and verify pin
        if let certificate = SecTrustGetCertificateAtIndex(serverTrust, 0) {
            if validatePin(certificate: certificate, against: pins) {
                completionHandler(.useCredential, URLCredential(trust: serverTrust))
                return
            }
        }

        completionHandler(.cancelAuthenticationChallenge, nil)
    }

    /// Validates if the certificate's public key matches any pinned certificate.
    ///
    /// - Parameters:
    ///   - certificate: The SecCertificate to validate
    ///   - pins: The array of SPKI pins to validate against
    /// - Returns: True if the certificate matches a pinned certificate; otherwise false
    private func validatePin(certificate: SecCertificate, against pins: [String]) -> Bool {
        guard let publicKey = SecCertificateCopyKey(certificate) else {
            return false
        }

        let keyData = SecKeyCopyExternalRepresentation(publicKey, nil) as Data?
        guard let keyData = keyData else {
            return false
        }

        let digest = SHA256.hash(data: keyData)
        let pin = "sha256/" + Data(digest).base64EncodedString()

        return pins.contains(pin)
    }
}

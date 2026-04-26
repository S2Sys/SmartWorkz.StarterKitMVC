# CertificatePinning API Reference

## Classes & Interfaces

### CertificatePinningService

- **Namespace:** `SmartWorkz.Shared.Security.CertificatePinning.CertificatePinningService`
- **Summary:** Implements certificate pinning using SPKI (Subject Public Key Info) hashing.
            This service manages and validates certificate pins for specific hostnames,
            supporting multiple pins per hostname for certificate rotation scenarios.

#### Methods & Properties

- **AddPin** - Adds a certificate pin for the specified hostname.
            Thread-safe operation that supports multiple pins per hostname.
  - Parameters:
    - `hostName`: The hostname to add the pin for.
    - `spkiPin`: The SPKI pin in sha256/BASE64 format.
- **GetPins** - Retrieves all pins for the specified hostname.
            Returns an empty list if no pins are configured.
  - Parameters:
    - `hostName`: The hostname to retrieve pins for.
  - Returns: A read-only list of SPKI pins for the hostname.
- **RemovePins** - Removes all pins for the specified hostname.
            Thread-safe operation.
  - Parameters:
    - `hostName`: The hostname to remove pins for.
- **ValidateCertificatePin** - Validates if the provided certificate's public key matches any pinned certificate for the hostname.
            Returns false if no pins are configured for the hostname or if extraction fails.
  - Parameters:
    - `certificate`: The X.509 certificate to validate.
    - `hostName`: The hostname to validate against.
  - Returns: True if the certificate's SPKI matches a pinned certificate; otherwise false.
- **ExtractPublicKeyPin** - Extracts the SPKI (Subject Public Key Info) pin from an X.509 certificate.
            The pin is computed as SHA-256 hash of the public key bytes, encoded as "sha256/BASE64".
  - Parameters:
    - `certificate`: The certificate to extract the pin from.
  - Returns: The SPKI pin in sha256/BASE64 format.

### ICertificatePinningService

- **Namespace:** `SmartWorkz.Shared.Security.CertificatePinning.ICertificatePinningService`
- **Summary:** Defines the contract for certificate pinning services.
            Certificate pinning enhances security by validating that the server's certificate
            matches a pinned public key, preventing man-in-the-middle attacks even if a CA is compromised.

#### Methods & Properties

- **ValidateCertificatePin** - Validates if the provided certificate matches any pinned certificate for the given hostname.
  - Parameters:
    - `certificate`: The X.509 certificate to validate.
    - `hostName`: The hostname to validate against.
  - Returns: True if the certificate matches a pinned certificate; otherwise false.
- **AddPin** - Adds a certificate pin for the specified hostname.
  - Parameters:
    - `hostName`: The hostname to add the pin for.
    - `spkiPin`: The SPKI (Subject Public Key Info) pin in sha256/BASE64 format.
- **GetPins** - Retrieves all pins for the specified hostname.
  - Parameters:
    - `hostName`: The hostname to retrieve pins for.
  - Returns: A read-only list of pins for the hostname, or empty if no pins exist.
- **RemovePins** - Removes all pins for the specified hostname.
  - Parameters:
    - `hostName`: The hostname to remove pins for.

### CertificatePinningService

- **Namespace:** `SmartWorkz.Shared.Security.CertificatePinning.CertificatePinningService`
- **Summary:** Implements certificate pinning using SPKI (Subject Public Key Info) hashing.
            This service manages and validates certificate pins for specific hostnames,
            supporting multiple pins per hostname for certificate rotation scenarios.

#### Methods & Properties

- **AddPin** - Adds a certificate pin for the specified hostname.
            Thread-safe operation that supports multiple pins per hostname.
  - Parameters:
    - `hostName`: The hostname to add the pin for.
    - `spkiPin`: The SPKI pin in sha256/BASE64 format.
- **GetPins** - Retrieves all pins for the specified hostname.
            Returns an empty list if no pins are configured.
  - Parameters:
    - `hostName`: The hostname to retrieve pins for.
  - Returns: A read-only list of SPKI pins for the hostname.
- **RemovePins** - Removes all pins for the specified hostname.
            Thread-safe operation.
  - Parameters:
    - `hostName`: The hostname to remove pins for.
- **ValidateCertificatePin** - Validates if the provided certificate's public key matches any pinned certificate for the hostname.
            Returns false if no pins are configured for the hostname or if extraction fails.
  - Parameters:
    - `certificate`: The X.509 certificate to validate.
    - `hostName`: The hostname to validate against.
  - Returns: True if the certificate's SPKI matches a pinned certificate; otherwise false.
- **ExtractPublicKeyPin** - Extracts the SPKI (Subject Public Key Info) pin from an X.509 certificate.
            The pin is computed as SHA-256 hash of the public key bytes, encoded as "sha256/BASE64".
  - Parameters:
    - `certificate`: The certificate to extract the pin from.
  - Returns: The SPKI pin in sha256/BASE64 format.

### ICertificatePinningService

- **Namespace:** `SmartWorkz.Shared.Security.CertificatePinning.ICertificatePinningService`
- **Summary:** Defines the contract for certificate pinning services.
            Certificate pinning enhances security by validating that the server's certificate
            matches a pinned public key, preventing man-in-the-middle attacks even if a CA is compromised.

#### Methods & Properties

- **ValidateCertificatePin** - Validates if the provided certificate matches any pinned certificate for the given hostname.
  - Parameters:
    - `certificate`: The X.509 certificate to validate.
    - `hostName`: The hostname to validate against.
  - Returns: True if the certificate matches a pinned certificate; otherwise false.
- **AddPin** - Adds a certificate pin for the specified hostname.
  - Parameters:
    - `hostName`: The hostname to add the pin for.
    - `spkiPin`: The SPKI (Subject Public Key Info) pin in sha256/BASE64 format.
- **GetPins** - Retrieves all pins for the specified hostname.
  - Parameters:
    - `hostName`: The hostname to retrieve pins for.
  - Returns: A read-only list of pins for the hostname, or empty if no pins exist.
- **RemovePins** - Removes all pins for the specified hostname.
  - Parameters:
    - `hostName`: The hostname to remove pins for.

### CertificatePinningService

- **Namespace:** `SmartWorkz.Shared.Security.CertificatePinning.CertificatePinningService`
- **Summary:** Implements certificate pinning using SPKI (Subject Public Key Info) hashing.
            This service manages and validates certificate pins for specific hostnames,
            supporting multiple pins per hostname for certificate rotation scenarios.

#### Methods & Properties

- **AddPin** - Adds a certificate pin for the specified hostname.
            Thread-safe operation that supports multiple pins per hostname.
  - Parameters:
    - `hostName`: The hostname to add the pin for.
    - `spkiPin`: The SPKI pin in sha256/BASE64 format.
- **GetPins** - Retrieves all pins for the specified hostname.
            Returns an empty list if no pins are configured.
  - Parameters:
    - `hostName`: The hostname to retrieve pins for.
  - Returns: A read-only list of SPKI pins for the hostname.
- **RemovePins** - Removes all pins for the specified hostname.
            Thread-safe operation.
  - Parameters:
    - `hostName`: The hostname to remove pins for.
- **ValidateCertificatePin** - Validates if the provided certificate's public key matches any pinned certificate for the hostname.
            Returns false if no pins are configured for the hostname or if extraction fails.
  - Parameters:
    - `certificate`: The X.509 certificate to validate.
    - `hostName`: The hostname to validate against.
  - Returns: True if the certificate's SPKI matches a pinned certificate; otherwise false.
- **ExtractPublicKeyPin** - Extracts the SPKI (Subject Public Key Info) pin from an X.509 certificate.
            The pin is computed as SHA-256 hash of the public key bytes, encoded as "sha256/BASE64".
  - Parameters:
    - `certificate`: The certificate to extract the pin from.
  - Returns: The SPKI pin in sha256/BASE64 format.

### ICertificatePinningService

- **Namespace:** `SmartWorkz.Shared.Security.CertificatePinning.ICertificatePinningService`
- **Summary:** Defines the contract for certificate pinning services.
            Certificate pinning enhances security by validating that the server's certificate
            matches a pinned public key, preventing man-in-the-middle attacks even if a CA is compromised.

#### Methods & Properties

- **ValidateCertificatePin** - Validates if the provided certificate matches any pinned certificate for the given hostname.
  - Parameters:
    - `certificate`: The X.509 certificate to validate.
    - `hostName`: The hostname to validate against.
  - Returns: True if the certificate matches a pinned certificate; otherwise false.
- **AddPin** - Adds a certificate pin for the specified hostname.
  - Parameters:
    - `hostName`: The hostname to add the pin for.
    - `spkiPin`: The SPKI (Subject Public Key Info) pin in sha256/BASE64 format.
- **GetPins** - Retrieves all pins for the specified hostname.
  - Parameters:
    - `hostName`: The hostname to retrieve pins for.
  - Returns: A read-only list of pins for the hostname, or empty if no pins exist.
- **RemovePins** - Removes all pins for the specified hostname.
  - Parameters:
    - `hostName`: The hostname to remove pins for.

### CertificatePinningService

- **Namespace:** `SmartWorkz.Shared.Security.CertificatePinning.CertificatePinningService`
- **Summary:** Implements certificate pinning using SPKI (Subject Public Key Info) hashing.
            This service manages and validates certificate pins for specific hostnames,
            supporting multiple pins per hostname for certificate rotation scenarios.

#### Methods & Properties

- **AddPin** - Adds a certificate pin for the specified hostname.
            Thread-safe operation that supports multiple pins per hostname.
  - Parameters:
    - `hostName`: The hostname to add the pin for.
    - `spkiPin`: The SPKI pin in sha256/BASE64 format.
- **GetPins** - Retrieves all pins for the specified hostname.
            Returns an empty list if no pins are configured.
  - Parameters:
    - `hostName`: The hostname to retrieve pins for.
  - Returns: A read-only list of SPKI pins for the hostname.
- **RemovePins** - Removes all pins for the specified hostname.
            Thread-safe operation.
  - Parameters:
    - `hostName`: The hostname to remove pins for.
- **ValidateCertificatePin** - Validates if the provided certificate's public key matches any pinned certificate for the hostname.
            Returns false if no pins are configured for the hostname or if extraction fails.
  - Parameters:
    - `certificate`: The X.509 certificate to validate.
    - `hostName`: The hostname to validate against.
  - Returns: True if the certificate's SPKI matches a pinned certificate; otherwise false.
- **ExtractPublicKeyPin** - Extracts the SPKI (Subject Public Key Info) pin from an X.509 certificate.
            The pin is computed as SHA-256 hash of the public key bytes, encoded as "sha256/BASE64".
  - Parameters:
    - `certificate`: The certificate to extract the pin from.
  - Returns: The SPKI pin in sha256/BASE64 format.

### ICertificatePinningService

- **Namespace:** `SmartWorkz.Shared.Security.CertificatePinning.ICertificatePinningService`
- **Summary:** Defines the contract for certificate pinning services.
            Certificate pinning enhances security by validating that the server's certificate
            matches a pinned public key, preventing man-in-the-middle attacks even if a CA is compromised.

#### Methods & Properties

- **ValidateCertificatePin** - Validates if the provided certificate matches any pinned certificate for the given hostname.
  - Parameters:
    - `certificate`: The X.509 certificate to validate.
    - `hostName`: The hostname to validate against.
  - Returns: True if the certificate matches a pinned certificate; otherwise false.
- **AddPin** - Adds a certificate pin for the specified hostname.
  - Parameters:
    - `hostName`: The hostname to add the pin for.
    - `spkiPin`: The SPKI (Subject Public Key Info) pin in sha256/BASE64 format.
- **GetPins** - Retrieves all pins for the specified hostname.
  - Parameters:
    - `hostName`: The hostname to retrieve pins for.
  - Returns: A read-only list of pins for the hostname, or empty if no pins exist.
- **RemovePins** - Removes all pins for the specified hostname.
  - Parameters:
    - `hostName`: The hostname to remove pins for.

### CertificatePinningService

- **Namespace:** `SmartWorkz.Shared.Security.CertificatePinning.CertificatePinningService`
- **Summary:** Implements certificate pinning using SPKI (Subject Public Key Info) hashing.
            This service manages and validates certificate pins for specific hostnames,
            supporting multiple pins per hostname for certificate rotation scenarios.

#### Methods & Properties

- **AddPin** - Adds a certificate pin for the specified hostname.
            Thread-safe operation that supports multiple pins per hostname.
  - Parameters:
    - `hostName`: The hostname to add the pin for.
    - `spkiPin`: The SPKI pin in sha256/BASE64 format.
- **GetPins** - Retrieves all pins for the specified hostname.
            Returns an empty list if no pins are configured.
  - Parameters:
    - `hostName`: The hostname to retrieve pins for.
  - Returns: A read-only list of SPKI pins for the hostname.
- **RemovePins** - Removes all pins for the specified hostname.
            Thread-safe operation.
  - Parameters:
    - `hostName`: The hostname to remove pins for.
- **ValidateCertificatePin** - Validates if the provided certificate's public key matches any pinned certificate for the hostname.
            Returns false if no pins are configured for the hostname or if extraction fails.
  - Parameters:
    - `certificate`: The X.509 certificate to validate.
    - `hostName`: The hostname to validate against.
  - Returns: True if the certificate's SPKI matches a pinned certificate; otherwise false.
- **ExtractPublicKeyPin** - Extracts the SPKI (Subject Public Key Info) pin from an X.509 certificate.
            The pin is computed as SHA-256 hash of the public key bytes, encoded as "sha256/BASE64".
  - Parameters:
    - `certificate`: The certificate to extract the pin from.
  - Returns: The SPKI pin in sha256/BASE64 format.

### ICertificatePinningService

- **Namespace:** `SmartWorkz.Shared.Security.CertificatePinning.ICertificatePinningService`
- **Summary:** Defines the contract for certificate pinning services.
            Certificate pinning enhances security by validating that the server's certificate
            matches a pinned public key, preventing man-in-the-middle attacks even if a CA is compromised.

#### Methods & Properties

- **ValidateCertificatePin** - Validates if the provided certificate matches any pinned certificate for the given hostname.
  - Parameters:
    - `certificate`: The X.509 certificate to validate.
    - `hostName`: The hostname to validate against.
  - Returns: True if the certificate matches a pinned certificate; otherwise false.
- **AddPin** - Adds a certificate pin for the specified hostname.
  - Parameters:
    - `hostName`: The hostname to add the pin for.
    - `spkiPin`: The SPKI (Subject Public Key Info) pin in sha256/BASE64 format.
- **GetPins** - Retrieves all pins for the specified hostname.
  - Parameters:
    - `hostName`: The hostname to retrieve pins for.
  - Returns: A read-only list of pins for the hostname, or empty if no pins exist.
- **RemovePins** - Removes all pins for the specified hostname.
  - Parameters:
    - `hostName`: The hostname to remove pins for.

### CertificatePinningService

- **Namespace:** `SmartWorkz.Shared.Security.CertificatePinning.CertificatePinningService`
- **Summary:** Implements certificate pinning using SPKI (Subject Public Key Info) hashing.
            This service manages and validates certificate pins for specific hostnames,
            supporting multiple pins per hostname for certificate rotation scenarios.

#### Methods & Properties

- **AddPin** - Adds a certificate pin for the specified hostname.
            Thread-safe operation that supports multiple pins per hostname.
  - Parameters:
    - `hostName`: The hostname to add the pin for.
    - `spkiPin`: The SPKI pin in sha256/BASE64 format.
- **GetPins** - Retrieves all pins for the specified hostname.
            Returns an empty list if no pins are configured.
  - Parameters:
    - `hostName`: The hostname to retrieve pins for.
  - Returns: A read-only list of SPKI pins for the hostname.
- **RemovePins** - Removes all pins for the specified hostname.
            Thread-safe operation.
  - Parameters:
    - `hostName`: The hostname to remove pins for.
- **ValidateCertificatePin** - Validates if the provided certificate's public key matches any pinned certificate for the hostname.
            Returns false if no pins are configured for the hostname or if extraction fails.
  - Parameters:
    - `certificate`: The X.509 certificate to validate.
    - `hostName`: The hostname to validate against.
  - Returns: True if the certificate's SPKI matches a pinned certificate; otherwise false.
- **ExtractPublicKeyPin** - Extracts the SPKI (Subject Public Key Info) pin from an X.509 certificate.
            The pin is computed as SHA-256 hash of the public key bytes, encoded as "sha256/BASE64".
  - Parameters:
    - `certificate`: The certificate to extract the pin from.
  - Returns: The SPKI pin in sha256/BASE64 format.

### ICertificatePinningService

- **Namespace:** `SmartWorkz.Shared.Security.CertificatePinning.ICertificatePinningService`
- **Summary:** Defines the contract for certificate pinning services.
            Certificate pinning enhances security by validating that the server's certificate
            matches a pinned public key, preventing man-in-the-middle attacks even if a CA is compromised.

#### Methods & Properties

- **ValidateCertificatePin** - Validates if the provided certificate matches any pinned certificate for the given hostname.
  - Parameters:
    - `certificate`: The X.509 certificate to validate.
    - `hostName`: The hostname to validate against.
  - Returns: True if the certificate matches a pinned certificate; otherwise false.
- **AddPin** - Adds a certificate pin for the specified hostname.
  - Parameters:
    - `hostName`: The hostname to add the pin for.
    - `spkiPin`: The SPKI (Subject Public Key Info) pin in sha256/BASE64 format.
- **GetPins** - Retrieves all pins for the specified hostname.
  - Parameters:
    - `hostName`: The hostname to retrieve pins for.
  - Returns: A read-only list of pins for the hostname, or empty if no pins exist.
- **RemovePins** - Removes all pins for the specified hostname.
  - Parameters:
    - `hostName`: The hostname to remove pins for.

### CertificatePinningService

- **Namespace:** `SmartWorkz.Shared.Security.CertificatePinning.CertificatePinningService`
- **Summary:** Implements certificate pinning using SPKI (Subject Public Key Info) hashing.
            This service manages and validates certificate pins for specific hostnames,
            supporting multiple pins per hostname for certificate rotation scenarios.

#### Methods & Properties

- **AddPin** - Adds a certificate pin for the specified hostname.
            Thread-safe operation that supports multiple pins per hostname.
  - Parameters:
    - `hostName`: The hostname to add the pin for.
    - `spkiPin`: The SPKI pin in sha256/BASE64 format.
- **GetPins** - Retrieves all pins for the specified hostname.
            Returns an empty list if no pins are configured.
  - Parameters:
    - `hostName`: The hostname to retrieve pins for.
  - Returns: A read-only list of SPKI pins for the hostname.
- **RemovePins** - Removes all pins for the specified hostname.
            Thread-safe operation.
  - Parameters:
    - `hostName`: The hostname to remove pins for.
- **ValidateCertificatePin** - Validates if the provided certificate's public key matches any pinned certificate for the hostname.
            Returns false if no pins are configured for the hostname or if extraction fails.
  - Parameters:
    - `certificate`: The X.509 certificate to validate.
    - `hostName`: The hostname to validate against.
  - Returns: True if the certificate's SPKI matches a pinned certificate; otherwise false.
- **ExtractPublicKeyPin** - Extracts the SPKI (Subject Public Key Info) pin from an X.509 certificate.
            The pin is computed as SHA-256 hash of the public key bytes, encoded as "sha256/BASE64".
  - Parameters:
    - `certificate`: The certificate to extract the pin from.
  - Returns: The SPKI pin in sha256/BASE64 format.

### ICertificatePinningService

- **Namespace:** `SmartWorkz.Shared.Security.CertificatePinning.ICertificatePinningService`
- **Summary:** Defines the contract for certificate pinning services.
            Certificate pinning enhances security by validating that the server's certificate
            matches a pinned public key, preventing man-in-the-middle attacks even if a CA is compromised.

#### Methods & Properties

- **ValidateCertificatePin** - Validates if the provided certificate matches any pinned certificate for the given hostname.
  - Parameters:
    - `certificate`: The X.509 certificate to validate.
    - `hostName`: The hostname to validate against.
  - Returns: True if the certificate matches a pinned certificate; otherwise false.
- **AddPin** - Adds a certificate pin for the specified hostname.
  - Parameters:
    - `hostName`: The hostname to add the pin for.
    - `spkiPin`: The SPKI (Subject Public Key Info) pin in sha256/BASE64 format.
- **GetPins** - Retrieves all pins for the specified hostname.
  - Parameters:
    - `hostName`: The hostname to retrieve pins for.
  - Returns: A read-only list of pins for the hostname, or empty if no pins exist.
- **RemovePins** - Removes all pins for the specified hostname.
  - Parameters:
    - `hostName`: The hostname to remove pins for.

### CertificatePinningService

- **Namespace:** `SmartWorkz.Shared.Security.CertificatePinning.CertificatePinningService`
- **Summary:** Implements certificate pinning using SPKI (Subject Public Key Info) hashing.
            This service manages and validates certificate pins for specific hostnames,
            supporting multiple pins per hostname for certificate rotation scenarios.

#### Methods & Properties

- **AddPin** - Adds a certificate pin for the specified hostname.
            Thread-safe operation that supports multiple pins per hostname.
  - Parameters:
    - `hostName`: The hostname to add the pin for.
    - `spkiPin`: The SPKI pin in sha256/BASE64 format.
- **GetPins** - Retrieves all pins for the specified hostname.
            Returns an empty list if no pins are configured.
  - Parameters:
    - `hostName`: The hostname to retrieve pins for.
  - Returns: A read-only list of SPKI pins for the hostname.
- **RemovePins** - Removes all pins for the specified hostname.
            Thread-safe operation.
  - Parameters:
    - `hostName`: The hostname to remove pins for.
- **ValidateCertificatePin** - Validates if the provided certificate's public key matches any pinned certificate for the hostname.
            Returns false if no pins are configured for the hostname or if extraction fails.
  - Parameters:
    - `certificate`: The X.509 certificate to validate.
    - `hostName`: The hostname to validate against.
  - Returns: True if the certificate's SPKI matches a pinned certificate; otherwise false.
- **ExtractPublicKeyPin** - Extracts the SPKI (Subject Public Key Info) pin from an X.509 certificate.
            The pin is computed as SHA-256 hash of the public key bytes, encoded as "sha256/BASE64".
  - Parameters:
    - `certificate`: The certificate to extract the pin from.
  - Returns: The SPKI pin in sha256/BASE64 format.

### ICertificatePinningService

- **Namespace:** `SmartWorkz.Shared.Security.CertificatePinning.ICertificatePinningService`
- **Summary:** Defines the contract for certificate pinning services.
            Certificate pinning enhances security by validating that the server's certificate
            matches a pinned public key, preventing man-in-the-middle attacks even if a CA is compromised.

#### Methods & Properties

- **ValidateCertificatePin** - Validates if the provided certificate matches any pinned certificate for the given hostname.
  - Parameters:
    - `certificate`: The X.509 certificate to validate.
    - `hostName`: The hostname to validate against.
  - Returns: True if the certificate matches a pinned certificate; otherwise false.
- **AddPin** - Adds a certificate pin for the specified hostname.
  - Parameters:
    - `hostName`: The hostname to add the pin for.
    - `spkiPin`: The SPKI (Subject Public Key Info) pin in sha256/BASE64 format.
- **GetPins** - Retrieves all pins for the specified hostname.
  - Parameters:
    - `hostName`: The hostname to retrieve pins for.
  - Returns: A read-only list of pins for the hostname, or empty if no pins exist.
- **RemovePins** - Removes all pins for the specified hostname.
  - Parameters:
    - `hostName`: The hostname to remove pins for.


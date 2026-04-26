# Security API Reference

## Classes & Interfaces

### SecurityTestingUtilities

- **Namespace:** `SmartWorkz.Web.Security.SecurityTestingUtilities`
- **Summary:** Comprehensive security testing utilities for penetration testing and vulnerability assessment.
            Provides curated payloads for OWASP Top 10 vulnerabilities and validation helpers.

#### Methods & Properties

- **ValidateInputValidationService** - Validates that an input validation function correctly rejects known malicious payloads.
  - Parameters:
    - `validationFunc`: The validation function to test (returns true if invalid/malicious)
    - `payloads`: Collection of payloads to test
  - Returns: True if all payloads are correctly rejected, false otherwise
- **GenerateOWASPTestCases** - Generates comprehensive OWASP Top 10 test cases for security testing.
  - Returns: Enumerable of tuples containing (Category, Description, Payload)
- **GetPayloadStatistics** - Gets payload statistics for reporting and analysis.
  - Returns: Dictionary containing payload categories and their counts
- **GetAllPayloads** - Combines all available payloads into a single collection for comprehensive testing.
  - Returns: All payloads from all categories

### SecurityTestingUtilities

- **Namespace:** `SmartWorkz.Web.Security.SecurityTestingUtilities`
- **Summary:** Comprehensive security testing utilities for penetration testing and vulnerability assessment.
            Provides curated payloads for OWASP Top 10 vulnerabilities and validation helpers.

#### Methods & Properties

- **ValidateInputValidationService** - Validates that an input validation function correctly rejects known malicious payloads.
  - Parameters:
    - `validationFunc`: The validation function to test (returns true if invalid/malicious)
    - `payloads`: Collection of payloads to test
  - Returns: True if all payloads are correctly rejected, false otherwise
- **GenerateOWASPTestCases** - Generates comprehensive OWASP Top 10 test cases for security testing.
  - Returns: Enumerable of tuples containing (Category, Description, Payload)
- **GetPayloadStatistics** - Gets payload statistics for reporting and analysis.
  - Returns: Dictionary containing payload categories and their counts
- **GetAllPayloads** - Combines all available payloads into a single collection for comprehensive testing.
  - Returns: All payloads from all categories

### SecurityTestingUtilities

- **Namespace:** `SmartWorkz.Web.Security.SecurityTestingUtilities`
- **Summary:** Comprehensive security testing utilities for penetration testing and vulnerability assessment.
            Provides curated payloads for OWASP Top 10 vulnerabilities and validation helpers.

#### Methods & Properties

- **ValidateInputValidationService** - Validates that an input validation function correctly rejects known malicious payloads.
  - Parameters:
    - `validationFunc`: The validation function to test (returns true if invalid/malicious)
    - `payloads`: Collection of payloads to test
  - Returns: True if all payloads are correctly rejected, false otherwise
- **GenerateOWASPTestCases** - Generates comprehensive OWASP Top 10 test cases for security testing.
  - Returns: Enumerable of tuples containing (Category, Description, Payload)
- **GetPayloadStatistics** - Gets payload statistics for reporting and analysis.
  - Returns: Dictionary containing payload categories and their counts
- **GetAllPayloads** - Combines all available payloads into a single collection for comprehensive testing.
  - Returns: All payloads from all categories


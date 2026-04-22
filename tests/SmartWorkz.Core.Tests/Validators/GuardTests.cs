namespace SmartWorkz.Core.Tests.Validators;

using SmartWorkz.Core.Validators;

public class GuardTests
{
    #region Task 10 - Email, URL, Phone Tests

    #region ValidEmail Tests

    [Fact]
    public void ValidEmail_WithValidEmail_ReturnsEmail()
    {
        // Arrange
        var email = "user@example.com";

        // Act
        var result = Guard.ValidEmail(email, nameof(email));

        // Assert
        Assert.Equal(email, result);
    }

    [Fact]
    public void ValidEmail_WithValidComplexEmail_ReturnsEmail()
    {
        // Arrange
        var email = "john.doe+tag@company.co.uk";

        // Act
        var result = Guard.ValidEmail(email, nameof(email));

        // Assert
        Assert.Equal(email, result);
    }

    [Fact]
    public void ValidEmail_WithEmailContainingNumbers_ReturnsEmail()
    {
        // Arrange
        var email = "user123@domain456.com";

        // Act
        var result = Guard.ValidEmail(email, nameof(email));

        // Assert
        Assert.Equal(email, result);
    }

    [Fact]
    public void ValidEmail_WithNull_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidEmail(null!, nameof(email)));
    }

    [Fact]
    public void ValidEmail_WithEmpty_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidEmail("", nameof(email)));
    }

    [Fact]
    public void ValidEmail_WithWhitespace_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidEmail("   ", nameof(email)));
    }

    [Fact]
    public void ValidEmail_WithoutAtSymbol_ThrowsArgumentException()
    {
        // Arrange
        var email = "usernodomain.com";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidEmail(email, nameof(email)));
    }

    [Fact]
    public void ValidEmail_WithoutLocalPart_ThrowsArgumentException()
    {
        // Arrange
        var email = "@domain.com";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidEmail(email, nameof(email)));
    }

    [Fact]
    public void ValidEmail_WithoutDomain_ThrowsArgumentException()
    {
        // Arrange
        var email = "user@";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidEmail(email, nameof(email)));
    }

    [Fact]
    public void ValidEmail_WithoutTld_ThrowsArgumentException()
    {
        // Arrange
        var email = "user@domain";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidEmail(email, nameof(email)));
    }

    [Fact]
    public void ValidEmail_WithLeadingSpace_ReturnsTrimmed()
    {
        // Arrange
        var email = "  user@example.com";

        // Act
        var result = Guard.ValidEmail(email, nameof(email));

        // Assert
        Assert.Equal("user@example.com", result);
    }

    [Fact]
    public void ValidEmail_WithTrailingSpace_ReturnsTrimmed()
    {
        // Arrange
        var email = "user@example.com  ";

        // Act
        var result = Guard.ValidEmail(email, nameof(email));

        // Assert
        Assert.Equal("user@example.com", result);
    }

    [Fact]
    public void ValidEmail_WithMultipleAtSymbols_ThrowsArgumentException()
    {
        // Arrange
        var email = "user@@example.com";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidEmail(email, nameof(email)));
    }

    #endregion

    #region ValidUrl Tests

    [Fact]
    public void ValidUrl_WithValidHttpUrl_ReturnsUrl()
    {
        // Arrange
        var url = "http://example.com";

        // Act
        var result = Guard.ValidUrl(url, nameof(url));

        // Assert
        Assert.Equal(url, result);
    }

    [Fact]
    public void ValidUrl_WithValidHttpsUrl_ReturnsUrl()
    {
        // Arrange
        var url = "https://secure.example.com";

        // Act
        var result = Guard.ValidUrl(url, nameof(url));

        // Assert
        Assert.Equal(url, result);
    }

    [Fact]
    public void ValidUrl_WithPath_ReturnsUrl()
    {
        // Arrange
        var url = "https://api.example.com/v1/resource";

        // Act
        var result = Guard.ValidUrl(url, nameof(url));

        // Assert
        Assert.Equal(url, result);
    }

    [Fact]
    public void ValidUrl_WithQueryString_ReturnsUrl()
    {
        // Arrange
        var url = "https://example.com/search?q=test&lang=en";

        // Act
        var result = Guard.ValidUrl(url, nameof(url));

        // Assert
        Assert.Equal(url, result);
    }

    [Fact]
    public void ValidUrl_WithFtpScheme_ReturnsUrl()
    {
        // Arrange
        var url = "ftp://files.example.com";

        // Act
        var result = Guard.ValidUrl(url, nameof(url));

        // Assert
        Assert.Equal(url, result);
    }

    [Fact]
    public void ValidUrl_WithNull_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidUrl(null!, nameof(url)));
    }

    [Fact]
    public void ValidUrl_WithEmpty_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidUrl("", nameof(url)));
    }

    [Fact]
    public void ValidUrl_WithInvalidFormat_ThrowsArgumentException()
    {
        // Arrange
        var url = "not a url";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidUrl(url, nameof(url)));
    }

    [Fact]
    public void ValidUrl_WithMissingScheme_ThrowsArgumentException()
    {
        // Arrange
        var url = "example.com";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidUrl(url, nameof(url)));
    }

    [Fact]
    public void ValidUrl_WithLeadingSpace_ReturnsTrimmed()
    {
        // Arrange
        var url = "  https://example.com";

        // Act
        var result = Guard.ValidUrl(url, nameof(url));

        // Assert
        Assert.Equal("https://example.com", result);
    }

    #endregion

    #region ValidHttpsUrl Tests

    [Fact]
    public void ValidHttpsUrl_WithValidHttpsUrl_ReturnsUrl()
    {
        // Arrange
        var url = "https://secure.example.com";

        // Act
        var result = Guard.ValidHttpsUrl(url, nameof(url));

        // Assert
        Assert.Equal(url, result);
    }

    [Fact]
    public void ValidHttpsUrl_WithHttpUrl_ThrowsArgumentException()
    {
        // Arrange
        var url = "http://example.com";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidHttpsUrl(url, nameof(url)));
    }

    [Fact]
    public void ValidHttpsUrl_WithFtpUrl_ThrowsArgumentException()
    {
        // Arrange
        var url = "ftp://files.example.com";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidHttpsUrl(url, nameof(url)));
    }

    [Fact]
    public void ValidHttpsUrl_WithHttpsAndPath_ReturnsUrl()
    {
        // Arrange
        var url = "https://api.example.com/v1/secure/endpoint";

        // Act
        var result = Guard.ValidHttpsUrl(url, nameof(url));

        // Assert
        Assert.Equal(url, result);
    }

    [Fact]
    public void ValidHttpsUrl_WithInvalidUrl_ThrowsArgumentException()
    {
        // Arrange
        var url = "not a url";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidHttpsUrl(url, nameof(url)));
    }

    #endregion

    #region ValidPhone Tests

    [Fact]
    public void ValidPhone_WithValidPhoneNumber_ReturnsPhone()
    {
        // Arrange
        var phone = "+14155552671";

        // Act
        var result = Guard.ValidPhone(phone, nameof(phone));

        // Assert
        Assert.Equal(phone, result);
    }

    [Fact]
    public void ValidPhone_WithUsPhoneNumber_ReturnsPhone()
    {
        // Arrange
        var phone = "2025551234";

        // Act
        var result = Guard.ValidPhone(phone, nameof(phone));

        // Assert
        Assert.Equal(phone, result);
    }

    [Fact]
    public void ValidPhone_WithUkPhoneNumber_ReturnsPhone()
    {
        // Arrange
        var phone = "+442071838750";

        // Act
        var result = Guard.ValidPhone(phone, nameof(phone));

        // Assert
        Assert.Equal(phone, result);
    }

    [Fact]
    public void ValidPhone_WithPhoneContainingSpaces_ReturnsNormalizedPhone()
    {
        // Arrange
        var phone = "+1 415 555 2671";
        var expected = "+14155552671";

        // Act
        var result = Guard.ValidPhone(phone, nameof(phone));

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ValidPhone_WithPhoneContainingDashes_ReturnsNormalizedPhone()
    {
        // Arrange
        var phone = "+1-415-555-2671";
        var expected = "+14155552671";

        // Act
        var result = Guard.ValidPhone(phone, nameof(phone));

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ValidPhone_WithPhoneContainingParentheses_ReturnsNormalizedPhone()
    {
        // Arrange
        var phone = "+1 (415) 555-2671";
        var expected = "+14155552671";

        // Act
        var result = Guard.ValidPhone(phone, nameof(phone));

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ValidPhone_WithTooShortNumber_ThrowsArgumentException()
    {
        // Arrange
        var phone = "123456789";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidPhone(phone, nameof(phone)));
    }

    [Fact]
    public void ValidPhone_WithTooLongNumber_ThrowsArgumentException()
    {
        // Arrange
        var phone = "123456789012345678";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidPhone(phone, nameof(phone)));
    }

    [Fact]
    public void ValidPhone_WithLetters_ThrowsArgumentException()
    {
        // Arrange
        var phone = "202555123a";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidPhone(phone, nameof(phone)));
    }

    [Fact]
    public void ValidPhone_WithNull_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidPhone(null!, nameof(phone)));
    }

    [Fact]
    public void ValidPhone_WithEmpty_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidPhone("", nameof(phone)));
    }

    #endregion

    #region ValidPhoneE164 Tests

    [Fact]
    public void ValidPhoneE164_WithValidE164Format_ReturnsPhone()
    {
        // Arrange
        var phone = "+14155552671";

        // Act
        var result = Guard.ValidPhoneE164(phone, nameof(phone));

        // Assert
        Assert.Equal(phone, result);
    }

    [Fact]
    public void ValidPhoneE164_WithUkPhoneE164_ReturnsPhone()
    {
        // Arrange
        var phone = "+442071838750";

        // Act
        var result = Guard.ValidPhoneE164(phone, nameof(phone));

        // Assert
        Assert.Equal(phone, result);
    }

    [Fact]
    public void ValidPhoneE164_WithChinaPhoneE164_ReturnsPhone()
    {
        // Arrange
        var phone = "+8613800138000";

        // Act
        var result = Guard.ValidPhoneE164(phone, nameof(phone));

        // Assert
        Assert.Equal(phone, result);
    }

    [Fact]
    public void ValidPhoneE164_WithoutPlusPrefix_ThrowsArgumentException()
    {
        // Arrange
        var phone = "14155552671";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidPhoneE164(phone, nameof(phone)));
    }

    [Fact]
    public void ValidPhoneE164_WithCountryCodeZero_ThrowsArgumentException()
    {
        // Arrange
        var phone = "+01234567890";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidPhoneE164(phone, nameof(phone)));
    }

    [Fact]
    public void ValidPhoneE164_WithTooShortNumber_ThrowsArgumentException()
    {
        // Arrange
        var phone = "+123456";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidPhoneE164(phone, nameof(phone)));
    }

    [Fact]
    public void ValidPhoneE164_WithTooLongNumber_ThrowsArgumentException()
    {
        // Arrange
        var phone = "+123456789012345678";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidPhoneE164(phone, nameof(phone)));
    }

    [Fact]
    public void ValidPhoneE164_WithFormattingChars_ReturnsNormalizedPhone()
    {
        // Arrange
        var phone = "+1 (415) 555-2671";
        var expected = "+14155552671";

        // Act
        var result = Guard.ValidPhoneE164(phone, nameof(phone));

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ValidPhoneE164_WithNull_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidPhoneE164(null!, "phone"));
    }

    #endregion

    #region PhoneInList Tests

    [Fact]
    public void PhoneInList_WithPhoneInList_ReturnsPhone()
    {
        // Arrange
        var phone = "+14155552671";
        var allowedPhones = new[] { "+14155552671", "+442071838750" };

        // Act
        var result = Guard.PhoneInList(phone, allowedPhones, nameof(phone));

        // Assert
        Assert.Equal(phone, result);
    }

    [Fact]
    public void PhoneInList_WithFormattedPhone_ReturnsNormalizedPhone()
    {
        // Arrange
        var phone = "+1 (415) 555-2671";
        var expected = "+14155552671";
        var allowedPhones = new[] { "+14155552671", "+442071838750" };

        // Act
        var result = Guard.PhoneInList(phone, allowedPhones, nameof(phone));

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void PhoneInList_WithPhoneNotInList_ThrowsArgumentException()
    {
        // Arrange
        var phone = "+33142345678";
        var allowedPhones = new[] { "+14155552671", "+442071838750" };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.PhoneInList(phone, allowedPhones, nameof(phone)));
    }

    [Fact]
    public void PhoneInList_WithEmptyList_ThrowsArgumentException()
    {
        // Arrange
        var phone = "+14155552671";
        var allowedPhones = Array.Empty<string>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.PhoneInList(phone, allowedPhones, nameof(phone)));
    }

    [Fact]
    public void PhoneInList_WithInvalidPhone_ThrowsArgumentException()
    {
        // Arrange
        var phone = "invalid";
        var allowedPhones = new[] { "+14155552671" };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.PhoneInList(phone, allowedPhones, nameof(phone)));
    }

    #endregion

    #endregion

    #region Task 11 - Money, Regex, Enum Tests

    #region ValidMoney Tests

    [Fact]
    public void ValidMoney_WithPositiveAmount_ReturnsAmount()
    {
        // Arrange
        var amount = 99.99m;

        // Act
        var result = Guard.ValidMoney(amount, nameof(amount));

        // Assert
        Assert.Equal(amount, result);
    }

    [Fact]
    public void ValidMoney_WithZero_ReturnsAmount()
    {
        // Arrange
        var amount = 0m;

        // Act
        var result = Guard.ValidMoney(amount, nameof(amount));

        // Assert
        Assert.Equal(amount, result);
    }

    [Fact]
    public void ValidMoney_WithNegativeAmount_ThrowsArgumentException()
    {
        // Arrange
        var amount = -10m;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidMoney(amount, nameof(amount)));
    }

    [Fact]
    public void ValidMoney_WithAmountBelowMin_ThrowsArgumentException()
    {
        // Arrange
        var amount = 9m;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidMoney(amount, nameof(amount), min: 10m));
    }

    [Fact]
    public void ValidMoney_WithAmountAboveMax_ThrowsArgumentException()
    {
        // Arrange
        var amount = 150m;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidMoney(amount, nameof(amount), max: 100m));
    }

    [Fact]
    public void ValidMoney_WithAmountWithinRange_ReturnsAmount()
    {
        // Arrange
        var amount = 50m;

        // Act
        var result = Guard.ValidMoney(amount, nameof(amount), min: 10m, max: 100m);

        // Assert
        Assert.Equal(amount, result);
    }

    [Fact]
    public void ValidMoney_WithAmountAtMinBoundary_ReturnsAmount()
    {
        // Arrange
        var amount = 10m;

        // Act
        var result = Guard.ValidMoney(amount, nameof(amount), min: 10m, max: 100m);

        // Assert
        Assert.Equal(amount, result);
    }

    [Fact]
    public void ValidMoney_WithAmountAtMaxBoundary_ReturnsAmount()
    {
        // Arrange
        var amount = 100m;

        // Act
        var result = Guard.ValidMoney(amount, nameof(amount), min: 10m, max: 100m);

        // Assert
        Assert.Equal(amount, result);
    }

    [Fact]
    public void ValidMoney_WithLargeAmount_ReturnsAmount()
    {
        // Arrange
        var amount = 999999.99m;

        // Act
        var result = Guard.ValidMoney(amount, nameof(amount));

        // Assert
        Assert.Equal(amount, result);
    }

    #endregion

    #region ValidCurrency Tests

    [Fact]
    public void ValidCurrency_WithValidCurrencyCode_ReturnsCurrency()
    {
        // Arrange
        var currency = "USD";

        // Act
        var result = Guard.ValidCurrency(currency, nameof(currency));

        // Assert
        Assert.Equal("USD", result);
    }

    [Fact]
    public void ValidCurrency_WithLowercaseCurrency_ReturnsUppercase()
    {
        // Arrange
        var currency = "eur";

        // Act
        var result = Guard.ValidCurrency(currency, nameof(currency));

        // Assert
        Assert.Equal("EUR", result);
    }

    [Fact]
    public void ValidCurrency_WithValidEuroCurrency_ReturnsCurrency()
    {
        // Arrange
        var currency = "EUR";

        // Act
        var result = Guard.ValidCurrency(currency, nameof(currency));

        // Assert
        Assert.Equal("EUR", result);
    }

    [Fact]
    public void ValidCurrency_WithTooShortCode_ThrowsArgumentException()
    {
        // Arrange
        var currency = "US";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidCurrency(currency, nameof(currency)));
    }

    [Fact]
    public void ValidCurrency_WithTooLongCode_ThrowsArgumentException()
    {
        // Arrange
        var currency = "USDA";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidCurrency(currency, nameof(currency)));
    }

    [Fact]
    public void ValidCurrency_WithNumbers_ThrowsArgumentException()
    {
        // Arrange
        var currency = "US1";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidCurrency(currency, nameof(currency)));
    }

    [Fact]
    public void ValidCurrency_WithNull_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidCurrency(null!, nameof(currency)));
    }

    #endregion

    #region MatchesRegex Tests

    [Fact]
    public void MatchesRegex_WithMatchingPattern_ReturnsValue()
    {
        // Arrange
        var value = "ABC123DEF";
        var pattern = @"^[A-Z0-9]+$";

        // Act
        var result = Guard.MatchesRegex(value, pattern, nameof(value));

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void MatchesRegex_WithNonMatchingPattern_ThrowsArgumentException()
    {
        // Arrange
        var value = "ABC-123";
        var pattern = @"^[A-Z0-9]+$";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.MatchesRegex(value, pattern, nameof(value)));
    }

    [Fact]
    public void MatchesRegex_WithDatePattern_ReturnsValue()
    {
        // Arrange
        var value = "2024-03-15";
        var pattern = @"^\d{4}-\d{2}-\d{2}$";

        // Act
        var result = Guard.MatchesRegex(value, pattern, nameof(value));

        // Assert
        Assert.Equal(value, result);
    }

    [Fact]
    public void MatchesRegex_WithNull_ThrowsArgumentException()
    {
        // Arrange
        var pattern = @"^[A-Z]+$";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.MatchesRegex(null!, pattern, nameof(pattern)));
    }

    #endregion

    #region MatchesAlphanumeric Tests

    [Fact]
    public void MatchesAlphanumeric_WithLettersAndNumbers_ReturnsValue()
    {
        // Arrange
        var value = "Product123";

        // Act
        var result = Guard.MatchesAlphanumeric(value, nameof(value));

        // Assert
        Assert.Equal("Product123", result);
    }

    [Fact]
    public void MatchesAlphanumeric_WithLettersOnly_ReturnsValue()
    {
        // Arrange
        var value = "ProductName";

        // Act
        var result = Guard.MatchesAlphanumeric(value, nameof(value));

        // Assert
        Assert.Equal("ProductName", result);
    }

    [Fact]
    public void MatchesAlphanumeric_WithNumbersOnly_ReturnsValue()
    {
        // Arrange
        var value = "123456";

        // Act
        var result = Guard.MatchesAlphanumeric(value, nameof(value));

        // Assert
        Assert.Equal("123456", result);
    }

    [Fact]
    public void MatchesAlphanumeric_WithHyphen_ThrowsArgumentException()
    {
        // Arrange
        var value = "Product-123";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.MatchesAlphanumeric(value, nameof(value)));
    }

    [Fact]
    public void MatchesAlphanumeric_WithSpace_ThrowsArgumentException()
    {
        // Arrange
        var value = "Product 123";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.MatchesAlphanumeric(value, nameof(value)));
    }

    [Fact]
    public void MatchesAlphanumeric_WithSpecialChar_ThrowsArgumentException()
    {
        // Arrange
        var value = "Product@123";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.MatchesAlphanumeric(value, nameof(value)));
    }

    #endregion

    #region MatchesAlphanumericWithHyphens Tests

    [Fact]
    public void MatchesAlphanumericWithHyphens_WithValidFormat_ReturnsValue()
    {
        // Arrange
        var value = "Product-123-ABC";

        // Act
        var result = Guard.MatchesAlphanumericWithHyphens(value, nameof(value));

        // Assert
        Assert.Equal("Product-123-ABC", result);
    }

    [Fact]
    public void MatchesAlphanumericWithHyphens_WithoutHyphens_ReturnsValue()
    {
        // Arrange
        var value = "Product123ABC";

        // Act
        var result = Guard.MatchesAlphanumericWithHyphens(value, nameof(value));

        // Assert
        Assert.Equal("Product123ABC", result);
    }

    [Fact]
    public void MatchesAlphanumericWithHyphens_WithSpace_ThrowsArgumentException()
    {
        // Arrange
        var value = "Product-123 ABC";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.MatchesAlphanumericWithHyphens(value, nameof(value)));
    }

    [Fact]
    public void MatchesAlphanumericWithHyphens_WithUnderscore_ThrowsArgumentException()
    {
        // Arrange
        var value = "Product_123";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.MatchesAlphanumericWithHyphens(value, nameof(value)));
    }

    #endregion

    #region ValidEnum Tests

    [Fact]
    public void ValidEnum_WithDefinedEnumValue_ReturnsValue()
    {
        // Arrange
        var state = EntityState.Active;

        // Act
        var result = Guard.ValidEnum(state, nameof(state));

        // Assert
        Assert.Equal(EntityState.Active, result);
    }

    [Fact]
    public void ValidEnum_WithAnotherDefinedValue_ReturnsValue()
    {
        // Arrange
        var state = EntityState.Approved;

        // Act
        var result = Guard.ValidEnum(state, nameof(state));

        // Assert
        Assert.Equal(EntityState.Approved, result);
    }

    [Fact]
    public void ValidEnum_WithUndefinedValue_ThrowsArgumentException()
    {
        // Arrange
        var state = (EntityState)999;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidEnum(state, nameof(state)));
    }

    #endregion

    #region ValidStateForEntity Tests

    [Fact]
    public void ValidStateForEntity_WithAllowedState_ReturnsState()
    {
        // Arrange
        var state = EntityState.OrderPlaced;
        var allowedStates = new HashSet<EntityState> { EntityState.OrderPlaced, EntityState.OrderConfirmed };

        // Act
        var result = Guard.ValidStateForEntity(state, nameof(state), allowedStates);

        // Assert
        Assert.Equal(EntityState.OrderPlaced, result);
    }

    [Fact]
    public void ValidStateForEntity_WithDisallowedState_ThrowsArgumentException()
    {
        // Arrange
        var state = EntityState.Delivered;
        var allowedStates = new HashSet<EntityState> { EntityState.OrderPlaced, EntityState.OrderConfirmed };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidStateForEntity(state, nameof(state), allowedStates));
    }

    [Fact]
    public void ValidStateForEntity_WithEmptyAllowedSet_ThrowsArgumentException()
    {
        // Arrange
        var state = EntityState.Active;
        var allowedStates = new HashSet<EntityState>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidStateForEntity(state, nameof(state), allowedStates));
    }

    [Fact]
    public void ValidStateForEntity_WithMultipleAllowedStates_ReturnsStateWhenFound()
    {
        // Arrange
        var state = EntityState.Shipped;
        var allowedStates = new HashSet<EntityState>
        {
            EntityState.OrderConfirmed,
            EntityState.Shipped,
            EntityState.Delivered
        };

        // Act
        var result = Guard.ValidStateForEntity(state, nameof(state), allowedStates);

        // Assert
        Assert.Equal(EntityState.Shipped, result);
    }

    #endregion

    #endregion

    #region Task 12 - Pagination & Text Tests

    #region ValidPageSize Tests

    [Fact]
    public void ValidPageSize_WithValidSize_ReturnsSize()
    {
        // Arrange
        var pageSize = 10;

        // Act
        var result = Guard.ValidPageSize(pageSize, nameof(pageSize));

        // Assert
        Assert.Equal(10, result);
    }

    [Fact]
    public void ValidPageSize_WithSizeOne_ReturnsSize()
    {
        // Arrange
        var pageSize = 1;

        // Act
        var result = Guard.ValidPageSize(pageSize, nameof(pageSize));

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void ValidPageSize_WithMaxSize_ReturnsSize()
    {
        // Arrange
        var pageSize = 100;

        // Act
        var result = Guard.ValidPageSize(pageSize, nameof(pageSize));

        // Assert
        Assert.Equal(100, result);
    }

    [Fact]
    public void ValidPageSize_WithSizeBelowMin_ThrowsArgumentException()
    {
        // Arrange
        var pageSize = 0;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidPageSize(pageSize, nameof(pageSize)));
    }

    [Fact]
    public void ValidPageSize_WithSizeAboveMax_ThrowsArgumentException()
    {
        // Arrange
        var pageSize = 150;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidPageSize(pageSize, nameof(pageSize)));
    }

    [Fact]
    public void ValidPageSize_WithCustomMin_ReturnsSize()
    {
        // Arrange
        var pageSize = 5;

        // Act
        var result = Guard.ValidPageSize(pageSize, nameof(pageSize), min: 5, max: 200);

        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public void ValidPageSize_WithCustomMax_ReturnsSize()
    {
        // Arrange
        var pageSize = 500;

        // Act
        var result = Guard.ValidPageSize(pageSize, nameof(pageSize), min: 1, max: 1000);

        // Assert
        Assert.Equal(500, result);
    }

    [Fact]
    public void ValidPageSize_WithCustomMinBelowMin_ThrowsArgumentException()
    {
        // Arrange
        var pageSize = 4;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidPageSize(pageSize, nameof(pageSize), min: 5, max: 200));
    }

    [Fact]
    public void ValidPageSize_WithCustomMaxAboveMax_ThrowsArgumentException()
    {
        // Arrange
        var pageSize = 1001;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidPageSize(pageSize, nameof(pageSize), min: 1, max: 1000));
    }

    #endregion

    #region ValidPageNumber Tests

    [Fact]
    public void ValidPageNumber_WithValidPageNumber_ReturnsNumber()
    {
        // Arrange
        var pageNumber = 1;

        // Act
        var result = Guard.ValidPageNumber(pageNumber, nameof(pageNumber));

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void ValidPageNumber_WithLargePageNumber_ReturnsNumber()
    {
        // Arrange
        var pageNumber = 999;

        // Act
        var result = Guard.ValidPageNumber(pageNumber, nameof(pageNumber));

        // Assert
        Assert.Equal(999, result);
    }

    [Fact]
    public void ValidPageNumber_WithZero_ThrowsArgumentException()
    {
        // Arrange
        var pageNumber = 0;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidPageNumber(pageNumber, nameof(pageNumber)));
    }

    [Fact]
    public void ValidPageNumber_WithNegativeNumber_ThrowsArgumentException()
    {
        // Arrange
        var pageNumber = -1;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.ValidPageNumber(pageNumber, nameof(pageNumber)));
    }

    #endregion

    #region NoExtraWhitespace Tests

    [Fact]
    public void NoExtraWhitespace_WithValidText_ReturnsText()
    {
        // Arrange
        var text = "John Smith";

        // Act
        var result = Guard.NoExtraWhitespace(text, nameof(text));

        // Assert
        Assert.Equal("John Smith", result);
    }

    [Fact]
    public void NoExtraWhitespace_WithSingleWord_ReturnsText()
    {
        // Arrange
        var text = "Hello";

        // Act
        var result = Guard.NoExtraWhitespace(text, nameof(text));

        // Assert
        Assert.Equal("Hello", result);
    }

    [Fact]
    public void NoExtraWhitespace_WithLeadingSpace_ReturnsTrimmed()
    {
        // Arrange
        var text = "  John Smith";

        // Act
        var result = Guard.NoExtraWhitespace(text, nameof(text));

        // Assert
        Assert.Equal("John Smith", result);
    }

    [Fact]
    public void NoExtraWhitespace_WithTrailingSpace_ReturnsTrimmed()
    {
        // Arrange
        var text = "John Smith  ";

        // Act
        var result = Guard.NoExtraWhitespace(text, nameof(text));

        // Assert
        Assert.Equal("John Smith", result);
    }

    [Fact]
    public void NoExtraWhitespace_WithDoubleSpace_ThrowsArgumentException()
    {
        // Arrange
        var text = "John  Smith";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.NoExtraWhitespace(text, nameof(text)));
    }

    [Fact]
    public void NoExtraWhitespace_WithTripleSpace_ThrowsArgumentException()
    {
        // Arrange
        var text = "John   Smith";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.NoExtraWhitespace(text, nameof(text)));
    }

    [Fact]
    public void NoExtraWhitespace_WithNull_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.NoExtraWhitespace(null!, nameof(text)));
    }

    [Fact]
    public void NoExtraWhitespace_WithEmpty_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.NoExtraWhitespace("", nameof(text)));
    }

    #endregion

    #region LengthBetween Tests

    [Fact]
    public void LengthBetween_WithValidLength_ReturnsText()
    {
        // Arrange
        var text = "John";

        // Act
        var result = Guard.LengthBetween(text, 1, 256, nameof(text));

        // Assert
        Assert.Equal("John", result);
    }

    [Fact]
    public void LengthBetween_WithMinLength_ReturnsText()
    {
        // Arrange
        var text = "J";

        // Act
        var result = Guard.LengthBetween(text, 1, 256, nameof(text));

        // Assert
        Assert.Equal("J", result);
    }

    [Fact]
    public void LengthBetween_WithMaxLength_ReturnsText()
    {
        // Arrange
        var text = new string('a', 256);

        // Act
        var result = Guard.LengthBetween(text, 1, 256, nameof(text));

        // Assert
        Assert.Equal(256, result.Length);
    }

    [Fact]
    public void LengthBetween_WithTextBelowMin_ThrowsArgumentException()
    {
        // Arrange
        var text = "Jo";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.LengthBetween(text, 3, 256, nameof(text)));
    }

    [Fact]
    public void LengthBetween_WithTextAboveMax_ThrowsArgumentException()
    {
        // Arrange
        var text = new string('a', 257);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.LengthBetween(text, 1, 256, nameof(text)));
    }

    [Fact]
    public void LengthBetween_WithEmptyText_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.LengthBetween("", 1, 256, nameof(text)));
    }

    [Fact]
    public void LengthBetween_WithNull_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.LengthBetween(null!, 1, 256, nameof(text)));
    }

    [Fact]
    public void LengthBetween_WithLeadingSpace_TrimsThenValidates()
    {
        // Arrange
        var text = "  John  ";

        // Act
        var result = Guard.LengthBetween(text, 1, 256, nameof(text));

        // Assert
        Assert.Equal("John", result);
    }

    #endregion

    #region SanitizeText Tests

    [Fact]
    public void SanitizeText_WithValidText_ReturnsText()
    {
        // Arrange
        var text = "John Smith";

        // Act
        var result = Guard.SanitizeText(text);

        // Assert
        Assert.Equal("John Smith", result);
    }

    [Fact]
    public void SanitizeText_WithLeadingSpace_ReturnsTrimmed()
    {
        // Arrange
        var text = "  John Smith";

        // Act
        var result = Guard.SanitizeText(text);

        // Assert
        Assert.Equal("John Smith", result);
    }

    [Fact]
    public void SanitizeText_WithTrailingSpace_ReturnsTrimmed()
    {
        // Arrange
        var text = "John Smith  ";

        // Act
        var result = Guard.SanitizeText(text);

        // Assert
        Assert.Equal("John Smith", result);
    }

    [Fact]
    public void SanitizeText_WithDoubleSpace_ReturnsCollapsed()
    {
        // Arrange
        var text = "John  Smith";

        // Act
        var result = Guard.SanitizeText(text);

        // Assert
        Assert.Equal("John Smith", result);
    }

    [Fact]
    public void SanitizeText_WithTripleSpace_ReturnsCollapsed()
    {
        // Arrange
        var text = "John   Smith";

        // Act
        var result = Guard.SanitizeText(text);

        // Assert
        Assert.Equal("John Smith", result);
    }

    [Fact]
    public void SanitizeText_WithMixedWhitespace_ReturnsNormalized()
    {
        // Arrange
        var text = "  John   Smith   Doe  ";

        // Act
        var result = Guard.SanitizeText(text);

        // Assert
        Assert.Equal("John Smith Doe", result);
    }

    [Fact]
    public void SanitizeText_WithNull_ReturnsEmpty()
    {
        // Arrange
        var text = (string)null!;

        // Act
        var result = Guard.SanitizeText(text);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void SanitizeText_WithEmpty_ReturnsEmpty()
    {
        // Act
        var result = Guard.SanitizeText("");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void SanitizeText_WithWhitespaceOnly_ReturnsEmpty()
    {
        // Act
        var result = Guard.SanitizeText("   ");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void SanitizeText_WithTabs_ReturnsTrimmedAndCollapsed()
    {
        // Arrange
        var text = "\tJohn\t\tSmith\t";

        // Act
        var result = Guard.SanitizeText(text);

        // Assert
        Assert.Equal("John Smith", result);
    }

    [Fact]
    public void SanitizeText_WithNewlines_ReturnsTrimmedAndCollapsed()
    {
        // Arrange
        var text = "\nJohn\n\nSmith\n";

        // Act
        var result = Guard.SanitizeText(text);

        // Assert
        Assert.Equal("John Smith", result);
    }

    #endregion

    #endregion
}

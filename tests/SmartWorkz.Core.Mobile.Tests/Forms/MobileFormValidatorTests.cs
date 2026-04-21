namespace SmartWorkz.Mobile.Tests.Forms;

using SmartWorkz.Shared;

public class MobileFormValidatorTests
{
    private record TestModel(string Name, string Email);

    private class TestValidator : ValidatorBase<TestModel>
    {
        public TestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaxLength(50);
            RuleFor(x => x.Email).NotEmpty()
                .Custom(e => Task.FromResult(e.Contains('@')), "Invalid email");
        }
    }

    [Fact]
    public async Task ValidateAsync_ValidModel_IsValidTrue()
    {
        var validator = new MobileFormValidator<TestModel>(new TestValidator());
        var model = new TestModel("Alice", "alice@test.com");

        var result = await validator.ValidateAsync(model);

        Assert.True(result);
        Assert.True(validator.IsValid);
        Assert.Empty(validator.FieldErrors);
    }

    [Fact]
    public async Task ValidateAsync_InvalidModel_IsValidFalse_WithErrors()
    {
        var validator = new MobileFormValidator<TestModel>(new TestValidator());
        var model = new TestModel("", "not-an-email");

        var result = await validator.ValidateAsync(model);

        Assert.False(result);
        Assert.False(validator.IsValid);
        Assert.True(validator.FieldErrors.Count > 0);
    }

    [Fact]
    public async Task GetError_ReturnsMessageForField()
    {
        var validator = new MobileFormValidator<TestModel>(new TestValidator());
        await validator.ValidateAsync(new TestModel("", "not-an-email"));

        var err = validator.GetError(nameof(TestModel.Name));

        Assert.NotNull(err);
        Assert.NotEmpty(err!);
    }

    [Fact]
    public async Task GetError_UnknownField_ReturnsNull()
    {
        var validator = new MobileFormValidator<TestModel>(new TestValidator());
        await validator.ValidateAsync(new TestModel("Alice", "alice@test.com"));

        Assert.Null(validator.GetError("NonExistent"));
    }
}

using FractalDataWorks.Configuration;
using FluentValidation;
using FluentValidation.Results;

namespace FractalDataWorks.Configuration.Tests;

public class ConfigurationBaseTests
{
    private class TestConfiguration : ConfigurationBase<TestConfiguration>
    {
        public override string SectionName => "TestSection";

        public string TestProperty { get; set; } = string.Empty;
    }

    private class TestConfigurationWithValidator : ConfigurationBase<TestConfigurationWithValidator>
    {
        public override string SectionName => "TestWithValidatorSection";

        public string RequiredProperty { get; set; } = string.Empty;

        protected override IValidator<TestConfigurationWithValidator>? GetValidator()
        {
            return new TestValidator();
        }

        private class TestValidator : AbstractValidator<TestConfigurationWithValidator>
        {
            public TestValidator()
            {
                RuleFor(x => x.RequiredProperty)
                    .NotEmpty()
                    .WithMessage("RequiredProperty cannot be empty");
            }
        }
    }

    [Fact]
    public void Constructor_ShouldInitializeDefaultValues()
    {
        // Act
        var config = new TestConfiguration();

        // Assert
        config.Id.ShouldBe(0);
        config.Name.ShouldBe(string.Empty);
        config.IsEnabled.ShouldBeTrue();
        config.CreatedAt.ShouldNotBe(default);
        config.CreatedAt.Kind.ShouldBe(DateTimeKind.Utc);
        config.ModifiedAt.ShouldBeNull();
    }

    [Fact]
    public void Validate_WithNoValidator_ReturnsSuccess()
    {
        // Arrange
        var config = new TestConfiguration();

        // Act
        var result = config.Validate();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WithValidator_ValidConfiguration_ReturnsSuccess()
    {
        // Arrange
        var config = new TestConfigurationWithValidator
        {
            RequiredProperty = "Valid"
        };

        // Act
        var result = config.Validate();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.IsValid.ShouldBeTrue();
        result.Value.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void Validate_WithValidator_InvalidConfiguration_ReturnsFailedValidation()
    {
        // Arrange
        var config = new TestConfigurationWithValidator
        {
            RequiredProperty = "" // Invalid - empty
        };

        // Act
        var result = config.Validate();

        // Assert
        result.IsSuccess.ShouldBeTrue(); // Result operation succeeds
        result.Value.IsValid.ShouldBeFalse(); // But validation fails
        result.Value.Errors.Count.ShouldBe(1);
        result.Value.Errors[0].ErrorMessage.ShouldBe("RequiredProperty cannot be empty");
    }

    [Fact]
    public void MarkAsModified_ShouldSetModifiedAt()
    {
        // Arrange
        var config = new TestConfigurationWithMarkAsModifiedExposed();
        var beforeMark = DateTime.UtcNow;

        // Act
        config.CallMarkAsModified();

        // Assert
        config.ModifiedAt.ShouldNotBeNull();
        config.ModifiedAt.Value.ShouldBeGreaterThanOrEqualTo(beforeMark);
        config.ModifiedAt.Value.Kind.ShouldBe(DateTimeKind.Utc);
    }

    // Helper class to expose MarkAsModified for testing
    private class TestConfigurationWithMarkAsModifiedExposed : ConfigurationBase<TestConfigurationWithMarkAsModifiedExposed>
    {
        public override string SectionName => "TestSection";

        public void CallMarkAsModified() => MarkAsModified();
    }
}

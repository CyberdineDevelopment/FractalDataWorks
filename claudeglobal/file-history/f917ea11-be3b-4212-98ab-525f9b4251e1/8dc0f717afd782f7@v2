using System;
using FractalDataWorks.Messages;

namespace FractalDataWorks.Messages.Tests;

/// <summary>
/// Tests for ValidationMessage class.
/// </summary>
public class ValidationMessageTests
{
    [Fact]
    public void Constructor_InitializesWithValidationError()
    {
        var validationError = "Email address is invalid";

        var message = new ValidationMessage(validationError);

        message.ValidationError.ShouldBe(validationError);
        message.Id.ShouldBe(1004);
        message.Name.ShouldBe("Validation");
        message.Severity.ShouldBe(MessageSeverity.Error);
        message.Message.ShouldBe(validationError);
        message.Code.ShouldBe("VALIDATION");
        message.Source.ShouldBe("Validation");
    }

    [Theory]
    [InlineData("Field is required")]
    [InlineData("Value must be greater than zero")]
    [InlineData("Invalid format")]
    public void Constructor_WithVariousErrors_CreatesCorrectMessage(string validationError)
    {
        var message = new ValidationMessage(validationError);

        message.ValidationError.ShouldBe(validationError);
        message.Message.ShouldBe(validationError);
    }
}

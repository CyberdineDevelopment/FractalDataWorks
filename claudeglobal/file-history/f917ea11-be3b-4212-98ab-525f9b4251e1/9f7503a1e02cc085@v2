using System;
using FractalDataWorks.Messages;

namespace FractalDataWorks.Messages.Tests;

/// <summary>
/// Tests for ErrorMessage class.
/// </summary>
public class ErrorMessageTests
{
    [Fact]
    public void Constructor_InitializesWithErrorDescription()
    {
        var errorDesc = "An error has occurred";

        var message = new ErrorMessage(errorDesc);

        message.ErrorDescription.ShouldBe(errorDesc);
        message.Id.ShouldBe(1003);
        message.Name.ShouldBe("Error");
        message.Severity.ShouldBe(MessageSeverity.Error);
        message.Message.ShouldBe(errorDesc);
        message.Code.ShouldBe("ERROR");
        message.Source.ShouldBe("General");
    }

    [Theory]
    [InlineData("Database connection failed")]
    [InlineData("Invalid operation")]
    [InlineData("System error")]
    public void Constructor_WithVariousErrors_CreatesCorrectMessage(string errorDescription)
    {
        var message = new ErrorMessage(errorDescription);

        message.ErrorDescription.ShouldBe(errorDescription);
        message.Message.ShouldBe(errorDescription);
    }
}

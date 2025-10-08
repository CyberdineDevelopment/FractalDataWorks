using System;
using FractalDataWorks.Messages;

namespace FractalDataWorks.Messages.Tests;

/// <summary>
/// Tests for ArgumentNullMessage class.
/// </summary>
public class ArgumentNullMessageTests
{
    [Fact]
    public void Constructor_InitializesWithParameterName()
    {
        var paramName = "testParameter";

        var message = new ArgumentNullMessage(paramName);

        message.ParameterName.ShouldBe(paramName);
        message.Id.ShouldBe(1001);
        message.Name.ShouldBe("ArgumentNull");
        message.Severity.ShouldBe(MessageSeverity.Error);
        message.Message.ShouldBe($"Parameter '{paramName}' cannot be null");
        message.Code.ShouldBe("ARG_NULL");
        message.Source.ShouldBe("ArgumentValidation");
    }

    [Theory]
    [InlineData("id")]
    [InlineData("configuration")]
    [InlineData("serviceFactory")]
    public void Constructor_WithVariousParameterNames_CreatesCorrectMessage(string parameterName)
    {
        var message = new ArgumentNullMessage(parameterName);

        message.ParameterName.ShouldBe(parameterName);
        message.Message.ShouldContain(parameterName);
    }
}

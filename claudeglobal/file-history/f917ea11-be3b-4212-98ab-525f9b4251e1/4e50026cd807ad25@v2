using System;
using FractalDataWorks.Messages;

namespace FractalDataWorks.Messages.Tests;

/// <summary>
/// Tests for NotFoundMessage class.
/// </summary>
public class NotFoundMessageTests
{
    [Fact]
    public void Constructor_InitializesWithResourceDescription()
    {
        var resourceDesc = "User with ID 123 not found";

        var message = new NotFoundMessage(resourceDesc);

        message.ResourceDescription.ShouldBe(resourceDesc);
        message.Id.ShouldBe(1002);
        message.Name.ShouldBe("NotFound");
        message.Severity.ShouldBe(MessageSeverity.Error);
        message.Message.ShouldBe(resourceDesc);
        message.Code.ShouldBe("NOT_FOUND");
        message.Source.ShouldBe("ResourceLookup");
    }

    [Theory]
    [InlineData("Configuration not found")]
    [InlineData("File does not exist")]
    [InlineData("Resource unavailable")]
    public void Constructor_WithVariousResources_CreatesCorrectMessage(string resourceDescription)
    {
        var message = new NotFoundMessage(resourceDescription);

        message.ResourceDescription.ShouldBe(resourceDescription);
        message.Message.ShouldBe(resourceDescription);
    }
}

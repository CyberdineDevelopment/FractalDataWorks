using System;
using FractalDataWorks.Messages;

namespace FractalDataWorks.Messages.Tests;

/// <summary>
/// Tests for NotImplementedMessage class.
/// </summary>
public class NotImplementedMessageTests
{
    [Fact]
    public void Constructor_InitializesWithFeatureDescription()
    {
        var featureDesc = "Advanced search feature";

        var message = new NotImplementedMessage(featureDesc);

        message.FeatureDescription.ShouldBe(featureDesc);
        message.Id.ShouldBe(1005);
        message.Name.ShouldBe("NotImplemented");
        message.Severity.ShouldBe(MessageSeverity.Error);
        message.Message.ShouldBe(featureDesc);
        message.Code.ShouldBe("NOT_IMPLEMENTED");
        message.Source.ShouldBe("Implementation");
    }

    [Theory]
    [InlineData("Export to PDF")]
    [InlineData("Multi-factor authentication")]
    [InlineData("Real-time notifications")]
    public void Constructor_WithVariousFeatures_CreatesCorrectMessage(string featureDescription)
    {
        var message = new NotImplementedMessage(featureDescription);

        message.FeatureDescription.ShouldBe(featureDescription);
        message.Message.ShouldBe(featureDescription);
    }
}

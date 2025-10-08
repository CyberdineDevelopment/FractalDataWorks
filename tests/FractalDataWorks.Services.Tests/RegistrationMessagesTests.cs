using FractalDataWorks.Messages;
using FractalDataWorks.Services.Messages;

namespace FractalDataWorks.Services.Tests;

/// <summary>
/// Tests for registration message classes for 100% coverage.
/// </summary>
public class RegistrationMessagesTests
{
    [Fact]
    public void ConfigurationRegistryNotFoundMessage_DefaultConstructor_CreatesMessage()
    {
        var message = new ConfigurationRegistryNotFoundMessage();

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void ConfigurationRegistryNotFoundMessage_WithConfigurationType_CreatesMessage()
    {
        var message = new ConfigurationRegistryNotFoundMessage("TestConfiguration");

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }
}

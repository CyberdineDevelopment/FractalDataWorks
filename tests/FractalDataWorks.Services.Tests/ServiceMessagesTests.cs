using FractalDataWorks.Messages;
using FractalDataWorks.Services.Messages;

namespace FractalDataWorks.Services.Tests;

/// <summary>
/// Tests for service message classes for 100% coverage.
/// </summary>
public class ServiceMessagesTests
{
    [Fact]
    public void ConfigurationCannotBeNullMessage_DefaultConstructor_CreatesMessage()
    {
        var message = new ConfigurationCannotBeNullMessage();

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void ConfigurationCannotBeNullMessage_WithParameterName_CreatesMessage()
    {
        var message = new ConfigurationCannotBeNullMessage("testParam");

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void ConfigurationCannotBeNullMessage_WithParameterAndSection_CreatesMessage()
    {
        var message = new ConfigurationCannotBeNullMessage("testParam", "testSection");

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void ConfigurationNotInitializedMessage_DefaultConstructor_CreatesMessage()
    {
        var message = new ConfigurationNotInitializedMessage();

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void ConfigurationNotInitializedMessage_WithCustomMessage_CreatesMessage()
    {
        var message = new ConfigurationNotInitializedMessage("Custom error");

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void ServiceCreationFailedMessage_DefaultConstructor_CreatesMessage()
    {
        var message = new ServiceCreationFailedMessage();

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void ServiceCreationFailedMessage_WithServiceName_CreatesMessage()
    {
        var message = new ServiceCreationFailedMessage("TestService");

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void ServiceCreationFailedMessage_WithServiceNameAndReason_CreatesMessage()
    {
        var message = new ServiceCreationFailedMessage("TestService", "No constructor found");

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void FactoryNullMessage_DefaultConstructor_CreatesMessage()
    {
        var message = new FactoryNullMessage();

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void ServiceTypeNameNullOrEmptyMessage_DefaultConstructor_CreatesMessage()
    {
        var message = new ServiceTypeNameNullOrEmptyMessage();

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void RegistrationFailedMessage_DefaultConstructor_CreatesMessage()
    {
        var message = new RegistrationFailedMessage();

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void RegistrationFailedMessage_WithServiceTypeAndReason_CreatesMessage()
    {
        var message = new RegistrationFailedMessage("TestService", "Duplicate registration");

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void ServiceRegisteredMessage_DefaultConstructor_CreatesMessage()
    {
        var message = new ServiceRegisteredMessage();

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Information);
    }

    [Fact]
    public void ServiceRegisteredMessage_WithServiceTypeAndLifetime_CreatesMessage()
    {
        var message = new ServiceRegisteredMessage("TestService", "Singleton");

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Information);
    }

    [Fact]
    public void InvalidCommandMessage_DefaultConstructor_CreatesMessage()
    {
        var message = new InvalidCommandMessage();

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void InvalidCommandTypeMessage_DefaultConstructor_CreatesMessage()
    {
        var message = new InvalidCommandTypeMessage();

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void ValidationFailedMessage_DefaultConstructor_CreatesMessage()
    {
        var message = new ValidationFailedMessage();

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void ConfigurationNotFoundMessage_DefaultConstructor_CreatesMessage()
    {
        var message = new ConfigurationNotFoundMessage();

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void RecordNotFoundMessage_DefaultConstructor_CreatesMessage()
    {
        var message = new RecordNotFoundMessage();

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Warning);
    }

    [Fact]
    public void NoServiceTypesRegisteredMessage_DefaultConstructor_CreatesMessage()
    {
        var message = new NoServiceTypesRegisteredMessage();

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void ServiceTypeUnknownMessage_DefaultConstructor_CreatesMessage()
    {
        var message = new ServiceTypeUnknownMessage();

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }
}

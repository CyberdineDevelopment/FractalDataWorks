using FractalDataWorks.Messages;
using FractalDataWorks.Services.Messages;

namespace FractalDataWorks.Services.Tests;

/// <summary>
/// Tests for factory message classes for 100% coverage.
/// </summary>
public class FactoryMessagesTests
{
    [Fact]
    public void CouldNotCreateObjectMessage_DefaultConstructor_CreatesMessage()
    {
        var message = new CouldNotCreateObjectMessage();

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void CouldNotCreateObjectMessage_WithObjectType_CreatesMessage()
    {
        var message = new CouldNotCreateObjectMessage("TestType");

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void CouldNotCreateObjectMessage_WithObjectTypeAndReason_CreatesMessage()
    {
        var message = new CouldNotCreateObjectMessage("TestType", "Missing constructor");

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void LifetimeNullMessage_DefaultConstructor_CreatesMessage()
    {
        var message = new LifetimeNullMessage();

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void ServiceCreatedSuccessfullyMessage_DefaultConstructor_CreatesMessage()
    {
        var message = new ServiceCreatedSuccessfullyMessage();

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Information);
    }

    [Fact]
    public void ServiceCreatedSuccessfullyMessage_WithServiceType_CreatesMessage()
    {
        var message = new ServiceCreatedSuccessfullyMessage("TestService");

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Information);
    }

    [Fact]
    public void ServiceTypeCastFailedMessage_DefaultConstructor_CreatesMessage()
    {
        var message = new ServiceTypeCastFailedMessage();

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void ServiceTypeCastFailedMessage_WithSourceAndTargetTypes_CreatesMessage()
    {
        var message = new ServiceTypeCastFailedMessage("SourceType", "TargetType");

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void FastGenericCreationFailedMessage_DefaultConstructor_CreatesMessage()
    {
        var message = new FastGenericCreationFailedMessage();

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void FastGenericCreationFailedMessage_WithServiceType_CreatesMessage()
    {
        var message = new FastGenericCreationFailedMessage("TestService");

        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }
}

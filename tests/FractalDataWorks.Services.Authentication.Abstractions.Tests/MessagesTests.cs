using FractalDataWorks.Services.Authentication.Abstractions.Messages;
using FractalDataWorks.Messages;
using Shouldly;

namespace FractalDataWorks.Services.Authentication.Abstractions.Tests;

public sealed class MessagesTests
{
    [Fact]
    public void AuthenticationFailedMessage_ShouldHaveCorrectProperties()
    {
        // Act
        var message = new AuthenticationFailedMessage();

        // Assert
        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void AuthenticationTypeNotSpecifiedMessage_ShouldHaveCorrectProperties()
    {
        // Act
        var message = new AuthenticationTypeNotSpecifiedMessage("TestConfig");

        // Assert
        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void CommandExecutionNotSupportedMessage_ShouldHaveCorrectProperties()
    {
        // Act
        var message = new CommandExecutionNotSupportedMessage();

        // Assert
        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void ConfigurationBindingFailedMessage_ShouldHaveCorrectProperties()
    {
        // Act
        var message = new ConfigurationBindingFailedMessage("TestConfigType");

        // Assert
        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void ConfigurationNameNullMessage_ShouldHaveCorrectProperties()
    {
        // Act
        var message = new ConfigurationNameNullMessage();

        // Assert
        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void ConfigurationNullMessage_ShouldHaveCorrectProperties()
    {
        // Act
        var message = new ConfigurationNullMessage();

        // Assert
        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void ConfigurationSectionNotFoundMessage_ShouldHaveCorrectProperties()
    {
        // Act
        var message = new ConfigurationSectionNotFoundMessage("TestSection");

        // Assert
        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void InvalidTokenMessage_ShouldHaveCorrectProperties()
    {
        // Act
        var message = new InvalidTokenMessage();

        // Assert
        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void NoFactoryRegisteredMessage_ShouldHaveCorrectProperties()
    {
        // Act
        var message = new NoFactoryRegisteredMessage("TestAuthType");

        // Assert
        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void RefreshTokenInvalidMessage_ShouldHaveCorrectProperties()
    {
        // Act
        var message = new RefreshTokenInvalidMessage();

        // Assert
        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void ServiceCreationExceptionMessage_ShouldHaveCorrectProperties()
    {
        // Act
        var message = new ServiceCreationExceptionMessage("Test exception");

        // Assert
        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void TokenExpiredMessage_ShouldHaveCorrectProperties()
    {
        // Act
        var message = new TokenExpiredMessage();

        // Assert
        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void TokenNullOrEmptyMessage_ShouldHaveCorrectProperties()
    {
        // Act
        var message = new TokenNullOrEmptyMessage();

        // Assert
        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void TokenRevocationFailedMessage_ShouldHaveCorrectProperties()
    {
        // Act
        var message = new TokenRevocationFailedMessage("Revocation error");

        // Assert
        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void UnknownAuthenticationTypeMessage_ShouldHaveCorrectProperties()
    {
        // Act
        var message = new UnknownAuthenticationTypeMessage("UnknownType");

        // Assert
        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void AuthenticationMessageCollectionBase_ShouldBeAccessible()
    {
        // Act
        var collection = AuthenticationMessageCollectionBase.All();

        // Assert
        collection.ShouldNotBeEmpty();
    }
}

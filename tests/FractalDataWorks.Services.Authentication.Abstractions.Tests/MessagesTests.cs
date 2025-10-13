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
        message.Category.ShouldBe("Authentication");
    }

    [Fact]
    public void AuthenticationTypeNotSpecifiedMessage_ShouldHaveCorrectProperties()
    {
        // Act
        var message = new AuthenticationTypeNotSpecifiedMessage();

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
        var message = new ConfigurationBindingFailedMessage();

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
        var message = new ConfigurationSectionNotFoundMessage();

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
        var message = new NoFactoryRegisteredMessage();

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
        var message = new ServiceCreationExceptionMessage();

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
        var message = new TokenRevocationFailedMessage();

        // Assert
        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void UnknownAuthenticationTypeMessage_ShouldHaveCorrectProperties()
    {
        // Act
        var message = new UnknownAuthenticationTypeMessage();

        // Assert
        message.ShouldNotBeNull();
        message.Severity.ShouldBe(MessageSeverity.Error);
    }

    [Fact]
    public void AuthenticationMessageCollectionBase_ShouldBeAccessible()
    {
        // Act
        var collection = AuthenticationMessageCollectionBase.All;

        // Assert
        collection.ShouldNotBeNull();
        collection.ShouldNotBeEmpty();
    }
}

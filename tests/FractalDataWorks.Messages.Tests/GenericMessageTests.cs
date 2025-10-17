using FractalDataWorks.Messages;

namespace FractalDataWorks.Messages.Tests;

/// <summary>
/// Tests for the GenericMessage class.
/// </summary>
public class GenericMessageTests
{
    #region Constructor Tests

    [Fact]
    public void DefaultConstructor_InitializesWithDefaults()
    {
        // Act
        var message = new GenericMessage();

        // Assert
        message.Message.ShouldBe(string.Empty);
        message.Severity.ShouldBe(MessageSeverity.Information);
        message.Code.ShouldBeNull();
        message.Source.ShouldBeNull();
        message.Id.ShouldBe(1);
        message.Name.ShouldBe("GenericMessage");
        message.Timestamp.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(1));
        message.CorrelationId.ShouldNotBe(Guid.Empty);
        message.Metadata.ShouldNotBeNull();
        message.Metadata.ShouldBeEmpty();
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessageAndDefaultSeverity()
    {
        // Arrange
        const string messageText = "Test message";

        // Act
        var message = new GenericMessage(messageText);

        // Assert
        message.Message.ShouldBe(messageText);
        message.Severity.ShouldBe(MessageSeverity.Information);
        message.Code.ShouldBeNull();
        message.Source.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithNullMessage_SetsEmptyString()
    {
        // Act
        var message = new GenericMessage(null!);

        // Assert
        message.Message.ShouldBe(string.Empty);
        message.Severity.ShouldBe(MessageSeverity.Information);
    }

    [Fact]
    public void Constructor_WithFullDetails_SetsAllProperties()
    {
        // Arrange
        const string messageText = "Error occurred";
        const string code = "ERR001";
        const string source = "TestModule";

        // Act
        var message = new GenericMessage(MessageSeverity.Error, messageText, code, source);

        // Assert
        message.Severity.ShouldBe(MessageSeverity.Error);
        message.Message.ShouldBe(messageText);
        message.Code.ShouldBe(code);
        message.Source.ShouldBe(source);
    }

    [Fact]
    public void Constructor_WithFullDetailsNullMessage_SetsEmptyString()
    {
        // Act
        var message = new GenericMessage(MessageSeverity.Warning, null!, "CODE", "Source");

        // Assert
        message.Message.ShouldBe(string.Empty);
        message.Severity.ShouldBe(MessageSeverity.Warning);
        message.Code.ShouldBe("CODE");
        message.Source.ShouldBe("Source");
    }

    [Fact]
    public void Constructor_WithFullDetailsNullCodeAndSource_AcceptsNulls()
    {
        // Act
        var message = new GenericMessage(MessageSeverity.Critical, "Critical error", null, null);

        // Assert
        message.Message.ShouldBe("Critical error");
        message.Severity.ShouldBe(MessageSeverity.Critical);
        message.Code.ShouldBeNull();
        message.Source.ShouldBeNull();
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Properties_CanBeSetAndRead()
    {
        // Arrange
        var message = new GenericMessage();
        var timestamp = DateTime.UtcNow.AddHours(-1);
        var correlationId = Guid.NewGuid();
        var metadata = new Dictionary<string, object> { ["key"] = "value" };

        // Act
        message.Message = "Updated message";
        message.Severity = MessageSeverity.Warning;
        message.Code = "WARN001";
        message.Source = "UpdatedSource";
        message.Id = 42;
        message.Name = "CustomName";
        message.Timestamp = timestamp;
        message.CorrelationId = correlationId;
        message.Metadata = metadata;

        // Assert
        message.Message.ShouldBe("Updated message");
        message.Severity.ShouldBe(MessageSeverity.Warning);
        message.Code.ShouldBe("WARN001");
        message.Source.ShouldBe("UpdatedSource");
        message.Id.ShouldBe(42);
        message.Name.ShouldBe("CustomName");
        message.Timestamp.ShouldBe(timestamp);
        message.CorrelationId.ShouldBe(correlationId);
        message.Metadata.ShouldBeSameAs(metadata);
    }

    [Fact]
    public void CorrelationId_GeneratesUniqueValues()
    {
        // Act
        var message1 = new GenericMessage();
        var message2 = new GenericMessage();

        // Assert
        message1.CorrelationId.ShouldNotBe(message2.CorrelationId);
        message1.CorrelationId.ShouldNotBe(Guid.Empty);
        message2.CorrelationId.ShouldNotBe(Guid.Empty);
    }

    [Theory]
    [InlineData(MessageSeverity.Information)]
    [InlineData(MessageSeverity.Warning)]
    [InlineData(MessageSeverity.Error)]
    [InlineData(MessageSeverity.Critical)]
    public void Severity_SupportsAllSeverityLevels(MessageSeverity severity)
    {
        // Arrange & Act
        var message = new GenericMessage(severity, "Test message");

        // Assert
        message.Severity.ShouldBe(severity);
    }

    #endregion
}

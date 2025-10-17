using FractalDataWorks.Messages;

namespace FractalDataWorks.Messages.Tests;

public sealed class MessageTemplateTests
{
    // Test message implementations for testing base class functionality
    private sealed class TestMessage : MessageTemplate<MessageSeverity>
    {
        public TestMessage(int id, string name, MessageSeverity severity, string message, string? code = null, string? source = null)
            : base(id, name, severity, message, code, source)
        {
        }

        public TestMessage(int id, string name, MessageSeverity severity, string message, string? code, string? source, IDictionary<string, object?>? details, object? data)
            : base(id, name, severity, message, code, source, details, data)
        {
        }
    }

    private sealed class TestMessageWithSeverityChange : MessageTemplate<MessageSeverity>
    {
        public TestMessageWithSeverityChange(int id, string name, MessageSeverity severity, string message, string? code = null, string? source = null)
            : base(id, name, severity, message, code, source)
        {
        }

        public override MessageTemplate<MessageSeverity> WithSeverity(MessageSeverity severity)
        {
            return new TestMessageWithSeverityChange(Id, Name, severity, Message, Code, Source);
        }
    }

    [Fact]
    public void Constructor_ShouldSetAllProperties_WhenCalledWithBasicParameters()
    {
        // Arrange & Act
        var message = new TestMessage(
            id: 1,
            name: "TestMessage",
            severity: MessageSeverity.Error,
            message: "Test error message",
            code: "TEST001",
            source: "TestSource");

        // Assert
        message.Id.ShouldBe(1);
        message.Name.ShouldBe("TestMessage");
        message.Severity.ShouldBe(MessageSeverity.Error);
        message.Message.ShouldBe("Test error message");
        message.Code.ShouldBe("TEST001");
        message.Source.ShouldBe("TestSource");
        message.OriginatedIn.ShouldBe("TestSource");
        message.Details.ShouldBeNull();
        message.Data.ShouldBeNull();
        message.Timestamp.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void Constructor_ShouldSetOriginatedInToUnknown_WhenSourceIsNull()
    {
        // Arrange & Act
        var message = new TestMessage(
            id: 1,
            name: "TestMessage",
            severity: MessageSeverity.Information,
            message: "Test message",
            code: null,
            source: null);

        // Assert
        message.Source.ShouldBeNull();
        message.OriginatedIn.ShouldBe("Unknown");
    }

    [Fact]
    public void Constructor_ShouldSetDetailsAndData_WhenCalledWithExtendedParameters()
    {
        // Arrange
        var details = new Dictionary<string, object?> { { "Key1", "Value1" }, { "Key2", 42 } };
        var data = new { Property = "Value" };

        // Act
        var message = new TestMessage(
            id: 1,
            name: "TestMessage",
            severity: MessageSeverity.Warning,
            message: "Test warning",
            code: "WARN001",
            source: "TestSource",
            details: details,
            data: data);

        // Assert
        message.Details.ShouldBe(details);
        message.Data.ShouldBe(data);
    }

    [Fact]
    public void Format_ShouldReturnUnformattedMessage_WhenNoArgumentsProvided()
    {
        // Arrange
        var message = new TestMessage(
            id: 1,
            name: "TestMessage",
            severity: MessageSeverity.Information,
            message: "Simple message without placeholders");

        // Act
        var formatted = message.Format();

        // Assert
        formatted.ShouldBe("Simple message without placeholders");
    }

    [Fact]
    public void Format_ShouldReturnUnformattedMessage_WhenEmptyArgumentsArrayProvided()
    {
        // Arrange
        var message = new TestMessage(
            id: 1,
            name: "TestMessage",
            severity: MessageSeverity.Information,
            message: "Simple message");

        // Act
        var formatted = message.Format(Array.Empty<object>());

        // Assert
        formatted.ShouldBe("Simple message");
    }

    [Fact]
    public void Format_ShouldFormatMessage_WhenArgumentsProvided()
    {
        // Arrange
        var message = new TestMessage(
            id: 1,
            name: "TestMessage",
            severity: MessageSeverity.Error,
            message: "Error occurred in {0} at {1}");

        // Act
        var formatted = message.Format("TestMethod", "line 42");

        // Assert
        formatted.ShouldBe("Error occurred in TestMethod at line 42");
    }

    [Fact]
    public void Format_ShouldHandleMultipleArguments_WithComplexFormatting()
    {
        // Arrange
        var message = new TestMessage(
            id: 1,
            name: "TestMessage",
            severity: MessageSeverity.Warning,
            message: "User {0} attempted {1} at {2:yyyy-MM-dd HH:mm:ss}");

        var timestamp = new DateTime(2025, 1, 15, 10, 30, 0);

        // Act
        var formatted = message.Format("Alice", "login", timestamp);

        // Assert
        formatted.ShouldBe("User Alice attempted login at 2025-01-15 10:30:00");
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenMessagesAreIdentical()
    {
        // Arrange
        var message1 = new TestMessage(1, "Test", MessageSeverity.Error, "Message", "CODE", "Source");
        var message2 = new TestMessage(1, "Test", MessageSeverity.Error, "Message", "CODE", "Source");

        // Act
        var result = message1.Equals(message2);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenIdsAreDifferent()
    {
        // Arrange
        var message1 = new TestMessage(1, "Test", MessageSeverity.Error, "Message", "CODE", "Source");
        var message2 = new TestMessage(2, "Test", MessageSeverity.Error, "Message", "CODE", "Source");

        // Act
        var result = message1.Equals(message2);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenNamesAreDifferent()
    {
        // Arrange
        var message1 = new TestMessage(1, "Test1", MessageSeverity.Error, "Message", "CODE", "Source");
        var message2 = new TestMessage(1, "Test2", MessageSeverity.Error, "Message", "CODE", "Source");

        // Act
        var result = message1.Equals(message2);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenSeveritiesAreDifferent()
    {
        // Arrange
        var message1 = new TestMessage(1, "Test", MessageSeverity.Error, "Message", "CODE", "Source");
        var message2 = new TestMessage(1, "Test", MessageSeverity.Warning, "Message", "CODE", "Source");

        // Act
        var result = message1.Equals(message2);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenMessagesAreDifferent()
    {
        // Arrange
        var message1 = new TestMessage(1, "Test", MessageSeverity.Error, "Message1", "CODE", "Source");
        var message2 = new TestMessage(1, "Test", MessageSeverity.Error, "Message2", "CODE", "Source");

        // Act
        var result = message1.Equals(message2);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenCodesAreDifferent()
    {
        // Arrange
        var message1 = new TestMessage(1, "Test", MessageSeverity.Error, "Message", "CODE1", "Source");
        var message2 = new TestMessage(1, "Test", MessageSeverity.Error, "Message", "CODE2", "Source");

        // Act
        var result = message1.Equals(message2);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenSourcesAreDifferent()
    {
        // Arrange
        var message1 = new TestMessage(1, "Test", MessageSeverity.Error, "Message", "CODE", "Source1");
        var message2 = new TestMessage(1, "Test", MessageSeverity.Error, "Message", "CODE", "Source2");

        // Act
        var result = message1.Equals(message2);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenBothCodesAreNull()
    {
        // Arrange
        var message1 = new TestMessage(1, "Test", MessageSeverity.Error, "Message", null, "Source");
        var message2 = new TestMessage(1, "Test", MessageSeverity.Error, "Message", null, "Source");

        // Act
        var result = message1.Equals(message2);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void Equals_ShouldReturnTrue_WhenBothSourcesAreNull()
    {
        // Arrange
        var message1 = new TestMessage(1, "Test", MessageSeverity.Error, "Message", "CODE", null);
        var message2 = new TestMessage(1, "Test", MessageSeverity.Error, "Message", "CODE", null);

        // Act
        var result = message1.Equals(message2);

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparingWithNull()
    {
        // Arrange
        var message = new TestMessage(1, "Test", MessageSeverity.Error, "Message", "CODE", "Source");

        // Act
        var result = message.Equals(null);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void Equals_ShouldReturnFalse_WhenComparingWithDifferentType()
    {
        // Arrange
        var message = new TestMessage(1, "Test", MessageSeverity.Error, "Message", "CODE", "Source");
        var other = new object();

        // Act
        var result = message.Equals(other);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void GetHashCode_ShouldReturnSameValue_ForIdenticalMessages()
    {
        // Arrange
        var message1 = new TestMessage(1, "Test", MessageSeverity.Error, "Message", "CODE", "Source");
        var message2 = new TestMessage(1, "Test", MessageSeverity.Error, "Message", "CODE", "Source");

        // Act
        var hash1 = message1.GetHashCode();
        var hash2 = message2.GetHashCode();

        // Assert
        hash1.ShouldBe(hash2);
    }

    [Fact]
    public void GetHashCode_ShouldReturnDifferentValue_ForDifferentMessages()
    {
        // Arrange
        var message1 = new TestMessage(1, "Test", MessageSeverity.Error, "Message", "CODE", "Source");
        var message2 = new TestMessage(2, "Test", MessageSeverity.Error, "Message", "CODE", "Source");

        // Act
        var hash1 = message1.GetHashCode();
        var hash2 = message2.GetHashCode();

        // Assert
        hash1.ShouldNotBe(hash2);
    }

    [Fact]
    public void ToString_ShouldIncludeAllComponents_WithAllPropertiesSet()
    {
        // Arrange
        var message = new TestMessage(
            id: 1,
            name: "TestMessage",
            severity: MessageSeverity.Error,
            message: "Test error message",
            code: "TEST001",
            source: "TestSource");

        // Act
        var result = message.ToString();

        // Assert
        result.ShouldContain("[Error]");
        result.ShouldContain("(TEST001)");
        result.ShouldContain("Test error message");
        result.ShouldContain("Source: TestSource");
        result.ShouldContain("UTC");
    }

    [Fact]
    public void ToString_ShouldOmitCode_WhenCodeIsNull()
    {
        // Arrange
        var message = new TestMessage(
            id: 1,
            name: "TestMessage",
            severity: MessageSeverity.Information,
            message: "Test message",
            code: null,
            source: "TestSource");

        // Act
        var result = message.ToString();

        // Assert
        result.ShouldContain("[Information]");
        result.ShouldNotContain("(");
        result.ShouldContain("Test message");
        result.ShouldContain("Source: TestSource");
    }

    [Fact]
    public void ToString_ShouldOmitSource_WhenSourceIsNull()
    {
        // Arrange
        var message = new TestMessage(
            id: 1,
            name: "TestMessage",
            severity: MessageSeverity.Warning,
            message: "Test warning",
            code: "WARN001",
            source: null);

        // Act
        var result = message.ToString();

        // Assert
        result.ShouldContain("[Warning]");
        result.ShouldContain("(WARN001)");
        result.ShouldContain("Test warning");
        result.ShouldNotContain("Source:");
    }

    [Fact]
    public void ToString_ShouldFormatTimestamp_InCorrectFormat()
    {
        // Arrange
        var message = new TestMessage(
            id: 1,
            name: "TestMessage",
            severity: MessageSeverity.Critical,
            message: "Critical error");

        // Act
        var result = message.ToString();

        // Assert
        result.ShouldMatch(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2} UTC");
    }

    [Fact]
    public void WithSeverity_ShouldThrowNotSupportedException_ByDefault()
    {
        // Arrange
        var message = new TestMessage(1, "Test", MessageSeverity.Error, "Message");

        // Act & Assert
        Should.Throw<NotSupportedException>(() => message.WithSeverity(MessageSeverity.Warning))
            .Message.ShouldContain("TestMessage");
    }

    [Fact]
    public void WithSeverity_ShouldReturnNewInstanceWithNewSeverity_WhenOverridden()
    {
        // Arrange
        var message = new TestMessageWithSeverityChange(1, "Test", MessageSeverity.Error, "Message", "CODE", "Source");

        // Act
        var result = message.WithSeverity(MessageSeverity.Warning);

        // Assert
        result.ShouldNotBeSameAs(message);
        result.Severity.ShouldBe(MessageSeverity.Warning);
        result.Id.ShouldBe(message.Id);
        result.Name.ShouldBe(message.Name);
        result.Message.ShouldBe(message.Message);
        result.Code.ShouldBe(message.Code);
        result.Source.ShouldBe(message.Source);
    }

    [Theory]
    [InlineData(MessageSeverity.Information)]
    [InlineData(MessageSeverity.Warning)]
    [InlineData(MessageSeverity.Error)]
    [InlineData(MessageSeverity.Critical)]
    public void Constructor_ShouldAcceptAllSeverityLevels(MessageSeverity severity)
    {
        // Arrange & Act
        var message = new TestMessage(1, "Test", severity, "Test message");

        // Assert
        message.Severity.ShouldBe(severity);
    }
}

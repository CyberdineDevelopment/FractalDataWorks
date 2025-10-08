using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.Messages;

namespace FractalDataWorks.Messages.Tests;

/// <summary>
/// Tests for MessageTemplate base class covering all pathways for 100% coverage.
/// </summary>
public class MessageTemplateTests
{
    // Test implementation of abstract MessageTemplate
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

    [Fact]
    public void Constructor_WithBasicParameters_InitializesProperties()
    {
        var message = new TestMessage(1, "TestMsg", MessageSeverity.Information, "Test message", "CODE001", "TestSource");

        message.Id.ShouldBe(1);
        message.Name.ShouldBe("TestMsg");
        message.Severity.ShouldBe(MessageSeverity.Information);
        message.Message.ShouldBe("Test message");
        message.Code.ShouldBe("CODE001");
        message.Source.ShouldBe("TestSource");
        message.OriginatedIn.ShouldBe("TestSource");
        message.Timestamp.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(1));
        message.Details.ShouldBeNull();
        message.Data.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithNullSource_SetsOriginatedInToUnknown()
    {
        var message = new TestMessage(1, "TestMsg", MessageSeverity.Information, "Test", null, null);

        message.Source.ShouldBeNull();
        message.OriginatedIn.ShouldBe("Unknown");
    }

    [Fact]
    public void Constructor_WithAllParameters_InitializesAllProperties()
    {
        var details = new Dictionary<string, object?> { ["key1"] = "value1", ["key2"] = 42 };
        var data = new { Property = "value" };

        var message = new TestMessage(2, "FullMsg", MessageSeverity.Error, "Full message", "CODE002", "Source", details, data);

        message.Id.ShouldBe(2);
        message.Name.ShouldBe("FullMsg");
        message.Severity.ShouldBe(MessageSeverity.Error);
        message.Message.ShouldBe("Full message");
        message.Code.ShouldBe("CODE002");
        message.Source.ShouldBe("Source");
        message.OriginatedIn.ShouldBe("Source");
        message.Details.ShouldBe(details);
        message.Data.ShouldBe(data);
    }

    [Fact]
    public void Format_WithNoArgs_ReturnsOriginalMessage()
    {
        var message = new TestMessage(1, "Test", MessageSeverity.Information, "Simple message");

        var formatted = message.Format();

        formatted.ShouldBe("Simple message");
    }

    [Fact]
    public void Format_WithArgs_FormatsMessage()
    {
        var message = new TestMessage(1, "Test", MessageSeverity.Information, "Hello {0}, you have {1} messages");

        var formatted = message.Format("John", 5);

        formatted.ShouldBe("Hello John, you have 5 messages");
    }

    [Fact]
    public void Format_WithNullArgs_ReturnsOriginalMessage()
    {
        var message = new TestMessage(1, "Test", MessageSeverity.Information, "Message");

        var formatted = message.Format(null!);

        formatted.ShouldBe("Message");
    }

    [Fact]
    public void Format_WithEmptyArgs_ReturnsOriginalMessage()
    {
        var message = new TestMessage(1, "Test", MessageSeverity.Information, "Message");

        var formatted = message.Format(Array.Empty<object>());

        formatted.ShouldBe("Message");
    }

    [Fact]
    public void WithSeverity_ThrowsNotSupportedException()
    {
        var message = new TestMessage(1, "Test", MessageSeverity.Information, "Message");

        var exception = Should.Throw<NotSupportedException>(() => message.WithSeverity(MessageSeverity.Error));
        exception.Message.ShouldContain("WithSeverity is not supported for TestMessage");
    }

    [Fact]
    public void Equals_SameMessages_ReturnsTrue()
    {
        var message1 = new TestMessage(1, "Test", MessageSeverity.Information, "Message", "CODE", "Source");
        var message2 = new TestMessage(1, "Test", MessageSeverity.Information, "Message", "CODE", "Source");

        message1.Equals(message2).ShouldBeTrue();
    }

    [Fact]
    public void Equals_DifferentId_ReturnsFalse()
    {
        var message1 = new TestMessage(1, "Test", MessageSeverity.Information, "Message");
        var message2 = new TestMessage(2, "Test", MessageSeverity.Information, "Message");

        message1.Equals(message2).ShouldBeFalse();
    }

    [Fact]
    public void Equals_DifferentName_ReturnsFalse()
    {
        var message1 = new TestMessage(1, "Test1", MessageSeverity.Information, "Message");
        var message2 = new TestMessage(1, "Test2", MessageSeverity.Information, "Message");

        message1.Equals(message2).ShouldBeFalse();
    }

    [Fact]
    public void Equals_DifferentSeverity_ReturnsFalse()
    {
        var message1 = new TestMessage(1, "Test", MessageSeverity.Information, "Message");
        var message2 = new TestMessage(1, "Test", MessageSeverity.Error, "Message");

        message1.Equals(message2).ShouldBeFalse();
    }

    [Fact]
    public void Equals_DifferentMessage_ReturnsFalse()
    {
        var message1 = new TestMessage(1, "Test", MessageSeverity.Information, "Message1");
        var message2 = new TestMessage(1, "Test", MessageSeverity.Information, "Message2");

        message1.Equals(message2).ShouldBeFalse();
    }

    [Fact]
    public void Equals_DifferentCode_ReturnsFalse()
    {
        var message1 = new TestMessage(1, "Test", MessageSeverity.Information, "Message", "CODE1");
        var message2 = new TestMessage(1, "Test", MessageSeverity.Information, "Message", "CODE2");

        message1.Equals(message2).ShouldBeFalse();
    }

    [Fact]
    public void Equals_DifferentSource_ReturnsFalse()
    {
        var message1 = new TestMessage(1, "Test", MessageSeverity.Information, "Message", null, "Source1");
        var message2 = new TestMessage(1, "Test", MessageSeverity.Information, "Message", null, "Source2");

        message1.Equals(message2).ShouldBeFalse();
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        var message = new TestMessage(1, "Test", MessageSeverity.Information, "Message");

        message.Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void Equals_DifferentType_ReturnsFalse()
    {
        var message = new TestMessage(1, "Test", MessageSeverity.Information, "Message");

        message.Equals("string").ShouldBeFalse();
    }

    [Fact]
    public void GetHashCode_SameMessages_ReturnsSameHash()
    {
        var message1 = new TestMessage(1, "Test", MessageSeverity.Information, "Message", "CODE", "Source");
        var message2 = new TestMessage(1, "Test", MessageSeverity.Information, "Message", "CODE", "Source");

        message1.GetHashCode().ShouldBe(message2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentMessages_ReturnsDifferentHash()
    {
        var message1 = new TestMessage(1, "Test", MessageSeverity.Information, "Message1");
        var message2 = new TestMessage(2, "Test", MessageSeverity.Information, "Message2");

        message1.GetHashCode().ShouldNotBe(message2.GetHashCode());
    }

    [Fact]
    public void ToString_WithAllProperties_FormatsCorrectly()
    {
        var message = new TestMessage(1, "Test", MessageSeverity.Error, "Error occurred", "ERR001", "TestSource");

        var result = message.ToString();

        result.ShouldContain("[Error]");
        result.ShouldContain("(ERR001)");
        result.ShouldContain("Error occurred");
        result.ShouldContain("Source: TestSource");
        result.ShouldContain("UTC");
    }

    [Fact]
    public void ToString_WithoutCode_OmitsCodeSection()
    {
        var message = new TestMessage(1, "Test", MessageSeverity.Warning, "Warning message", null, "Source");

        var result = message.ToString();

        result.ShouldContain("[Warning]");
        result.ShouldNotContain("(");
        result.ShouldContain("Warning message");
    }

    [Fact]
    public void ToString_WithoutSource_OmitsSourceSection()
    {
        var message = new TestMessage(1, "Test", MessageSeverity.Information, "Info message", "CODE");

        var result = message.ToString();

        result.ShouldContain("[Information]");
        result.ShouldNotContain("Source:");
    }

    [Fact]
    public void ToString_WithEmptyCode_OmitsCodeSection()
    {
        var message = new TestMessage(1, "Test", MessageSeverity.Critical, "Critical", string.Empty);

        var result = message.ToString();

        result.ShouldNotContain("()");
    }

    [Fact]
    public void ToString_WithEmptySource_OmitsSourceSection()
    {
        var message = new TestMessage(1, "Test", MessageSeverity.Error, "Error", null, string.Empty);

        var result = message.ToString();

        result.ShouldNotContain("Source:");
    }

    [Fact]
    public void Timestamp_IsUtcDateTime()
    {
        var message = new TestMessage(1, "Test", MessageSeverity.Information, "Message");

        message.Timestamp.Kind.ShouldBe(DateTimeKind.Utc);
    }
}

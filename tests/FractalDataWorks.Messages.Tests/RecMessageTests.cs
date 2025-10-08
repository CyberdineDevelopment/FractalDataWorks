using System;
using System.Collections.Generic;
using FractalDataWorks.Messages;

namespace FractalDataWorks.Messages.Tests;

/// <summary>
/// Tests for RecMessage class covering all pathways for 100% coverage.
/// </summary>
public class RecMessageTests
{
    [Fact]
    public void Constructor_Default_InitializesWithDefaultValues()
    {
        var message = new RecMessage();

        message.Message.ShouldBe(string.Empty);
        message.Severity.ShouldBe(MessageSeverity.Information);
        message.Code.ShouldBeNull();
        message.Source.ShouldBeNull();
        message.Id.ShouldBe(1);
        message.Name.ShouldBe("RecMessage");
        message.Timestamp.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(1));
        message.CorrelationId.ShouldNotBe(Guid.Empty);
        message.Metadata.ShouldNotBeNull();
        message.Metadata.ShouldBeEmpty();
    }

    [Fact]
    public void Constructor_WithMessage_InitializesWithMessageAndDefaultSeverity()
    {
        var messageText = "Test message";

        var message = new RecMessage(messageText);

        message.Message.ShouldBe(messageText);
        message.Severity.ShouldBe(MessageSeverity.Information);
        message.Code.ShouldBeNull();
        message.Source.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithNullMessage_InitializesWithEmptyString()
    {
        var message = new RecMessage((string)null!);

        message.Message.ShouldBe(string.Empty);
    }

    [Fact]
    public void Constructor_WithFullParameters_InitializesAllProperties()
    {
        var severity = MessageSeverity.Error;
        var messageText = "Error occurred";
        var code = "ERR001";
        var source = "TestSource";

        var message = new RecMessage(severity, messageText, code, source);

        message.Severity.ShouldBe(severity);
        message.Message.ShouldBe(messageText);
        message.Code.ShouldBe(code);
        message.Source.ShouldBe(source);
        message.Timestamp.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(1));
        message.CorrelationId.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_WithFullParameters_NullMessageHandled()
    {
        var message = new RecMessage(MessageSeverity.Warning, null!, "CODE", "Source");

        message.Message.ShouldBe(string.Empty);
    }

    [Theory]
    [InlineData(MessageSeverity.Information)]
    [InlineData(MessageSeverity.Warning)]
    [InlineData(MessageSeverity.Error)]
    [InlineData(MessageSeverity.Critical)]
    public void Constructor_WithDifferentSeverities_SetsSeverityCorrectly(MessageSeverity severity)
    {
        var message = new RecMessage(severity, "Test", "CODE", "Source");

        message.Severity.ShouldBe(severity);
    }

    [Fact]
    public void Metadata_CanBeModified()
    {
        var message = new RecMessage();

        message.Metadata.Add("key1", "value1");
        message.Metadata.Add("key2", 42);

        message.Metadata.Count.ShouldBe(2);
        message.Metadata["key1"].ShouldBe("value1");
        message.Metadata["key2"].ShouldBe(42);
    }

    [Fact]
    public void Metadata_UsesOrdinalStringComparer()
    {
        var message = new RecMessage();

        message.Metadata.Add("Key", "value1");
        message.Metadata.ContainsKey("key").ShouldBeFalse(); // Case-sensitive

        var dict = message.Metadata as Dictionary<string, object>;
        dict.ShouldNotBeNull();
        dict.Comparer.ShouldBe(StringComparer.Ordinal);
    }

    [Fact]
    public void Properties_CanBeSetAndGet()
    {
        var message = new RecMessage
        {
            Id = 999,
            Name = "CustomName",
            Severity = MessageSeverity.Critical,
            Message = "Custom message",
            Code = "CUSTOM",
            Source = "CustomSource"
        };

        message.Id.ShouldBe(999);
        message.Name.ShouldBe("CustomName");
        message.Severity.ShouldBe(MessageSeverity.Critical);
        message.Message.ShouldBe("Custom message");
        message.Code.ShouldBe("CUSTOM");
        message.Source.ShouldBe("CustomSource");
    }

    [Fact]
    public void CorrelationId_IsUniqueForEachInstance()
    {
        var message1 = new RecMessage();
        var message2 = new RecMessage();

        message1.CorrelationId.ShouldNotBe(message2.CorrelationId);
    }

    [Fact]
    public void Timestamp_IsSetToUtcNow()
    {
        var beforeTime = DateTime.UtcNow;
        var message = new RecMessage();
        var afterTime = DateTime.UtcNow;

        message.Timestamp.ShouldBeGreaterThanOrEqualTo(beforeTime);
        message.Timestamp.ShouldBeLessThanOrEqualTo(afterTime);
        message.Timestamp.Kind.ShouldBe(DateTimeKind.Utc);
    }
}

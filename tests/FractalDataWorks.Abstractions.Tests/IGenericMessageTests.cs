using FractalDataWorks.Messages;

namespace FractalDataWorks.Abstractions.Tests;

public class IGenericMessageTests
{
    private class TestMessage : IGenericMessage
    {
        public string Message { get; init; } = string.Empty;
        public string? Code { get; init; }
        public string? Source { get; init; }
    }

    private enum TestSeverity
    {
        Info,
        Warning,
        Error
    }

    private class TestMessageWithSeverity : IGenericMessage<TestSeverity>
    {
        public string Message { get; init; } = string.Empty;
        public string? Code { get; init; }
        public string? Source { get; init; }
        public TestSeverity Severity { get; init; }
    }

    [Fact]
    public void IGenericMessage_CanBeImplemented()
    {
        var message = new TestMessage
        {
            Message = "Test message",
            Code = "T001",
            Source = "TestSource"
        };

        message.Message.ShouldBe("Test message");
        message.Code.ShouldBe("T001");
        message.Source.ShouldBe("TestSource");
    }

    [Fact]
    public void IGenericMessage_Message_CanBeSet()
    {
        var message = new TestMessage { Message = "Hello World" };

        message.Message.ShouldBe("Hello World");
    }

    [Fact]
    public void IGenericMessage_Code_CanBeNull()
    {
        var message = new TestMessage { Code = null };

        message.Code.ShouldBeNull();
    }

    [Fact]
    public void IGenericMessage_Source_CanBeNull()
    {
        var message = new TestMessage { Source = null };

        message.Source.ShouldBeNull();
    }

    [Fact]
    public void IGenericMessage_Generic_CanBeImplemented()
    {
        var message = new TestMessageWithSeverity
        {
            Message = "Warning message",
            Code = "W001",
            Source = "System",
            Severity = TestSeverity.Warning
        };

        message.Message.ShouldBe("Warning message");
        message.Code.ShouldBe("W001");
        message.Source.ShouldBe("System");
        message.Severity.ShouldBe(TestSeverity.Warning);
    }

    [Fact]
    public void IGenericMessage_Generic_InheritsFromNonGeneric()
    {
        var typed = new TestMessageWithSeverity
        {
            Message = "Test",
            Code = "T001",
            Severity = TestSeverity.Error
        };

        IGenericMessage nonGeneric = typed;

        nonGeneric.Message.ShouldBe("Test");
        nonGeneric.Code.ShouldBe("T001");
    }

    [Fact]
    public void IGenericMessage_Severity_CanBeSet()
    {
        var message = new TestMessageWithSeverity { Severity = TestSeverity.Error };

        message.Severity.ShouldBe(TestSeverity.Error);
    }
}

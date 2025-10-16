using FractalDataWorks.Messages;
using FractalDataWorks.Results;

namespace FractalDataWorks.Abstractions.Tests;

public class IGenericResultTests
{
    private class TestResult : IGenericResult
    {
        public bool IsSuccess { get; init; }
        public bool IsFailure { get; init; }
        public bool IsEmpty { get; init; }
        public bool Error { get; init; }
        public string? CurrentMessage { get; init; }
        public IReadOnlyList<IGenericMessage> Messages { get; init; } = Array.Empty<IGenericMessage>();
    }

    private class TestResultWithValue<T> : IGenericResult<T>
    {
        public bool IsSuccess { get; init; }
        public bool IsFailure { get; init; }
        public bool IsEmpty { get; init; }
        public bool Error { get; init; }
        public string? CurrentMessage { get; init; }
        public IReadOnlyList<IGenericMessage> Messages { get; init; } = Array.Empty<IGenericMessage>();
        public T? Value { get; init; }
    }

    private class TestMessage : IGenericMessage
    {
        public string Message { get; init; } = string.Empty;
        public string? Code { get; init; }
        public string? Source { get; init; }
    }

    [Fact]
    public void IGenericResult_CanBeImplemented()
    {
        var result = new TestResult { IsSuccess = true, IsFailure = false };

        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
    }

    [Fact]
    public void IGenericResult_IsSuccess_CanBeTrue()
    {
        var result = new TestResult { IsSuccess = true };

        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void IGenericResult_IsFailure_CanBeTrue()
    {
        var result = new TestResult { IsFailure = true };

        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void IGenericResult_IsEmpty_CanBeTrue()
    {
        var result = new TestResult { IsEmpty = true };

        result.IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public void IGenericResult_Error_CanBeTrue()
    {
        var result = new TestResult { Error = true };

        result.Error.ShouldBeTrue();
    }

    [Fact]
    public void IGenericResult_CurrentMessage_CanBeNull()
    {
        var result = new TestResult { CurrentMessage = null };

        result.CurrentMessage.ShouldBeNull();
    }

    [Fact]
    public void IGenericResult_CurrentMessage_CanBeSet()
    {
        var result = new TestResult { CurrentMessage = "Error occurred" };

        result.CurrentMessage.ShouldBe("Error occurred");
    }

    [Fact]
    public void IGenericResult_Messages_CanBeEmpty()
    {
        var result = new TestResult { Messages = Array.Empty<IGenericMessage>() };

        result.Messages.ShouldBeEmpty();
    }

    [Fact]
    public void IGenericResult_Messages_CanContainItems()
    {
        var messages = new List<IGenericMessage>
        {
            new TestMessage { Message = "Message 1" },
            new TestMessage { Message = "Message 2" }
        };
        var result = new TestResult { Messages = messages };

        result.Messages.Count.ShouldBe(2);
        result.Messages[0].Message.ShouldBe("Message 1");
        result.Messages[1].Message.ShouldBe("Message 2");
    }

    [Fact]
    public void IGenericResult_Generic_CanBeImplemented()
    {
        var result = new TestResultWithValue<string>
        {
            IsSuccess = true,
            Value = "Test Value"
        };

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("Test Value");
    }

    [Fact]
    public void IGenericResult_Generic_Value_CanBeNull()
    {
        var result = new TestResultWithValue<string> { Value = null };

        result.Value.ShouldBeNull();
    }

    [Fact]
    public void IGenericResult_Generic_Value_CanBeSet()
    {
        var result = new TestResultWithValue<int> { Value = 42 };

        result.Value.ShouldBe(42);
    }

    [Fact]
    public void IGenericResult_Generic_InheritsFromNonGeneric()
    {
        var typed = new TestResultWithValue<string>
        {
            IsSuccess = true,
            Value = "Test"
        };

        IGenericResult nonGeneric = typed;

        nonGeneric.IsSuccess.ShouldBeTrue();
    }
}

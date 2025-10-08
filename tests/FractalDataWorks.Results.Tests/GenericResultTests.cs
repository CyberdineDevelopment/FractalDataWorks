using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.Messages;
using FractalDataWorks.Results;

namespace FractalDataWorks.Results.Tests;

/// <summary>
/// Tests for GenericResult class covering all pathways for 100% coverage.
/// </summary>
public class GenericResultTests
{
    [Fact]
    public void Success_WithNoMessage_ReturnsSuccessResult()
    {
        var result = GenericResult.Success();

        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeFalse();
        result.IsEmpty.ShouldBeTrue();
        result.CurrentMessage.ShouldBeNull();
        result.Messages.ShouldBeEmpty();
    }

    [Fact]
    public void Success_WithStringMessage_ReturnsSuccessResultWithMessage()
    {
        var message = "Operation successful";

        var result = GenericResult.Success(message);

        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeFalse();
        result.IsEmpty.ShouldBeFalse();
        result.CurrentMessage.ShouldBe(message);
        result.Messages.Count.ShouldBe(1);
        result.Messages[0].Message.ShouldBe(message);
    }

    [Fact]
    public void Success_WithIGenericMessage_ReturnsSuccessResultWithMessage()
    {
        var message = new RecMessage(MessageSeverity.Information, "Test message", "TEST001", "TestSource");

        var result = GenericResult.Success(message);

        result.IsSuccess.ShouldBeTrue();
        result.IsEmpty.ShouldBeFalse();
        result.CurrentMessage.ShouldBe("Test message");
        result.Messages.Count.ShouldBe(1);
        result.Messages[0].ShouldBe(message);
        result.Messages[0].Code.ShouldBe("TEST001");
        result.Messages[0].Source.ShouldBe("TestSource");
    }

    [Fact]
    public void Success_WithGenericMessage_ReturnsSuccessResultWithMessage()
    {
        var message = new RecMessage(MessageSeverity.Information, "Success message");

        var result = GenericResult.Success<RecMessage>(message);

        result.IsSuccess.ShouldBeTrue();
        result.IsEmpty.ShouldBeFalse();
        result.CurrentMessage.ShouldBe("Success message");
        result.Messages.Count.ShouldBe(1);
    }

    [Fact]
    public void Success_WithEnumerableMessages_ReturnsSuccessResultWithAllMessages()
    {
        var messages = new List<IGenericMessage>
        {
            new RecMessage("Message 1"),
            new RecMessage("Message 2"),
            new RecMessage("Message 3")
        };

        var result = GenericResult.Success(messages.AsEnumerable());

        result.IsSuccess.ShouldBeTrue();
        result.IsEmpty.ShouldBeFalse();
        result.CurrentMessage.ShouldBe("Message 3"); // LIFO - Last In First Out
        result.Messages.Count.ShouldBe(3);
        result.Messages[0].Message.ShouldBe("Message 1");
        result.Messages[1].Message.ShouldBe("Message 2");
        result.Messages[2].Message.ShouldBe("Message 3");
    }

    [Fact]
    public void Success_WithParamsMessages_ReturnsSuccessResultWithAllMessages()
    {
        var result = GenericResult.Success(
            new RecMessage("Message 1"),
            new RecMessage("Message 2")
        );

        result.IsSuccess.ShouldBeTrue();
        result.IsEmpty.ShouldBeFalse();
        result.CurrentMessage.ShouldBe("Message 2");
        result.Messages.Count.ShouldBe(2);
    }

    [Fact]
    public void Failure_WithStringMessage_ReturnsFailureResultWithMessage()
    {
        var message = "Operation failed";

        var result = GenericResult.Failure(message);

        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeTrue();
        result.IsEmpty.ShouldBeFalse();
        result.CurrentMessage.ShouldBe(message);
        result.Messages.Count.ShouldBe(1);
        result.Messages[0].Message.ShouldBe(message);
    }

    [Fact]
    public void Failure_WithIGenericMessage_ReturnsFailureResultWithMessage()
    {
        var message = new RecMessage(MessageSeverity.Error, "Error occurred", "ERR001", "TestSource");

        var result = GenericResult.Failure(message);

        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeTrue();
        result.IsEmpty.ShouldBeFalse();
        result.CurrentMessage.ShouldBe("Error occurred");
        result.Messages.Count.ShouldBe(1);
        result.Messages[0].Code.ShouldBe("ERR001");
    }

    [Fact]
    public void Failure_WithGenericMessage_ReturnsFailureResultWithMessage()
    {
        var message = new RecMessage(MessageSeverity.Error, "Failure message");

        var result = GenericResult.Failure<RecMessage>(message);

        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.CurrentMessage.ShouldBe("Failure message");
    }

    [Fact]
    public void Failure_WithEnumerableMessages_ReturnsFailureResultWithAllMessages()
    {
        var messages = new List<IGenericMessage>
        {
            new RecMessage(MessageSeverity.Error, "Error 1"),
            new RecMessage(MessageSeverity.Error, "Error 2")
        };

        var result = GenericResult.Failure(messages.AsEnumerable());

        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.CurrentMessage.ShouldBe("Error 2");
        result.Messages.Count.ShouldBe(2);
    }

    [Fact]
    public void Failure_WithParamsMessages_ReturnsFailureResultWithAllMessages()
    {
        var result = GenericResult.Failure(
            new RecMessage(MessageSeverity.Error, "Error 1"),
            new RecMessage(MessageSeverity.Error, "Error 2")
        );

        result.IsSuccess.ShouldBeFalse();
        result.CurrentMessage.ShouldBe("Error 2");
        result.Messages.Count.ShouldBe(2);
    }

    [Fact]
    public void Success_WithNullEnumerableMessages_ReturnsSuccessResultWithNoMessages()
    {
        IEnumerable<IGenericMessage>? messages = null;

        var result = GenericResult.Success(messages!);

        result.IsSuccess.ShouldBeTrue();
        result.IsEmpty.ShouldBeTrue();
        result.Messages.ShouldBeEmpty();
    }

    [Fact]
    public void Failure_WithNullEnumerableMessages_ReturnsFailureResultWithNoMessages()
    {
        IEnumerable<IGenericMessage>? messages = null;

        var result = GenericResult.Failure(messages!);

        result.IsSuccess.ShouldBeFalse();
        result.IsEmpty.ShouldBeTrue();
        result.Messages.ShouldBeEmpty();
    }

    [Fact]
    public void Messages_ReturnsReadOnlyList()
    {
        var result = GenericResult.Success("Test");

        result.Messages.ShouldBeAssignableTo<IReadOnlyList<IGenericMessage>>();
    }

    [Fact]
    public void CurrentMessage_WithNoMessages_ReturnsNull()
    {
        var result = GenericResult.Success();

        result.CurrentMessage.ShouldBeNull();
    }

    [Fact]
    public void CurrentMessage_WithMultipleMessages_ReturnsLastMessage()
    {
        var result = GenericResult.Success(
            new RecMessage("First"),
            new RecMessage("Second"),
            new RecMessage("Third")
        );

        result.CurrentMessage.ShouldBe("Third");
    }
}

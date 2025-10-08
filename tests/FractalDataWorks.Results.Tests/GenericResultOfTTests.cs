using System;
using System.Collections.Generic;
using System.Linq;
using FractalDataWorks.Messages;
using FractalDataWorks.Results;

namespace FractalDataWorks.Results.Tests;

/// <summary>
/// Tests for GenericResult&lt;T&gt; class covering all pathways for 100% coverage.
/// </summary>
public class GenericResultOfTTests
{
    [Fact]
    public void Success_WithValue_ReturnsSuccessResultWithValue()
    {
        var value = 42;

        var result = GenericResult<int>.Success(value);

        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.IsEmpty.ShouldBeFalse();
        result.Value.ShouldBe(42);
        result.Messages.ShouldBeEmpty();
    }

    [Fact]
    public void Success_WithValueAndStringMessage_ReturnsSuccessResultWithValueAndMessage()
    {
        var value = "test result";
        var message = "Operation successful";

        var result = GenericResult<string>.Success(value, message);

        result.IsSuccess.ShouldBeTrue();
        result.IsEmpty.ShouldBeFalse();
        result.Value.ShouldBe("test result");
        result.CurrentMessage.ShouldBe(message);
        result.Messages.Count.ShouldBe(1);
    }

    [Fact]
    public void Success_WithValueAndIGenericMessage_ReturnsSuccessResultWithValueAndMessage()
    {
        var value = 100;
        var message = new RecMessage(MessageSeverity.Information, "Success", "INFO001");

        var result = GenericResult<int>.Success(value, message);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(100);
        result.CurrentMessage.ShouldBe("Success");
        result.Messages[0].Code.ShouldBe("INFO001");
    }

    [Fact]
    public void Success_WithValueAndGenericMessage_ReturnsSuccessResultWithValueAndMessage()
    {
        var value = true;
        var message = new RecMessage(MessageSeverity.Information, "Completed");

        var result = GenericResult<bool>.Success<RecMessage>(value, message);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(true);
        result.CurrentMessage.ShouldBe("Completed");
    }

    [Fact]
    public void Success_WithValueAndEnumerableMessages_ReturnsSuccessResultWithValueAndMessages()
    {
        var value = "result";
        var messages = new List<IGenericMessage>
        {
            new RecMessage("Message 1"),
            new RecMessage("Message 2")
        };

        var result = GenericResult<string>.Success(value, messages.AsEnumerable());

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("result");
        result.Messages.Count.ShouldBe(2);
        result.CurrentMessage.ShouldBe("Message 2");
    }

    [Fact]
    public void Success_WithValueAndParamsMessages_ReturnsSuccessResultWithValueAndMessages()
    {
        var value = 3.14;
        var result = GenericResult<double>.Success(
            value,
            new RecMessage("Info 1"),
            new RecMessage("Info 2")
        );

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(3.14);
        result.Messages.Count.ShouldBe(2);
    }

    [Fact]
    public void Failure_WithStringMessage_ReturnsFailureResultWithoutValue()
    {
        var result = GenericResult<int>.Failure("Operation failed");

        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeTrue();
        result.IsEmpty.ShouldBeTrue();
        result.CurrentMessage.ShouldBe("Operation failed");
        Should.Throw<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Failure_GenericMethod_WithStringMessage_ReturnsFailureResult()
    {
        var result = GenericResult<string>.Failure<string>("Error occurred");

        result.IsSuccess.ShouldBeFalse();
        result.CurrentMessage.ShouldBe("Error occurred");
        Should.Throw<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Failure_WithIGenericMessage_ReturnsFailureResultWithMessage()
    {
        var message = new RecMessage(MessageSeverity.Error, "Error", "ERR001");

        var result = GenericResult<int>.Failure(message);

        result.IsSuccess.ShouldBeFalse();
        result.CurrentMessage.ShouldBe("Error");
        result.Messages[0].Code.ShouldBe("ERR001");
    }

    [Fact]
    public void Failure_WithGenericMessage_ReturnsFailureResultWithMessage()
    {
        var message = new RecMessage(MessageSeverity.Error, "Failure");

        var result = GenericResult<bool>.Failure<RecMessage>(message);

        result.IsSuccess.ShouldBeFalse();
        result.CurrentMessage.ShouldBe("Failure");
    }

    [Fact]
    public void Failure_WithEnumerableMessages_ReturnsFailureResultWithMessages()
    {
        var messages = new List<IGenericMessage>
        {
            new RecMessage(MessageSeverity.Error, "Error 1"),
            new RecMessage(MessageSeverity.Error, "Error 2")
        };

        var result = GenericResult<string>.Failure(messages.AsEnumerable());

        result.IsSuccess.ShouldBeFalse();
        result.Messages.Count.ShouldBe(2);
        result.CurrentMessage.ShouldBe("Error 2");
    }

    [Fact]
    public void Failure_WithParamsMessages_ReturnsFailureResultWithMessages()
    {
        var result = GenericResult<int>.Failure(
            new RecMessage(MessageSeverity.Error, "Error 1"),
            new RecMessage(MessageSeverity.Error, "Error 2")
        );

        result.IsSuccess.ShouldBeFalse();
        result.Messages.Count.ShouldBe(2);
    }

    [Fact]
    public void Value_WhenFailure_ThrowsInvalidOperationException()
    {
        var result = GenericResult<int>.Failure("Error");

        var exception = Should.Throw<InvalidOperationException>(() => result.Value);
        exception.Message.ShouldBe("Cannot access value of a failed result.");
    }

    [Fact]
    public void IsEmpty_WhenSuccess_ReturnsFalse()
    {
        var result = GenericResult<int>.Success(42);

        result.IsEmpty.ShouldBeFalse();
    }

    [Fact]
    public void IsEmpty_WhenFailure_ReturnsTrue()
    {
        var result = GenericResult<int>.Failure("Error");

        result.IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public void Map_WhenSuccess_TransformsValue()
    {
        var result = GenericResult<int>.Success(10);
        var concreteResult = (GenericResult<int>)result;

        var mapped = concreteResult.Map(x => x * 2);

        mapped.IsSuccess.ShouldBeTrue();
        mapped.Value.ShouldBe(20);
    }

    [Fact]
    public void Map_WhenFailure_ReturnsFailureWithMessage()
    {
        var result = GenericResult<int>.Failure("Original error");
        var concreteResult = (GenericResult<int>)result;

        var mapped = concreteResult.Map(x => x * 2);

        mapped.IsSuccess.ShouldBeFalse();
        mapped.CurrentMessage.ShouldBe("Original error");
    }

    [Fact]
    public void Map_WithNullMapper_ThrowsArgumentNullException()
    {
        var result = GenericResult<int>.Success(10);
        var concreteResult = (GenericResult<int>)result;
        Func<int, string>? mapper = null;

        var exception = Should.Throw<ArgumentNullException>(() => concreteResult.Map(mapper!));
        exception.ParamName.ShouldBe("mapper");
    }

    [Fact]
    public void Map_WhenFailureWithNoMessage_UsesDefaultMessage()
    {
        var result = GenericResult<int>.Failure(new List<IGenericMessage>());
        var concreteResult = (GenericResult<int>)result;

        var mapped = concreteResult.Map(x => x.ToString());

        mapped.IsSuccess.ShouldBeFalse();
        mapped.CurrentMessage.ShouldBe("Operation failed");
    }

    [Fact]
    public void Match_WhenSuccess_CallsSuccessFunction()
    {
        var result = GenericResult<int>.Success(42);
        var concreteResult = (GenericResult<int>)result;

        var output = concreteResult.Match(
            success: x => $"Success: {x}",
            failure: err => $"Failure: {err}"
        );

        output.ShouldBe("Success: 42");
    }

    [Fact]
    public void Match_WhenFailure_CallsFailureFunction()
    {
        var result = GenericResult<int>.Failure("Error message");
        var concreteResult = (GenericResult<int>)result;

        var output = concreteResult.Match(
            success: x => $"Success: {x}",
            failure: err => $"Failure: {err}"
        );

        output.ShouldBe("Failure: Error message");
    }

    [Fact]
    public void Match_WhenFailureWithNoMessage_PassesEmptyString()
    {
        var result = GenericResult<int>.Failure(new List<IGenericMessage>());
        var concreteResult = (GenericResult<int>)result;

        var output = concreteResult.Match(
            success: x => "Success",
            failure: err => $"Error: '{err}'"
        );

        output.ShouldBe("Error: ''");
    }

    [Fact]
    public void Match_WithNullSuccessFunction_ThrowsArgumentNullException()
    {
        var result = GenericResult<int>.Success(10);
        var concreteResult = (GenericResult<int>)result;
        Func<int, string>? success = null;

        var exception = Should.Throw<ArgumentNullException>(() => concreteResult.Match(success!, x => "fail"));
        exception.ParamName.ShouldBe("success");
    }

    [Fact]
    public void Match_WithNullFailureFunction_ThrowsArgumentNullException()
    {
        var result = GenericResult<int>.Success(10);
        var concreteResult = (GenericResult<int>)result;
        Func<string, string>? failure = null;

        var exception = Should.Throw<ArgumentNullException>(() => concreteResult.Match(x => "success", failure!));
        exception.ParamName.ShouldBe("failure");
    }

    [Fact]
    public void Messages_Property_ReturnsMessagesFromBase()
    {
        var messages = new List<IGenericMessage>
        {
            new RecMessage("Message 1"),
            new RecMessage("Message 2")
        };

        var result = GenericResult<int>.Success(10, messages.AsEnumerable());

        result.Messages.Count.ShouldBe(2);
        result.Messages.ShouldBeAssignableTo<IReadOnlyList<IGenericMessage>>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void Success_WithVariousIntegerValues_StoresValueCorrectly(int value)
    {
        var result = GenericResult<int>.Success(value);

        result.Value.ShouldBe(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("test")]
    public void Success_WithVariousStringValues_StoresValueCorrectly(string? value)
    {
        var result = GenericResult<string?>.Success(value);

        result.Value.ShouldBe(value);
    }
}

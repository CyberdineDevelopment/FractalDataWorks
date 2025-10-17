using FractalDataWorks.Messages;
using FractalDataWorks.Results;

namespace FractalDataWorks.Results.Tests;

/// <summary>
/// Tests for the non-generic GenericResult class.
/// </summary>
public class GenericResultTests
{
    #region Success Tests

    [Fact]
    public void Success_WithNoMessage_CreatesSuccessResult()
    {
        // Act
        var result = GenericResult.Success();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeFalse();
        result.IsEmpty.ShouldBeTrue();
        result.CurrentMessage.ShouldBeNull();
        result.Messages.ShouldBeEmpty();
    }

    [Fact]
    public void Success_WithStringMessage_CreatesSuccessResultWithMessage()
    {
        // Arrange
        const string message = "Operation completed successfully";

        // Act
        var result = GenericResult.Success(message);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.IsEmpty.ShouldBeFalse();
        result.CurrentMessage.ShouldBe(message);
        result.Messages.Count.ShouldBe(1);
        result.Messages[0].Message.ShouldBe(message);
    }

    [Fact]
    public void Success_WithIGenericMessage_CreatesSuccessResultWithMessage()
    {
        // Arrange
        IGenericMessage message = new GenericMessage("Success message");

        // Act
        var result = GenericResult.Success(message);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsEmpty.ShouldBeFalse();
        result.CurrentMessage.ShouldBe(message.Message);
        result.Messages.Count.ShouldBe(1);
        result.Messages[0].ShouldBe(message);
    }

    [Fact]
    public void Success_WithGenericMessage_CreatesSuccessResultWithMessage()
    {
        // Arrange
        var message = new GenericMessage("Generic success");

        // Act
        var result = GenericResult.Success<GenericMessage>(message);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.CurrentMessage.ShouldBe(message.Message);
        result.Messages[0].ShouldBe(message);
    }

    [Fact]
    public void Success_WithMessageCollection_CreatesSuccessResultWithMultipleMessages()
    {
        // Arrange
        var messages = new List<IGenericMessage>
        {
            new GenericMessage("Message 1"),
            new GenericMessage("Message 2"),
            new GenericMessage("Message 3")
        };

        // Act
        var result = GenericResult.Success(messages);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsEmpty.ShouldBeFalse();
        result.Messages.Count.ShouldBe(3);
        result.CurrentMessage.ShouldBe("Message 3"); // LIFO - last message
        result.Messages[0].Message.ShouldBe("Message 1");
        result.Messages[1].Message.ShouldBe("Message 2");
        result.Messages[2].Message.ShouldBe("Message 3");
    }

    [Fact]
    public void Success_WithParamsMessages_CreatesSuccessResultWithMultipleMessages()
    {
        // Arrange
        var msg1 = new GenericMessage("Message 1");
        var msg2 = new GenericMessage("Message 2");

        // Act
        var result = GenericResult.Success(msg1, msg2);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Messages.Count.ShouldBe(2);
        result.CurrentMessage.ShouldBe("Message 2");
    }

    #endregion

    #region Failure Tests

    [Fact]
    public void Failure_WithStringMessage_CreatesFailureResultWithMessage()
    {
        // Arrange
        const string message = "Operation failed";

        // Act
        var result = GenericResult.Failure(message);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBeTrue();
        result.IsEmpty.ShouldBeFalse();
        result.CurrentMessage.ShouldBe(message);
        result.Messages.Count.ShouldBe(1);
    }

    [Fact]
    public void Failure_WithIGenericMessage_CreatesFailureResultWithMessage()
    {
        // Arrange
        IGenericMessage message = new GenericMessage("Failure message");

        // Act
        var result = GenericResult.Failure(message);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.CurrentMessage.ShouldBe(message.Message);
        result.Messages[0].ShouldBe(message);
    }

    [Fact]
    public void Failure_WithGenericMessage_CreatesFailureResultWithMessage()
    {
        // Arrange
        var message = new GenericMessage("Generic failure");

        // Act
        var result = GenericResult.Failure<GenericMessage>(message);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.CurrentMessage.ShouldBe(message.Message);
    }

    [Fact]
    public void Failure_WithMessageCollection_CreatesFailureResultWithMultipleMessages()
    {
        // Arrange
        var messages = new List<IGenericMessage>
        {
            new GenericMessage("Error 1"),
            new GenericMessage("Error 2")
        };

        // Act
        var result = GenericResult.Failure(messages);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Messages.Count.ShouldBe(2);
        result.CurrentMessage.ShouldBe("Error 2");
    }

    [Fact]
    public void Failure_WithParamsMessages_CreatesFailureResultWithMultipleMessages()
    {
        // Arrange
        var msg1 = new GenericMessage("Error 1");
        var msg2 = new GenericMessage("Error 2");
        var msg3 = new GenericMessage("Error 3");

        // Act
        var result = GenericResult.Failure(msg1, msg2, msg3);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Messages.Count.ShouldBe(3);
        result.CurrentMessage.ShouldBe("Error 3");
    }

    #endregion

    #region Property Tests

    [Fact]
    public void IsFailure_IsOppositeOfIsSuccess()
    {
        // Arrange & Act
        var success = GenericResult.Success();
        var failure = GenericResult.Failure("Error");

        // Assert
        success.IsFailure.ShouldBe(!success.IsSuccess);
        failure.IsFailure.ShouldBe(!failure.IsSuccess);
    }

    [Fact]
    public void Error_IsOppositeOfIsSuccess()
    {
        // Arrange & Act
        var success = GenericResult.Success();
        var failure = GenericResult.Failure("Error");

        // Assert
        success.Error.ShouldBe(!success.IsSuccess);
        failure.Error.ShouldBe(!failure.IsSuccess);
    }

    [Fact]
    public void CurrentMessage_ReturnsLastMessage_LIFO()
    {
        // Arrange
        var messages = new[]
        {
            new GenericMessage("First"),
            new GenericMessage("Second"),
            new GenericMessage("Third")
        };

        // Act
        var result = GenericResult.Success(messages);

        // Assert
        result.CurrentMessage.ShouldBe("Third");
    }

    [Fact]
    public void CurrentMessage_ReturnsNull_WhenNoMessages()
    {
        // Act
        var result = GenericResult.Success();

        // Assert
        result.CurrentMessage.ShouldBeNull();
    }

    [Fact]
    public void Messages_ReturnsReadOnlyCollection()
    {
        // Arrange
        var result = GenericResult.Success("Test");

        // Act & Assert
        result.Messages.ShouldBeAssignableTo<IReadOnlyList<IGenericMessage>>();
    }

    #endregion

    #region Generic GenericResult<T> Success Tests

    [Fact]
    public void GenericOfT_Success_WithValue_CreatesSuccessResultWithValue()
    {
        // Arrange
        const int value = 42;

        // Act
        var result = GenericResult<int>.Success(value);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.IsEmpty.ShouldBeFalse();
        result.Value.ShouldBe(value);
        result.Messages.ShouldBeEmpty();
    }

    [Fact]
    public void GenericOfT_Success_WithValueAndStringMessage_CreatesSuccessResultWithValueAndMessage()
    {
        // Arrange
        const string value = "test";
        const string message = "Success message";

        // Act
        var result = GenericResult<string>.Success(value, message);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(value);
        result.CurrentMessage.ShouldBe(message);
        result.Messages.Count.ShouldBe(1);
    }

    [Fact]
    public void GenericOfT_Success_WithValueAndIGenericMessage_CreatesSuccessResultWithValueAndMessage()
    {
        // Arrange
        const double value = 3.14;
        IGenericMessage message = new GenericMessage("Pi calculated");

        // Act
        var result = GenericResult<double>.Success(value, message);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(value);
        result.CurrentMessage.ShouldBe(message.Message);
        result.Messages[0].ShouldBe(message);
    }

    [Fact]
    public void GenericOfT_Success_WithValueAndGenericMessage_CreatesSuccessResultWithValueAndMessage()
    {
        // Arrange
        const bool value = true;
        var message = new GenericMessage("Operation succeeded");

        // Act
        var result = GenericResult<bool>.Success<GenericMessage>(value, message);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(value);
        result.CurrentMessage.ShouldBe(message.Message);
    }

    [Fact]
    public void GenericOfT_Success_WithValueAndMessageCollection_CreatesSuccessResultWithValueAndMultipleMessages()
    {
        // Arrange
        var value = new List<int> { 1, 2, 3 };
        var messages = new List<IGenericMessage>
        {
            new GenericMessage("Step 1 complete"),
            new GenericMessage("Step 2 complete")
        };

        // Act
        var result = GenericResult<List<int>>.Success(value, messages);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(value);
        result.Messages.Count.ShouldBe(2);
        result.CurrentMessage.ShouldBe("Step 2 complete");
    }

    [Fact]
    public void GenericOfT_Success_WithValueAndParamsMessages_CreatesSuccessResultWithValueAndMultipleMessages()
    {
        // Arrange
        const int value = 100;
        var msg1 = new GenericMessage("Message 1");
        var msg2 = new GenericMessage("Message 2");

        // Act
        var result = GenericResult<int>.Success(value, msg1, msg2);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(value);
        result.Messages.Count.ShouldBe(2);
    }

    [Fact]
    public void GenericOfT_Success_WithNullValue_StoresNullValue()
    {
        // Act
        var result = GenericResult<string?>.Success(null);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.IsEmpty.ShouldBeFalse(); // Has value (even if null) because IsSuccess
    }

    #endregion

    #region Generic GenericResult<T> Failure Tests

    [Fact]
    public void GenericOfT_Failure_WithStringMessage_CreatesFailureResult()
    {
        // Arrange
        const string message = "Operation failed";

        // Act
        var result = GenericResult<int>.Failure(message);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.IsEmpty.ShouldBeTrue();
        result.CurrentMessage.ShouldBe(message);
        Should.Throw<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void GenericOfT_Failure_WithIGenericMessage_CreatesFailureResult()
    {
        // Arrange
        IGenericMessage message = new GenericMessage("Error occurred");

        // Act
        var result = GenericResult<string>.Failure(message);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.CurrentMessage.ShouldBe(message.Message);
        Should.Throw<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void GenericOfT_Failure_WithGenericMessage_CreatesFailureResult()
    {
        // Arrange
        var message = new GenericMessage("Validation failed");

        // Act
        var result = GenericResult<double>.Failure<GenericMessage>(message);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.CurrentMessage.ShouldBe(message.Message);
    }

    [Fact]
    public void GenericOfT_Failure_WithMessageCollection_CreatesFailureResultWithMultipleMessages()
    {
        // Arrange
        var messages = new List<IGenericMessage>
        {
            new GenericMessage("Error 1"),
            new GenericMessage("Error 2")
        };

        // Act
        var result = GenericResult<bool>.Failure(messages);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Messages.Count.ShouldBe(2);
        result.CurrentMessage.ShouldBe("Error 2");
    }

    [Fact]
    public void GenericOfT_Failure_WithParamsMessages_CreatesFailureResultWithMultipleMessages()
    {
        // Arrange
        var msg1 = new GenericMessage("Error 1");
        var msg2 = new GenericMessage("Error 2");

        // Act
        var result = GenericResult<int>.Failure(msg1, msg2);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Messages.Count.ShouldBe(2);
    }

    [Fact]
    public void GenericOfT_Failure_GenericStaticMethod_CreatesFailureResultOfSpecifiedType()
    {
        // Arrange
        const string message = "Generic failure";

        // Act
        var result = GenericResult<int>.Failure<string>(message);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.CurrentMessage.ShouldBe(message);
        Should.Throw<InvalidOperationException>(() => result.Value);
    }

    #endregion

    #region Generic GenericResult<T> Value Property Tests

    [Fact]
    public void GenericOfT_Value_WhenSuccess_ReturnsValue()
    {
        // Arrange
        const int expectedValue = 42;
        var result = GenericResult<int>.Success(expectedValue);

        // Act
        var actualValue = result.Value;

        // Assert
        actualValue.ShouldBe(expectedValue);
    }

    [Fact]
    public void GenericOfT_Value_WhenFailure_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = GenericResult<int>.Failure("Error");

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => result.Value);
        exception.Message.ShouldBe("Cannot access value of a failed result.");
    }

    [Fact]
    public void GenericOfT_IsEmpty_WhenSuccess_ReturnsFalse()
    {
        // Arrange
        var result = GenericResult<int>.Success(42);

        // Act & Assert
        result.IsEmpty.ShouldBeFalse();
    }

    [Fact]
    public void GenericOfT_IsEmpty_WhenFailure_ReturnsTrue()
    {
        // Arrange
        var result = GenericResult<int>.Failure("Error");

        // Act & Assert
        result.IsEmpty.ShouldBeTrue();
    }

    #endregion

    #region Generic GenericResult<T> Map Tests

    [Fact]
    public void GenericOfT_Map_WhenSuccess_TransformsValue()
    {
        // Arrange
        var result = (GenericResult<int>)GenericResult<int>.Success(5);

        // Act
        var mapped = result.Map(x => x * 2);

        // Assert
        mapped.IsSuccess.ShouldBeTrue();
        mapped.Value.ShouldBe(10);
    }

    [Fact]
    public void GenericOfT_Map_WhenSuccess_CanChangeType()
    {
        // Arrange
        var result = (GenericResult<int>)GenericResult<int>.Success(42);

        // Act
        var mapped = result.Map(x => x.ToString());

        // Assert
        mapped.IsSuccess.ShouldBeTrue();
        mapped.Value.ShouldBe("42");
    }

    [Fact]
    public void GenericOfT_Map_WhenFailure_ReturnsFailureWithSameMessage()
    {
        // Arrange
        const string errorMessage = "Original error";
        var result = (GenericResult<int>)GenericResult<int>.Failure(errorMessage);

        // Act
        var mapped = result.Map(x => x * 2);

        // Assert
        mapped.IsSuccess.ShouldBeFalse();
        mapped.CurrentMessage.ShouldBe(errorMessage);
    }

    [Fact]
    public void GenericOfT_Map_WithNullMapper_ThrowsArgumentNullException()
    {
        // Arrange
        var result = (GenericResult<int>)GenericResult<int>.Success(42);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => result.Map<string>(null!));
    }

    [Fact]
    public void GenericOfT_Map_PreservesFailureState()
    {
        // Arrange
        var result = (GenericResult<int>)GenericResult<int>.Failure("Error 1");

        // Act
        var mapped = result.Map(x => x.ToString());

        // Assert
        mapped.IsSuccess.ShouldBeFalse();
        mapped.IsFailure.ShouldBeTrue();
        mapped.Error.ShouldBeTrue();
    }

    [Fact]
    public void GenericOfT_Map_WhenFailureWithNoMessage_UsesDefaultMessage()
    {
        // Arrange
        var result = (GenericResult<int>)GenericResult<int>.Failure(new GenericMessage(null!));

        // Act
        var mapped = result.Map(x => x * 2);

        // Assert
        mapped.IsSuccess.ShouldBeFalse();
        mapped.CurrentMessage.ShouldBe("Operation failed");
    }

    #endregion

    #region Generic GenericResult<T> Match Tests

    [Fact]
    public void GenericOfT_Match_WhenSuccess_CallsSuccessFunction()
    {
        // Arrange
        var result = (GenericResult<int>)GenericResult<int>.Success(42);
        var successCalled = false;
        var failureCalled = false;

        // Act
        var matchResult = result.Match(
            value => { successCalled = true; return value * 2; },
            error => { failureCalled = true; return 0; }
        );

        // Assert
        successCalled.ShouldBeTrue();
        failureCalled.ShouldBeFalse();
        matchResult.ShouldBe(84);
    }

    [Fact]
    public void GenericOfT_Match_WhenFailure_CallsFailureFunction()
    {
        // Arrange
        const string errorMessage = "Error occurred";
        var result = (GenericResult<int>)GenericResult<int>.Failure(errorMessage);
        var successCalled = false;
        var failureCalled = false;

        // Act
        var matchResult = result.Match(
            value => { successCalled = true; return "Success"; },
            error => { failureCalled = true; return $"Failed: {error}"; }
        );

        // Assert
        successCalled.ShouldBeFalse();
        failureCalled.ShouldBeTrue();
        matchResult.ShouldBe($"Failed: {errorMessage}");
    }

    [Fact]
    public void GenericOfT_Match_WithNullSuccessFunction_ThrowsArgumentNullException()
    {
        // Arrange
        var result = (GenericResult<int>)GenericResult<int>.Success(42);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            result.Match<string>(null!, error => "Error"));
    }

    [Fact]
    public void GenericOfT_Match_WithNullFailureFunction_ThrowsArgumentNullException()
    {
        // Arrange
        var result = (GenericResult<int>)GenericResult<int>.Success(42);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            result.Match(value => "Success", null!));
    }

    [Fact]
    public void GenericOfT_Match_PassesCorrectValueToSuccessFunction()
    {
        // Arrange
        const int expectedValue = 123;
        var result = (GenericResult<int>)GenericResult<int>.Success(expectedValue);

        // Act
        var capturedValue = 0;
        result.Match(
            value => { capturedValue = value; return value; },
            error => 0
        );

        // Assert
        capturedValue.ShouldBe(expectedValue);
    }

    [Fact]
    public void GenericOfT_Match_PassesCorrectErrorToFailureFunction()
    {
        // Arrange
        const string expectedError = "Test error";
        var result = (GenericResult<int>)GenericResult<int>.Failure(expectedError);

        // Act
        var capturedError = string.Empty;
        result.Match(
            value => "Success",
            error => { capturedError = error; return error; }
        );

        // Assert
        capturedError.ShouldBe(expectedError);
    }

    [Fact]
    public void GenericOfT_Match_CanReturnDifferentType()
    {
        // Arrange
        var successResult = (GenericResult<int>)GenericResult<int>.Success(42);
        var failureResult = (GenericResult<int>)GenericResult<int>.Failure("Error");

        // Act
        var successMatch = successResult.Match(
            value => value.ToString(),
            error => "0"
        );
        var failureMatch = failureResult.Match(
            value => value.ToString(),
            error => error
        );

        // Assert
        successMatch.ShouldBe("42");
        failureMatch.ShouldBe("Error");
    }

    [Fact]
    public void GenericOfT_Match_WhenFailureWithNoMessage_PassesEmptyString()
    {
        // Arrange
        var result = (GenericResult<int>)GenericResult<int>.Failure(new GenericMessage(null!));

        // Act
        var capturedError = "not set";
        result.Match(
            value => "Success",
            error => { capturedError = error; return error; }
        );

        // Assert
        capturedError.ShouldBe(string.Empty);
    }

    #endregion

    #region Generic GenericResult<T> Complex Type Tests

    [Fact]
    public void GenericOfT_WithComplexType_WorksCorrectly()
    {
        // Arrange
        var person = new TestPerson { Name = "John", Age = 30 };

        // Act
        var result = GenericResult<TestPerson>.Success(person);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(person);
        result.Value.ShouldNotBeNull();
        result.Value.Name.ShouldBe("John");
        result.Value.Age.ShouldBe(30);
    }

    [Fact]
    public void GenericOfT_Map_WithComplexTypes_TransformsCorrectly()
    {
        // Arrange
        var person = new TestPerson { Name = "Jane", Age = 25 };
        var result = (GenericResult<TestPerson>)GenericResult<TestPerson>.Success(person);

        // Act
        var mapped = result.Map(p => $"{p.Name} is {p.Age} years old");

        // Assert
        mapped.IsSuccess.ShouldBeTrue();
        mapped.Value.ShouldBe("Jane is 25 years old");
    }

    private class TestPerson
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    #endregion
}

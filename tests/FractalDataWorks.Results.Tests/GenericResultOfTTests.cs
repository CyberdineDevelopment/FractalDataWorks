using FractalDataWorks.Messages;
using FractalDataWorks.Results;

namespace FractalDataWorks.Results.Tests;

/// <summary>
/// Tests for the generic GenericResult&lt;T&gt; class.
/// </summary>
public class GenericResultOfTTests
{
    #region Success Tests

    [Fact]
    public void Success_WithValue_CreatesSuccessResultWithValue()
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
    public void Success_WithValueAndStringMessage_CreatesSuccessResultWithValueAndMessage()
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
    public void Success_WithValueAndIGenericMessage_CreatesSuccessResultWithValueAndMessage()
    {
        // Arrange
        const double value = 3.14;
        var message = new RecMessage("Pi calculated");

        // Act
        var result = GenericResult<double>.Success(value, message);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(value);
        result.CurrentMessage.ShouldBe(message.Message);
        result.Messages[0].ShouldBe(message);
    }

    [Fact]
    public void Success_WithValueAndGenericMessage_CreatesSuccessResultWithValueAndMessage()
    {
        // Arrange
        const bool value = true;
        var message = new RecMessage("Operation succeeded");

        // Act
        var result = GenericResult<bool>.Success<RecMessage>(value, message);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(value);
        result.CurrentMessage.ShouldBe(message.Message);
    }

    [Fact]
    public void Success_WithValueAndMessageCollection_CreatesSuccessResultWithValueAndMultipleMessages()
    {
        // Arrange
        var value = new List<int> { 1, 2, 3 };
        var messages = new List<IGenericMessage>
        {
            new RecMessage("Step 1 complete"),
            new RecMessage("Step 2 complete")
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
    public void Success_WithValueAndParamsMessages_CreatesSuccessResultWithValueAndMultipleMessages()
    {
        // Arrange
        const int value = 100;
        var msg1 = new RecMessage("Message 1");
        var msg2 = new RecMessage("Message 2");

        // Act
        var result = GenericResult<int>.Success(value, msg1, msg2);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(value);
        result.Messages.Count.ShouldBe(2);
    }

    [Fact]
    public void Success_WithNullValue_StoresNullValue()
    {
        // Act
        var result = GenericResult<string?>.Success(null);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.IsEmpty.ShouldBeFalse(); // Has value (even if null) because IsSuccess
    }

    #endregion

    #region Failure Tests

    [Fact]
    public void Failure_WithStringMessage_CreatesFailureResult()
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
    public void Failure_WithIGenericMessage_CreatesFailureResult()
    {
        // Arrange
        var message = new RecMessage("Error occurred");

        // Act
        var result = GenericResult<string>.Failure(message);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.CurrentMessage.ShouldBe(message.Message);
        Should.Throw<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Failure_WithGenericMessage_CreatesFailureResult()
    {
        // Arrange
        var message = new RecMessage("Validation failed");

        // Act
        var result = GenericResult<double>.Failure<RecMessage>(message);

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
            new RecMessage("Error 1"),
            new RecMessage("Error 2")
        };

        // Act
        var result = GenericResult<bool>.Failure(messages);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Messages.Count.ShouldBe(2);
        result.CurrentMessage.ShouldBe("Error 2");
    }

    [Fact]
    public void Failure_WithParamsMessages_CreatesFailureResultWithMultipleMessages()
    {
        // Arrange
        var msg1 = new RecMessage("Error 1");
        var msg2 = new RecMessage("Error 2");

        // Act
        var result = GenericResult<int>.Failure(msg1, msg2);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Messages.Count.ShouldBe(2);
    }

    [Fact]
    public void Failure_GenericStaticMethod_CreatesFailureResultOfSpecifiedType()
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

    #region Value Property Tests

    [Fact]
    public void Value_WhenSuccess_ReturnsValue()
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
    public void Value_WhenFailure_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = GenericResult<int>.Failure("Error");

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() => result.Value);
        exception.Message.ShouldBe("Cannot access value of a failed result.");
    }

    [Fact]
    public void IsEmpty_WhenSuccess_ReturnsFalse()
    {
        // Arrange
        var result = GenericResult<int>.Success(42);

        // Act & Assert
        result.IsEmpty.ShouldBeFalse();
    }

    [Fact]
    public void IsEmpty_WhenFailure_ReturnsTrue()
    {
        // Arrange
        var result = GenericResult<int>.Failure("Error");

        // Act & Assert
        result.IsEmpty.ShouldBeTrue();
    }

    #endregion

    #region Map Tests

    [Fact]
    public void Map_WhenSuccess_TransformsValue()
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
    public void Map_WhenSuccess_CanChangeType()
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
    public void Map_WhenFailure_ReturnsFailureWithSameMessage()
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
    public void Map_WithNullMapper_ThrowsArgumentNullException()
    {
        // Arrange
        var result = (GenericResult<int>)GenericResult<int>.Success(42);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => result.Map<string>(null!));
    }

    [Fact]
    public void Map_PreservesFailureState()
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
    public void Map_WhenFailureWithNoMessage_UsesDefaultMessage()
    {
        // Arrange
        var result = (GenericResult<int>)GenericResult<int>.Failure(new RecMessage(null!));

        // Act
        var mapped = result.Map(x => x * 2);

        // Assert
        mapped.IsSuccess.ShouldBeFalse();
        mapped.CurrentMessage.ShouldBe("Operation failed");
    }

    #endregion

    #region Match Tests

    [Fact]
    public void Match_WhenSuccess_CallsSuccessFunction()
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
    public void Match_WhenFailure_CallsFailureFunction()
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
    public void Match_WithNullSuccessFunction_ThrowsArgumentNullException()
    {
        // Arrange
        var result = (GenericResult<int>)GenericResult<int>.Success(42);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            result.Match<string>(null!, error => "Error"));
    }

    [Fact]
    public void Match_WithNullFailureFunction_ThrowsArgumentNullException()
    {
        // Arrange
        var result = (GenericResult<int>)GenericResult<int>.Success(42);

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            result.Match(value => "Success", null!));
    }

    [Fact]
    public void Match_PassesCorrectValueToSuccessFunction()
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
    public void Match_PassesCorrectErrorToFailureFunction()
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
    public void Match_CanReturnDifferentType()
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
    public void Match_WhenFailureWithNoMessage_PassesEmptyString()
    {
        // Arrange
        var result = (GenericResult<int>)GenericResult<int>.Failure(new RecMessage(null!));

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

    #region Complex Type Tests

    [Fact]
    public void GenericResult_WithComplexType_WorksCorrectly()
    {
        // Arrange
        var person = new TestPerson { Name = "John", Age = 30 };

        // Act
        var result = GenericResult<TestPerson>.Success(person);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(person);
        result.Value.Name.ShouldBe("John");
        result.Value.Age.ShouldBe(30);
    }

    [Fact]
    public void Map_WithComplexTypes_TransformsCorrectly()
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
